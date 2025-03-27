using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Panthea.Common;
using UnityEngine;

namespace Panthea.Asset
{
    public class AssetsManager: IAssetsLocator
    {
        private AssetBundleDownloader ABDownloader;
        private AssetBundleRuntime Runtime;
        private IABFileTrack mAbFileTrack;
        private readonly string[] mEmptyStringArray = { };
        public static AssetRouter Router { get; private set; } = new();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Purge() => Router = new AssetRouter();
#endif
        
        public AssetsManager(IABFileTrack fileTrack, AssetBundleRuntime runtime, AssetBundleDownloader downloader)
        {
            mAbFileTrack = fileTrack;
            Runtime = runtime;
            ABDownloader = downloader;
        }

        /// <summary>
        /// 根据本地列表检测需要下载得文件内容
        /// </summary>
        /// <returns></returns>
        public List<AssetFileLog> FetchDownloadList() => ABDownloader.FetchDownloadList();

        public UniTask Download(List<string> path, IProgress<float> progress = null) => ABDownloader.Download(path, progress);

        public UniTask<object> Load(string path, Type type, CancellationToken token = default)
        {
            try
            {
                return Runtime.LoadAsset(path,type, token);
            }
            catch (System.Exception e)
            {
                Log.Error(e);
                return default;
            }
        }

        public UniTask<T> Load<T>(string path, CancellationToken token = default)
        {
            try
            {
                return Runtime.LoadAsset<T>(path, token);
            }
            catch (System.Exception e)
            {
                Log.Error(e);
                return default;
            }
        }

        public UniTask<Dictionary<string, List<object>>> LoadAll(string abPath)
        {
            try
            {
                return Runtime.LoadAllAsset(abPath);
            }
            catch (System.Exception e)
            {
                Log.Error(e);
                return default;
            }
        }

        public UniTask<ResPak> LoadAssetBundle(string filePath)
        {
            try
            {
                return Runtime.LoadAssetBundle(filePath);
            }
            catch (System.Exception e)
            {
                Log.Error(e);
                return default;
            }
        }

        public void UnloadAssetBundle(string filePath,bool releaseAssets = true) => Runtime.ReleaseAssetBundle(filePath,releaseAssets);

        public void UnloadAssetBundle(ResPak pak, bool releaseAsset = true) => Runtime.ReleaseAssetBundle(pak, releaseAsset);

        public void ReleaseInstance<TObject>(TObject obj) => Runtime.ReleaseInstance(obj);

        public void ReleaseAllAssetBundle() => Runtime.UnloadAllAssetBundle();

        public List<string> GetFilterAssetBundle(string[] path)
        {
            return mAbFileTrack.GetFilterAssetBundle(path);
        }

        public string[] GetDependencies(string abPath)
        {
            var info = mAbFileTrack.GetDownloadedBundle(abPath);
            if (info != null)
            {
                return info.Dependencies;
            }

            return null;
        }

        public Dictionary<string, ResPak> GetLoadedAssetBundle() => Runtime.GetLoadedAssetBundle();

        public string[] GetAllNameInBundle(string abPath)
        {
            var abInfo = mAbFileTrack.GetDownloadedBundle(abPath + AssetsConfig.Suffix);
            if (abInfo != null)
                return abInfo.Files ?? mEmptyStringArray;
            else
                return mEmptyStringArray;
        }

        public AssetFileLog FindAsset(string filePath)
        {
            return mAbFileTrack.GetRemoteBundleFromFileName(filePath);
        }

        public Shader GetShader(string shaderName)
        {
            // return LoadSync<Shader>("Shader/" + shaderName);
            return null;
        }
    }
}