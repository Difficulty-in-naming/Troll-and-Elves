#pragma warning disable CS1998
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Panthea.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Panthea.Asset
{
    public class EDITOR_AssetsManager: IAssetsLocator
    {
        private readonly Dictionary<string, Dictionary<Type, Object>> mCached = new();
        public const string PackPath = "Res/";
        public const string SearchPath = "Assets/Res/";
        public async UniTask<object> Load(string path, Type type,CancellationToken token = default)
        {
            if (type == typeof(GameObject))
            {
                throw new System.Exception($"你无法通过{path}加载类型为GameObject的实例,请使用AssetObject代替类型加载实例");
            }

            Type mapType = type;
            if (type == typeof(AssetObject))
            {
                mapType = typeof(GameObject);
            }
            
            object asset = null;
            if (mCached.TryGetValue(path, out var getter))
            {
                if (getter.TryGetValue(mapType, out var result))
                {
                    if (result != null)
                    {
                        asset = result;
                    }
                }
            }
            else
            {
                string dir = Path.Combine(SearchPath, PathUtils.FormatFilePath(Path.GetDirectoryName(path)));
                var allAssetGuids = AssetDatabase.FindAssets("t:" + mapType.Name, new[] { dir });
                for (int i = 0; i < allAssetGuids.Length; i++)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(allAssetGuids[i]);
                    int lastIndexOf = assetPath.IndexOf(SearchPath, StringComparison.Ordinal);
                    if (lastIndexOf == -1)
                    {
                        continue;
                    }

                    string tempAssetPath = assetPath.Substring(lastIndexOf + SearchPath.Length);
                    tempAssetPath = PathUtils.RemoveFileExtension(tempAssetPath);
                    tempAssetPath = tempAssetPath.ToLower();
                    if (tempAssetPath == path)
                    {
                        var obj = AssetDatabase.LoadAssetAtPath(assetPath,mapType);
                        if (!mCached.TryGetValue(path, out var dict))
                        {
                            dict = mCached[path] = new Dictionary<Type, Object>();
                        }

                        dict[mapType] = obj;
                        asset = obj;
                        break;
                    }
                }
            }

            if(type == typeof(AssetObject))
            {
                if(asset is GameObject go)
                    return new AssetObject(go,null);
                if(asset != null)
                    throw new System.Exception($"你无法通过{path}加载类型为AssetObject的实例,因为对象实例的类型是{asset.GetType()}");
            }

            return asset;
        }

        public async UniTask<T> Load<T>(string path,CancellationToken token = default) => (T)await Load(path, typeof(T));

        public UniTask<Dictionary<string, List<object>>> LoadAll(string path)
        {
            throw new System.Exception("禁止调用LoadAll,会严重导致内存问题和依赖引用出现问题");
        }

        public async UniTask<ResPak> LoadAssetBundle(string path) => null;
        public void UnloadAssetBundle(string path,bool releaseAssets) { }
        public void UnloadAssetBundle(ResPak pak, bool releaseAsset)
        {
        }

        public void ReleaseInstance<TObject>(TObject obj) { }
        public void ReleaseAllAssetBundle() { }
        public List<string> GetFilterAssetBundle(string[] path) => new(path);
        public string[] GetDependencies(string path) => Array.Empty<string>();
        public Dictionary<string, ResPak> GetLoadedAssetBundle() => new();
        public AssetFileLog FindAsset(string filePath) => throw new NotImplementedException();
        public bool Exists(string filePath) => SpExists(filePath);
        public UniTask Download(List<string> path, IProgress<float> progress = null) => UniTask.CompletedTask;
        public List<AssetFileLog> FetchDownloadList() => new();

        public string[] GetAllNameInBundle(string abPath)
        {
            DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/" + PackPath + abPath.Replace(AssetsConfig.Suffix, ""));
            if (!dir.Exists)
                return Array.Empty<string>();
            List<string> files = new List<string>();
            foreach (var node in dir.GetFiles())
            {
                var p = PathUtils.FullPathToUnityPath(node.FullName);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(p);
                if (obj != null)
                {
                    files.Add(PathUtils.RemoveFileExtension(PathUtils.FullPathToUnityPath(node.FullName).Remove(0, SearchPath.Length)));
                }
            }

            return files.ToArray();
        }

        public Shader GetShader(string shaderName)
        {
            var shader = Load<Shader>("Shader/" + shaderName);
            shader.Forget();
            return shader.AsTask().Result;
        }

        //主要给编辑器用的.不需要New Editor_AssetsManager就可以检测文件是否合法
        public static bool SpExists(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            filePath = filePath.Remove(filePath.Length - fileName.Length);
            var dir = Application.dataPath + "/Res/" + filePath;
            if (Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir,fileName + ".*");
                return files.Length != 0;
            }

            return false;
        }
    }
}
#endif
#pragma warning restore CS1998