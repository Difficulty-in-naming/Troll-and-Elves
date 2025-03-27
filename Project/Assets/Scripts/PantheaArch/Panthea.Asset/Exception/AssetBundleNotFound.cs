namespace Panthea.Asset
{
    public class AssetBundleNotFound: System.Exception
    {
        private string mPath;
        public override string Message => "无法从StreamingAssets或Persistent目录下找到{" + mPath + "}文件";

        public AssetBundleNotFound(string path)
        {
            mPath = path;
        }
    }
}