using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Panthea.Common;
using UnityEngine;
using UnityEngine.Networking;

namespace Panthea.Asset
{
    public class DefaultDownloader: IDownloadPlatform
    {
        public delegate string GetWritePath(string url);
        public delegate string WebUrl(string url);
        public delegate ValueTask<bool> VerifyFile(string url,FileInfo tempFile);

        private readonly WebUrl mWebUrl;
        private readonly GetWritePath mGetWriteFilePath;
        private readonly VerifyFile mVerifyFile;
        public DefaultDownloader(WebUrl url,GetWritePath getWriteFilePath,VerifyFile verifyFile)
        {
            mWebUrl = url;
            mGetWriteFilePath = getWriteFilePath;
            mVerifyFile = verifyFile;
        }
        
        public async UniTask<AssetBundle> Download(string url)
        {
            if (AssetsConfig.UseAssetBundleCache)
            {
                var path = mGetWriteFilePath(url);
                var tempPath = path + ".temp";
                var unityWebRequest = new UnityWebRequest { downloadHandler = new DownloadHandlerFile(tempPath), url = mWebUrl(url) };
                var originFilePath = path;
                if (File.Exists(tempPath))
                {
                    var length = new FileInfo(tempPath).Length;
                    unityWebRequest.SetRequestHeader("Range", $"bytes={length}-");
                }
            
                var webRequest = unityWebRequest.SendWebRequest();
                while (!webRequest.isDone)
                {
                    await UniTask.NextFrame();
                }
            
                if (!string.IsNullOrEmpty(unityWebRequest.error))
                {
                    throw new System.Exception(unityWebRequest.error);
                }
            
                if (File.Exists(tempPath))
                {
                    if(!await mVerifyFile(url,new FileInfo(tempPath)))
                    {
                        File.Delete(tempPath);
                        return null;
                    }
                }
#if !UNITY_WEBGL
                var assetBundle = AssetBundle.RecompressAssetBundleAsync(tempPath, path, BuildCompression.LZ4Runtime);
                await assetBundle;
#else
                File.Copy(tempPath, originFilePath, true);
#endif
                File.Delete(tempPath);
                unityWebRequest.Dispose();
                var abRequest = AssetBundle.LoadFromFileAsync(path);
                while (!abRequest.isDone)
                {
                    await UniTask.NextFrame();
                }

                return abRequest.assetBundle;
            }
            else
            {
                using var request = UnityWebRequestAssetBundle.GetAssetBundle(mWebUrl(url));
                await request.SendWebRequest();
                if (!string.IsNullOrEmpty(request.error))
                {
                    Log.Error("下载AssetBundle错误:" + request.error);
                }
                AssetBundle ab = (request.downloadHandler as DownloadHandlerAssetBundle)?.assetBundle;
                return ab;
            }
        }
        
        public async UniTask<string> GetText(string url)
        {
            url = mWebUrl(url);
            using var request = new UnityWebRequest();
            request.downloadHandler = new DownloadHandlerBuffer();
            request.url = url;
            var webRequest = request.SendWebRequest();
            while (!webRequest.isDone)
            {
                await UniTask.NextFrame();
            }
            if (!string.IsNullOrEmpty(request.error))
            {
                throw new System.Exception(request.error);
            }
            
            return request.downloadHandler.text;
        }

        public async UniTask<byte[]> GetBytes(string url)
        {
            url = mWebUrl(url);
            var request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            Debug.Log("开始下载:" + url);
            var webRequest = request.SendWebRequest();
            while (!webRequest.isDone)
            {
                await UniTask.NextFrame();
            }
            if (!string.IsNullOrEmpty(request.error))
            {
                Debug.LogError("下载失败:" + request.error);
                throw new System.Exception(request.error);
            }

            Debug.Log("下载完成:" + request.downloadedBytes);
            return request.downloadHandler.data;
        }
    }
}