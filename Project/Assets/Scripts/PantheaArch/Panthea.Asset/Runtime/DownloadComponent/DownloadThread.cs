namespace Panthea.Asset
{
    public class DownloadThread
    {
        public string Url;
        public string WritePath;
        public long Length;
        public long Version;
        public uint Crc;

        public DownloadThread(string url, string path, long length, long version, uint crc)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new System.Exception("Url 不能为空！！！！");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new System.Exception("下载路径不可为空");
            }

            if (length == 0)
            {
                throw new System.Exception("文件长度不能为0");
            }

            if (version == 0)
            {
                throw new System.Exception("版本号不能为0");
            }

            Url = url;
            WritePath = path;
            Length = length;
            Version = version;
            Crc = crc;
        }
    }
}