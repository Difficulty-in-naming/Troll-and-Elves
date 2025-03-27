using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Panthea.Common;
using Panthea.Utils;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

#if USEJSON
#elif USE_MEMORYPACK
using MemoryPack;
#endif

namespace Panthea.Asset
{
    public class GenerateFileLog : IBuildTask
    {
        [InjectContext(ContextUsage.In)] private IBundleBuildContent m_Content;
        [InjectContext] private IBundleBuildResults m_Results;

        private string mStreamingAssets = AssetsConfig.StreamingAssets;
        private readonly Dictionary<string, AssetFileLog> mSave = new Dictionary<string, AssetFileLog>();

        public ReturnCode Run()
        {
            var outputFolder = BuildPreference.Instance.OutputPath;
            var bundleMap = BuildPreference.Instance.Groups.ToDictionary(t1 => t1.name.Replace("-", "/") + AssetsConfig.Suffix, t2 => t2);

            foreach (var node in m_Content.BundleLayout)
            {
                string[] files = new string[node.Value.Count];
                if (bundleMap.TryGetValue(node.Key, out var value))
                {
                    for (var index = 0; index < node.Value.Count; index++)
                    {
                        var info = value.Files[index];
                        if (!string.IsNullOrEmpty(info.Address))
                            files[index] = info.Address.ToLower();
                        else
                        {
                            string file = PathUtils.FormatFilePath(AssetDatabase.GUIDToAssetPath(info.GUID));
                            string path = PathUtils.RemoveFileExtension(file.Replace("Assets/Res/", ""));
                            files[index] = path.ToLower();
                        }
                    }
                }
                else
                {
                    for (var index = 0; index < node.Value.Count; index++)
                    {
                        GUID sub = node.Value[index];
                        string file = PathUtils.FormatFilePath(AssetDatabase.GUIDToAssetPath(sub.ToString()));
                        string path = PathUtils.RemoveFileExtension(file.Replace("Assets/Res/", ""));
                        var info = BuildPreference.Instance.GetInfo(file.Replace("Assets/Res/", ""));
                        if (info != null && !string.IsNullOrEmpty(info.Address))
                            files[index] = info.Address.ToLower();
                        else
                            files[index] = path.ToLower();
                    }
                }

                if (m_Results.BundleInfos.TryGetValue(node.Key, out var realBundle))
                {
                    string filePath = realBundle.FileName;
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Exists)
                    {
                        var crc = Crc32CAlgorithm.Compute(fileInfo.OpenRead());
                        var dependencies = m_Results.BundleInfos[node.Key].Dependencies.ToList();
                        dependencies.Remove(node.Key); //不知道为什么会出现自己依赖自己的情况.
                        if (BuildPreference.Instance.AppendHash)
                        {
                            for (var index = 0; index < dependencies.Count; index++)
                            {
                                var originDep = dependencies[index];
                                dependencies[index] = PathUtils.RemoveFileExtension(originDep) + "_" + m_Results.BundleInfos[originDep].Hash + AssetsConfig.Suffix;
                            }
                        }
 
                        var addressName = filePath.Replace(outputFolder + "/", "");
                        var groupName = node.Key;
                        BuildObject group = null;
                        if (node.Key is not (AssetBundleBuilder.BuiltInShaderBundleName or AssetBundleBuilder.BuiltInMonoScriptBundleName))
                        {
                            string key = groupName.Replace(".bundle","");
                            group = BuildPreference.Instance.FindGroup(key);
                            if (group == null)
                            {
                                key = Path.GetDirectoryName(groupName).Replace("\\","/");
                                group = BuildPreference.Instance.FindGroup(key);
                                if (group == null)
                                    Log.Error($"Group:{addressName}不存在");
                            }
                        }
                        
#if WECHAT
                        bool include = false;
#else
                        bool include = false;
                        if (node.Key is AssetBundleBuilder.BuiltInShaderBundleName or AssetBundleBuilder.BuiltInMonoScriptBundleName)
                        {
                            include = true;
                        }
                        else
                        {
                            include = group != null ? group.Include : false;
                        }
#endif
                        mSave.Add(addressName,
                            new AssetFileLog(crc, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), addressName, files, dependencies.ToArray(), 
                                include ? FileState.Include : FileState.NotExists, fileInfo.Length, node.Key,group ? group.RealAddress ?? "" : ""));
                    }
                    else
                    {
                        Debug.Log("找不到文件:" + fileInfo.FullName);
                    }
                }
                else
                {
                    Debug.Log("找不到文件:" + node.Key);
                }

            }

            var json = JsonConvert.SerializeObject(mSave, Formatting.Indented);
            File.WriteAllText("./BuildXAssetBundleReport.json", json);
#if USE_MEMORYPACK 
            var bytes = MemoryPackSerializer.Serialize(mSave);
            File.WriteAllBytes(BuildPreference.Instance.OutputPath + "/all_local_file.bytes", bytes);
#elif USEJSON
            var text = JsonConvert.SerializeObject(mSave, Formatting.None,new JsonSerializerSettings{DefaultValueHandling = DefaultValueHandling.Ignore});
            File.WriteAllText(BuildPreference.Instance.OutputPath + "/all_local_file.bytes", text);
#endif
            return ReturnCode.Success;
        }

        public int Version => 1;
    }
}