using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Panthea.Asset
{
    public class AssetBundleRuntime
    {
        private readonly IAssetBundlePool mPool;

        public AssetBundleRuntime(IAssetBundlePool pool) => mPool = pool;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReleaseInstance<T>(T instance) => AssetsManager.Router.Get(instance).SubReferenceCount(instance);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnloadAllAssetBundle() => mPool.UnloadAllAssetBundle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<string, ResPak> GetLoadedAssetBundle() => mPool.GetLoadedAssetBundle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReleaseAssetBundle(string path,bool releaseAssets = true) => mPool.Release(path,releaseAssets);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReleaseAssetBundle(ResPak pak,bool releaseAssets = true) => mPool.Release(pak,releaseAssets);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<T> LoadAsset<T>(string path, CancellationToken token = default) => (T)await LoadAsset(path, typeof(T), token);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<object> LoadAsset(string path,Type type, CancellationToken token = default) => (await Internal_LoadAssetAsync(path, type, token)).Asset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<Dictionary<string, List<object>>> LoadAllAsset(string path) => await (await LoadAssetBundle(path)).LoadAll();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<ResPak> LoadAssetBundle(string path, CancellationToken token = default) => await mPool.GetAsync(path, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTask<LoadAssetResult> Internal_LoadAssetAsync(string path,Type type, CancellationToken token = default)
        {
            var ab = await LoadAssetBundle(path, token);
            var asset = await ab.Load(path,type);
            ab.AddReferenceCount(asset);
            AssetsManager.Router.Add(asset, ab);
            return new LoadAssetResult(ab, asset);
        }
    }
}