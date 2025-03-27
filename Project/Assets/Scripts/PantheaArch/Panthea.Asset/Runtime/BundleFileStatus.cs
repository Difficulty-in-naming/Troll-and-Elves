namespace Panthea.Asset
{
    public readonly struct BundleFileStatus
    {
        public bool NeedDownload { get; }
        public AssetFileLog Asset { get; }

        public BundleFileStatus(AssetFileLog asset, bool needDownload)
        {
            Asset = asset;
            NeedDownload = needDownload;
        }
    }
}