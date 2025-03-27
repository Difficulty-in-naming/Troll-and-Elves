using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Panthea.Asset
{
    public interface IAssetBundlePool
    {
        Dictionary<string, ResPak> GetLoadedAssetBundle();
        UniTask<ResPak> GetAsync(string path, CancellationToken token = default);
        void Release(string path, bool releaseAssets = true);
        void Release(ResPak ab, bool releaseAssets = true);
        void UnloadAllAssetBundle();
    }
}