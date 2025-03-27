using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Panthea.Asset.Exception;
using Panthea.Common;
using Panthea.Utils;
using UnityEngine;

namespace Panthea.Asset
{
    public class AssetBundleDownloader
    {
        private readonly IABFileTrack mFileLogContext;
        private readonly IDownloadPlatform mDownloadServices;
        public AssetBundleDownloader(IABFileTrack fileLog, IDownloadPlatform platform)
        {
            mFileLogContext = fileLog;
            mDownloadServices = platform;
        }

        /// <summary>
        /// 根据本地列表检测需要下载得文件内容
        /// </summary>
        /// <returns></returns>
        public List<AssetFileLog> FetchDownloadList()
        {
            return mFileLogContext.CheckUpdateList();
        }

        /// <summary>
        /// 检查本地是否存在这个文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        private string HasExist(string path)
        {
            var value = mFileLogContext.GetDownloadedFile(path);
            //这里我们返回的应该是最新版本的File
            if (value != null)
            {
                if (value.State == FileState.Downloaded)
                    return AssetsConfig.AssetBundlePersistentDataPath + "/" + value.Path;
                if (value.State == FileState.Include)
                    return AssetsConfig.StreamingAssets + "/" + value.Path;
                return string.Empty;
            }
            else
            {
                //从Filelog中找不到,可能文件之前下载了但是玩家没有下载完.导致Filelog还未被写入.这里我们去本地查找一下
                if (File.Exists(AssetsConfig.AssetBundlePersistentDataPath + "/" + path))
                {
                    return AssetsConfig.AssetBundlePersistentDataPath + "/" + path;
                }
            }

            return null;
        }
        
        public async UniTask<List<AssetBundle>> Download(List<string> paths, IProgress<float> progress = null)
        {
            var listPool = ListPool<AssetFileLog>.Create();
            foreach (var node in paths)
            {
                if (node.EndsWith(AssetsConfig.Suffix))
                {
                    listPool.Add(mFileLogContext.GetRemoteBundle(node));
                }
                else
                {
                    listPool.Add(mFileLogContext.GetRemoteBundleFromFileName(node));
                }
            }
            var assetBundles = await Internal_Download(listPool, progress);
            listPool.Dispose();
            return assetBundles;
        }
        
        private async UniTask<List<AssetBundle>> Internal_Download(List<AssetFileLog> pathList, IProgress<float> progress)
        {
            ListPool<UniTask<AssetBundle>> downloadTasks = ListPool<UniTask<AssetBundle>>.Create();
            List<AssetBundle> assetBundles = new List<AssetBundle>();

            foreach (var path in pathList)
            {
                downloadTasks.Add(Download(path.Path));
            }

            int length = downloadTasks.Count;
            if (length > 0)
            {
                int finished = 0;
                while (true)
                {
                    for (int i = downloadTasks.Count - 1; i >= 0; i--)
                    {
                        if (downloadTasks[i].Status.IsCompleted())
                        {
                            finished++;
                            assetBundles.Add(downloadTasks[i].GetAwaiter().GetResult());
                            downloadTasks.RemoveAt(i);
                        }
                    }

                    if (finished >= length)
                    {
                        progress?.Report(100);
                        break;
                    }

                    progress?.Report((float)finished / length * 100);
                    await UniTask.NextFrame();
                }

                mFileLogContext.SyncDownloadedFileLog();
            }
            downloadTasks.Dispose();
            return assetBundles;
        }

        private async UniTask<AssetBundle> Download(string path)
        {
            IDownloadPlatform service = mDownloadServices;
            int tryTimes = 0;

            while (tryTimes++ < 5)
            {
                try
                {
                    var result = await service.Download(path);
                    if (!result)
                    {
                        Log.Error($"文件{path}校验不通过.重新下载");
                        continue;
                    }
                    Log.Debug(path + "    下载完毕");
                    mFileLogContext.UpdateDownloadedAssetInMemory(path);
                    return result;
                }
                catch (RemoteFileNotFound e)
                {
                    Log.Error(e);
                }
                catch (System.Exception e)
                {
                    Log.Error($"下载文件{path}失败,正在重新尝试第{tryTimes}次,{e}");
                }
            }

            return null;
        }
    }
}