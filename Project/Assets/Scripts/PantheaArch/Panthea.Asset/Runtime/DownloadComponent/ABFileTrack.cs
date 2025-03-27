using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Panthea.Common;
using UnityEngine;

namespace Panthea.Asset
{
    public class ABFileTrack : IABFileTrack
    {
        private Dictionary<string, AssetFileLog> FileMapBundle { get; }
        private Dictionary<string, AssetFileLog> AllBundle { get; set; }
        private Dictionary<string, AssetFileLog> LookupShortPathAllBundle { get; set; }
        private Dictionary<string, AssetFileLog> RemoteBundle { get; set; }
        private Dictionary<string, string> RemoteFileMapBundleName { get; set; }
        private Dictionary<string, AssetFileLog> LookupShortPathRemoteBundle { get; set; }
        private Dictionary<string, AssetFileLog> CacheWriteDownloadedBundle { get; set; }
        private IDownloadPlatform DownloadPlatform { get; set; }
        public string FileLogName { get; }
        private bool IsDownloadingRemoteConfig { get; set; }
        public ABFileTrack(string fileLogName = null)
        {
            FileLogName = fileLogName;
            FileMapBundle = new Dictionary<string, AssetFileLog>();
            AllBundle = new Dictionary<string, AssetFileLog>();
            LookupShortPathAllBundle = new Dictionary<string, AssetFileLog>();
            LookupShortPathRemoteBundle = new Dictionary<string, AssetFileLog>();
            Init();
        }

        public IABFileTrack ConfigureDownloadPlatform(IDownloadPlatform platform)
        {
            DownloadPlatform = platform;
            return this;
        }

        private void Init()
        {
#if USE_MEMORYPACK
            byte[] content;
#elif USEJSON
            string content;
#endif
            Dictionary<string, AssetFileLog> includeBundle;
            var path = AssetsConfig.AssetBundlePersistentDataPath + FileLogName;
            if (!string.IsNullOrEmpty(FileLogName) && File.Exists(path))
            {
#if USE_MEMORYPACK
                content = File.ReadAllBytes(path);
                AllBundle = MemoryPack.MemoryPackSerializer.Deserialize<Dictionary<string, AssetFileLog>>(content);
#elif USEJSON
                content = File.ReadAllText(path);
                AllBundle = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, AssetFileLog>>(content);
#endif
                
            }

            AllBundle ??= new Dictionary<string, AssetFileLog>();
            CacheWriteDownloadedBundle = new Dictionary<string, AssetFileLog>(AllBundle);

            try
            {
#if USE_MEMORYPACK
                content = BetterStreamingAssets.GetBytes(FileLogName);
                includeBundle = MemoryPack.MemoryPackSerializer.Deserialize<Dictionary<string, AssetFileLog>>(content);
#elif USEJSON
                content = BetterStreamingAssets.GetText(FileLogName);
                includeBundle = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, AssetFileLog>>(content);
#endif
            }
            catch (System.Exception ex)
            {
                Debug.Log($"找不到本地文件:{FileLogName}初始化为空列表\n{ex}");
                includeBundle = new Dictionary<string, AssetFileLog>();
            }

            /*if (includeBundle != null)
            {
                foreach (var node in includeBundle)
                {
                    if (node.Value.State == FileState.NotExists) continue;
                    if (AllBundle.TryGetValue(node.Key, out var downloaded))
                    {
                        if (downloaded.Version > node.Value.Version) continue;
                        AllBundle[node.Key].State = node.Value.State;
                        LookupShortPathAllBundle[node.Value.ShortBundlePath].State = node.Value.State;
                    }
                    else
                    {
                        AllBundle.TryAdd(node.Key, node.Value);
                        LookupShortPathAllBundle.TryAdd(node.Value.ShortBundlePath, node.Value);
                    }
                }
            }*/

            foreach (var node in AllBundle)
            {
                BundleMapToFiles(node.Value);
                LookupShortPathAllBundle.TryAdd(node.Value.ShortBundlePath, node.Value);
            }
        }

        private void BundleMapToFiles(AssetFileLog node)
        {
            if (node.Files != null)
            {
                foreach (var file in node.Files)
                {
                    FileMapBundle[file] = node;
                }
            }
        }

        public async UniTask Initialize()
        {
            if (DownloadPlatform == null) return;

            while(IsDownloadingRemoteConfig) await UniTask.NextFrame();

            IsDownloadingRemoteConfig = true;
            try
            {
#if USE_MEMORYPACK
                var content = await DownloadPlatform.GetBytes(FileLogName);
                RemoteBundle = MemoryPack.MemoryPackSerializer.Deserialize<Dictionary<string, AssetFileLog>>(content);
#elif USEJSON
                var content = await DownloadPlatform.GetText(FileLogName);
                RemoteBundle = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, AssetFileLog>>(content);
#endif
            }
            catch(System.Exception ex)
            {
                RemoteBundle = AllBundle;
                Log.Error(ex);
            }
            
            RemoteFileMapBundleName = new Dictionary<string, string>(2048);
            foreach (var node in RemoteBundle)
            {
                if (node.Value.Files != null)
                {
                    foreach (var f in node.Value.Files)
                    {
                        RemoteFileMapBundleName[f] = node.Key;
                    }
                }

                LookupShortPathRemoteBundle.TryAdd(node.Value.ShortBundlePath, node.Value);
            }

            IsDownloadingRemoteConfig = false;
        }
        
        public List<AssetFileLog> CheckUpdateList()
        {
            var fileList = new List<AssetFileLog>();
            foreach (var network in RemoteBundle)
            {
                if (LookupShortPathAllBundle.TryGetValue(network.Value.ShortBundlePath, out var exist))
                {
                    if (exist.Crc != network.Value.Crc)
                    {
                        fileList.Add(network.Value);
                        if (AssetsConfig.EnsureLatestBundle)
                        {
                            exist.CopyFrom(network.Value);
                            exist.State = FileState.NotExists;
                        }
                    }
                }
            }
            return fileList;
        }

        public AssetFileLog GetDownloadedFile(string path) => FileMapBundle.TryGetValue(path, out var value) ? value : null;

        public AssetFileLog GetDownloadedBundle(string path) => AllBundle.TryGetValue(path, out var value) ? value : null;

        public AssetFileLog GetRemoteBundle(string path) => RemoteBundle.TryGetValue(path, out var value) ? value : null;

        public AssetFileLog GetRemoteBundleFromFileName(string path)
        {
            if (!RemoteFileMapBundleName.TryGetValue(path, out var bundle)) return null;
            return RemoteBundle.TryGetValue(bundle, out var value) ? value : null;
        }

        public void UpdateDownloadedAssetInMemory(string path)
        {
            if (RemoteBundle.TryGetValue(path, out var value))
            {
                if (LookupShortPathAllBundle.TryGetValue(value.ShortBundlePath, out var localBundle))
                {
                    AssetsUtils.DeleteBundle(localBundle.Path);
                    AllBundle[localBundle.Path] = value;
                    CacheWriteDownloadedBundle[localBundle.Path] = value;
                }
                else
                {
                    AllBundle[path] = value;
                    LookupShortPathAllBundle[value.ShortBundlePath] = value;
                    CacheWriteDownloadedBundle[path] = value;
                }
                
                value.State = FileState.Downloaded;
                BundleMapToFiles(value);
            }
        }

        public void SyncDownloadedFileLog()
        {
            string dirName = AssetsConfig.AssetBundlePersistentDataPath;
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
#if USE_MEMORYPACK
            var content = MemoryPack.MemoryPackSerializer.Serialize(CacheWriteDownloadedBundle);
            File.WriteAllBytes(dirName+ "/" + FileLogName, content);
#elif USEJSON
            var content = Newtonsoft.Json.JsonConvert.SerializeObject(CacheWriteDownloadedBundle);
            File.WriteAllText(dirName+ "/" + FileLogName, content);
#endif
        }

        public List<string> GetFilterAssetBundle(string[] path)
        {
            var list = new List<string>();
            foreach (var node in path)
            {
                list.AddRange(from bundle in RemoteBundle where bundle.Key.StartsWith(node, StringComparison.OrdinalIgnoreCase) select bundle.Key);
            }

            return list;
        }
    }
}