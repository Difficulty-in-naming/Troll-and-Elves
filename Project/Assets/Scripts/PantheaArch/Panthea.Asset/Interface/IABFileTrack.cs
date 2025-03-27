using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Panthea.Asset
{
    public interface IABFileTrack
    {
        string FileLogName { get; }
        IABFileTrack ConfigureDownloadPlatform(IDownloadPlatform platform);
        UniTask Initialize();
        List<AssetFileLog> CheckUpdateList();
        AssetFileLog GetRemoteBundle(string path);
        AssetFileLog GetDownloadedFile(string path);
        AssetFileLog GetDownloadedBundle(string path);
        AssetFileLog GetRemoteBundleFromFileName(string path);
        void UpdateDownloadedAssetInMemory(string path);
        void SyncDownloadedFileLog();
        List<string> GetFilterAssetBundle(string[] path);
    }
}