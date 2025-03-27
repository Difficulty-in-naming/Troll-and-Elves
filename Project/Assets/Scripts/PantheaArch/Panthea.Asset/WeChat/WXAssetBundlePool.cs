using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if WECHAT
namespace Panthea.Asset.WeChat
{
    public class WXAssetBundlePool : IAssetBundlePool
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

        public WXAssetBundlePool(IABFileTrack fileLog, AssetBundleDownloader downloader)
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
        private BundleFileStatus GetFileLog(string path)
        {
            if (!path.EndsWith(AssetsConfig.Suffix, StringComparison.OrdinalIgnoreCase))
            {
                var fileLog = mFileLog.GetDownloadedFile(path);
                if (fileLog != null)
                    return new BundleFileStatus(fileLog, true);

                fileLog = mFileLog.GetRemoteBundleFromFileName(path);
                return fileLog == null ? throw new System.Exception($"找不到文件{path}") : new BundleFileStatus(fileLog, true);
            }
            else
            {
                var fileLog = mFileLog.GetDownloadedBundle(path);
                if (fileLog != null)
                    return new BundleFileStatus(fileLog, true);

                fileLog = mFileLog.GetRemoteBundle(path);
                return fileLog == null ? throw new System.Exception($"找不到文件{path}") : new BundleFileStatus(fileLog, true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTask<ResPak> internal_Load(BundleFileStatus redirectAsset, CancellationToken token = default)
        {
            string path = redirectAsset.Asset.Path;
            AssetBundle assetBundle = null;

            mWaitList.TryAdd(path,ListPool<Action>.Create());

            var dep = LoadDependencies(redirectAsset);

            var keys = ListPool<string>.Create();
            keys.Add(path);
            assetBundle = (await mDownloader.Download(keys))[0];
            keys.Dispose();
            
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
            var fileLog = GetFileLog(path);
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

            var ab = internal_Get(fileLog.Asset.Path);
            return ab ?? await internal_Load(fileLog, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(string path, bool releaseAssets = true)
        {
            var fileLog = GetFileLog(path);
            var ab = internal_Get(fileLog.Asset.Path);
            Release(ab,releaseAssets);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(ResPak ab, bool releaseAssets = true)
        {
            //todo 暂未实现
            return;
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
#endif