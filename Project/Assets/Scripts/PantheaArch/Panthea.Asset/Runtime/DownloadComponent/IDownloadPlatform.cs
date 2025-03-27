using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Panthea.Asset
{
    public interface IDownloadPlatform
    {
        UniTask<AssetBundle> Download(string url);
        UniTask<string> GetText(string url);
        UniTask<byte[]> GetBytes(string url);
    }
}