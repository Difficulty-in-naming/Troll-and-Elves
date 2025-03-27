using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Panthea.Utils;
using UnityEngine;

namespace Panthea.Asset
{
    /// <summary>
    /// AssetBundle数据缓存池.
    /// </summary>
    public class AssetBundlePool : IAssetBundlePool
    {
        private readonly Dictionary<string, ResPak> mPool = new();
        private readonly Dictionary<ResPak, string> mLookup = new();
        public Dictionary<string, ResPak> GetLoadedAssetBundle() => new(mPool);

        /// <summary>
        /// 等待加载的列表,因为可能在同一帧当中多次请求同一个AB.第二次的时候我们判断是否在等待列表中.如果在的话我们就一直等到等待列表被移除以后返回结果
        /// </summary>
        private readonly ConcurrentDictionary<string,ListPool<Action>> mWaitList = new ();

        private readonly IABFileTrack mFileLog;
        private readonly AssetBundleDownloader mDownloader;

        public AssetBundlePool(IABFileTrack fileLog, AssetBundleDownloader downloader)
        {
            mFileLog = fileLog;
            mDownloader = downloader;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ResPak internal_Get(string path) => mPool.TryGetValue(path, out var value) ? value : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTask LoadDependencies(BundleFileStatus fileStatus)
        {
            var dep = fileStatus.Asset.Dependencies;
            if (dep == null || dep.Length == 0)
                return;
            List<UniTask<ResPak>> parallel = new List<UniTask<ResPak>>();
            foreach (var node in dep)
            {
                parallel.Add(GetAsync(node));
            }

            while (true)
            {
                var pass = parallel.All(node => node.Status.IsCompleted());
                if (pass)
                    break;
                await UniTask.NextFrame();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BundleFileStatus CheckAssetBundleIfNotExistsDownload(string path)
        {
            if (!path.EndsWith(AssetsConfig.Suffix, StringComparison.OrdinalIgnoreCase))
            {
                var fileLog = mFileLog.GetDownloadedFile(path);
                if (fileLog != null)
                    return new BundleFileStatus(fileLog, fileLog.State is FileState.NotExists);

                fileLog = mFileLog.GetRemoteBundleFromFileName(path);
                return fileLog == null ? throw new System.Exception($"找不到文件{path}") : new BundleFileStatus(fileLog, true);
            }
            else
            {
                var fileLog = mFileLog.GetDownloadedBundle(path);
                if (fileLog != null)
                    return new BundleFileStatus(fileLog, fileLog.State is FileState.NotExists);

                fileLog = mFileLog.GetRemoteBundle(path);
                return fileLog == null ? throw new System.Exception($"找不到文件{path}") : new BundleFileStatus(fileLog, true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTask<ResPak> internal_Load(BundleFileStatus redirectAsset,CancellationToken token = default)
        {
            string path = redirectAsset.Asset.Path;
            var address = redirectAsset.Asset.State == FileState.Downloaded || redirectAsset.Asset.State == FileState.NotExists
                ? AssetsConfig.AssetBundlePersistentDataPath + path
                : AssetsConfig.StreamingAssets + path;
            AssetBundle assetBundle = null;

            mWaitList.TryAdd(path,ListPool<Action>.Create());

            var dep = LoadDependencies(redirectAsset);

            if (redirectAsset.NeedDownload || redirectAsset.Asset.State == FileState.NotExists)
            {
                var keys = ListPool<string>.Create();
                keys.Add(path);
                assetBundle = (await mDownloader.Download(keys))[0];
                keys.Dispose();
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException("加载AB操作已取消");
                }
            }
            else
            {
                assetBundle = AssetBundle.LoadFromFile(address);
            }
            
            ResPak pak;
            if (mWaitList.TryGetValue(path, out _))
            {
                pak = new ResPak(assetBundle, redirectAsset.Asset);
                mPool.Add(path, pak);
                mLookup.Add(pak, path);
                mWaitList.TryRemove(path,out var list);
                for (var index = 0; index < list.Count; index++)
                {
                    var node = list[index];
                    node();
                }

                list.Dispose();
            }
            else
            {
                mPool.TryGetValue(path, out pak);
            }

            await dep;

            return pak;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<ResPak> GetAsync(string path, CancellationToken token = default)
        {
            var fileLog = CheckAssetBundleIfNotExistsDownload(path);
            AutoResetUniTaskCompletionSource source = AutoResetUniTaskCompletionSource.Create();
            if (mWaitList.TryGetValue(fileLog.Asset.Path, out var list))
            {
                list.Add(() => source.TrySetResult());
            }
            else
            {
                source.TrySetResult();
            }

            await source.Task;
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException("加载AB操作已取消");
            }
            var ab = internal_Get(fileLog.Asset.Path);
            return ab ?? await internal_Load(fileLog);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(string path, bool releaseAssets = true)
        {
            var fileLog = CheckAssetBundleIfNotExistsDownload(path);
            var ab = internal_Get(fileLog.Asset.Path);
            Release(ab, releaseAssets);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(ResPak ab, bool releaseAssets = true)
        {
            if (ab != null)
            {
                if (releaseAssets)
                {
                    if (mLookup.Remove(ab, out var value))
                    {
                        mPool.Remove(value);
                        ab.Unload(true);
                    }
                }
                else
                {
                    ab.Unload(false);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnloadAllAssetBundle()
        {
            List<ResPak> temp = new List<ResPak>(mLookup.Keys);
            foreach (var node in temp)
            {
                Release(node);
            }
        }
    }
}