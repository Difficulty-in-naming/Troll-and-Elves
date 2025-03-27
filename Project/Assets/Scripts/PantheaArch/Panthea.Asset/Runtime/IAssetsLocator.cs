using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Panthea.Asset
{
    public interface IAssetsLocator
    {
        UniTask<object> Load([MustLower]string path, Type type,CancellationToken token = default);
        UniTask<T> Load<T>([MustLower]string path,CancellationToken token = default);
        UniTask<Dictionary<string, List<object>>> LoadAll([MustLower]string path);
        UniTask<ResPak> LoadAssetBundle([MustLower]string path);
        void UnloadAssetBundle([MustLower]string path,bool releaseAsset);
        void UnloadAssetBundle(ResPak pak,bool releaseAsset);
        void ReleaseInstance<TObject>(TObject obj);
        void ReleaseAllAssetBundle();
        List<string> GetFilterAssetBundle(string[] path);
        string[] GetDependencies([MustLower]string abPath);
        Dictionary<string, ResPak> GetLoadedAssetBundle();
        string[] GetAllNameInBundle([MustLower]string abPath);
        AssetFileLog FindAsset([MustLower]string filePath);
        Shader GetShader([MustLower]string shaderName);
        List<AssetFileLog> FetchDownloadList();
        UniTask Download(List<string> path, IProgress<float> progress = null);
    }
}