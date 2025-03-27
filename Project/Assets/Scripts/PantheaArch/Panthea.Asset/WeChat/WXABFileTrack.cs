using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;

namespace Panthea.Asset
{
    public class WXABFileTrack : IABFileTrack
    {
        private Dictionary<string, AssetFileLog> RemoteBundle { get; set; }
        private Dictionary<string, AssetFileLog> RemoteFileMapBundleName { get; set; }
        private Dictionary<string, AssetFileLog> LookupShortPathRemoteBundle { get; set; }
        private IDownloadPlatform DownloadPlatform { get; set; }
        public string FileLogName { get; }
        private bool IsDownloadingRemoteConfig { get; set; }

        public WXABFileTrack(string fileLogName = null)
        {
            FileLogName = fileLogName;
            LookupShortPathRemoteBundle = new Dictionary<string, AssetFileLog>();
            Init();
        }

        public IABFileTrack ConfigureDownloadPlatform(IDownloadPlatform platform)
        {
            DownloadPlatform = platform;
            return this;
        }

        private void Init() { /*微信不需要这个接口*/ }

        public async UniTask Initialize()
        {
            if (DownloadPlatform == null) return;

            while (IsDownloadingRemoteConfig) await UniTask.NextFrame();

            IsDownloadingRemoteConfig = true;
#if USE_MEMORYPACK
            var content = await DownloadPlatform.GetBytes(FileLogName);
            RemoteBundle = MemoryPack.MemoryPackSerializer.Deserialize<Dictionary<string, AssetFileLog>>(content);
#elif USEJSON
            var content = await DownloadPlatform.GetText(FileLogName);
            RemoteBundle = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, AssetFileLog>>(content);
            var g = Newtonsoft.Json.JsonConvert.SerializeObject(RemoteBundle);
#endif
            RemoteFileMapBundleName = new Dictionary<string, AssetFileLog>(2048);
            foreach (var node in RemoteBundle)
            {
                if (node.Value.Files != null)
                {
                    foreach (var f in node.Value.Files)
                    {
                        RemoteFileMapBundleName[f] = node.Value;
                    }
                }
                LookupShortPathRemoteBundle.TryAdd(node.Value.ShortBundlePath, node.Value);
            }

            IsDownloadingRemoteConfig = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<AssetFileLog> CheckUpdateList() => new();/*微信不需要这个接口*/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssetFileLog GetDownloadedFile(string path) => RemoteFileMapBundleName.TryGetValue(path, out var value) ? value : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssetFileLog GetDownloadedBundle(string path) => RemoteBundle.TryGetValue(path, out var value) ? value : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssetFileLog GetRemoteBundle(string path) => RemoteBundle.TryGetValue(path, out var value) ? value : null;

        public AssetFileLog GetRemoteBundleFromFileName(string path)
        {
            if (!RemoteFileMapBundleName.TryGetValue(path, out var bundle)) return null;
            return RemoteBundle.TryGetValue(bundle.Path, out var value) ? value : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateDownloadedAssetInMemory(string path) { /*微信不需要这个接口*/ }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void SyncDownloadedFileLog() { /*微信不需要这个接口*/ }

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