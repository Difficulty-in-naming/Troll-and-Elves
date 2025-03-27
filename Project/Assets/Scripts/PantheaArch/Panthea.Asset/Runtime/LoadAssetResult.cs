namespace Panthea.Asset
{
    public struct LoadAssetResult
    {
        public ResPak Pak;
        public object Asset;

        public LoadAssetResult(ResPak pak, object asset)
        {
            Pak = pak;
            Asset = asset;
        }
    }
}