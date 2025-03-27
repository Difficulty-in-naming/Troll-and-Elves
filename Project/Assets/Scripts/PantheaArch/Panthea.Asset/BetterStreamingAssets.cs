using System;
using System.IO;
#if UNITY_ANDROID && !UNITY_EDITOR
using System.IO.Compression;
using System.Text;
using UnityEngine;
#endif

namespace Panthea.Asset
{
    public class BetterStreamingAssets
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    private static ZipArchive mArchive;

    public static ZipArchive Archive
    {
        get
        {
            if (mArchive == null)
            {
                mArchive = ZipFile.Open(Application.dataPath, ZipArchiveMode.Read);
            }

            return mArchive;
        }
    }
#endif
        public static byte[] GetBytes(string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!path.StartsWith("assets/"))
                path = "assets/" + path;
            using var stream = GetStream(path);
            return stream?.ToArray();
#else
            return File.ReadAllBytes(AssetsConfig.StreamingAssets + "/" + path);
#endif
        }

        public static string GetText(string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!path.StartsWith("assets/"))
                path = "assets/" + path;
            using var stream = Internal_GetStream(path);
            if(stream == null)
                return null;
            var reader = new StreamReader(stream, Encoding.UTF8);
            var s = reader.ReadToEnd();
            stream.Dispose();
            reader.Dispose();
            return s;
#else
            return File.ReadAllText(AssetsConfig.StreamingAssets + "/" + path);
#endif
        }

        private static Stream Internal_GetStream(string path)
        {
            Stream stream = null;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!path.StartsWith("assets/"))
                path = "assets/" + path;
            try
            {
                var entry = Archive.GetEntry(path);
                if (entry != null)
                {
                    stream = entry.Open();
                    return stream;
                }
                return null;
            }
            catch(System.Exception e)
            {
                Log.Error(e);
                stream = new FileStream(AssetsConfig.StreamingAssets + "/" + path.Remove(0,7), FileMode.Open, FileAccess.Read);
                return stream;
            }
#else
            stream = new FileStream(AssetsConfig.StreamingAssets + "/" + path, FileMode.Open, FileAccess.Read);
            return stream;
#endif
        }

        public static MemoryStream GetStream(string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!path.StartsWith("assets/"))
                path = "assets/" + path;
            try
            {
                var entry = Archive.GetEntry(path);
                if (entry != null)
                {
                    using (var stream = entry.Open())
                    {
                        var wrap = new MemoryStream();
                        stream.CopyTo(wrap);
                        return wrap;
                    }
                }
                return null;
            }
            catch(System.Exception e)
            {
                Log.Error(e);
                var memoryStream = new MemoryStream();
                using var fileStream = new FileStream(AssetsConfig.StreamingAssets + "/" + path.Remove(0, 7), FileMode.Open, FileAccess.Read);
                fileStream.CopyTo(memoryStream);
                return memoryStream;
            }

#else
            var memoryStream = new MemoryStream();
            using var fileStream = new FileStream(AssetsConfig.StreamingAssets + "/" + path, FileMode.Open, FileAccess.Read);
            fileStream.CopyTo(memoryStream);
            return memoryStream;
#endif
        }

        public static bool HasExists(string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!path.StartsWith("assets/"))
                path = "assets/" + path;
            try
            {
                var entry = Archive.GetEntry(path);
                if (entry != null)
                {
                    return true;
                }
                return false;
            }
            catch(System.Exception e)
            {
                Log.Error(e);
                return File.Exists(AssetsConfig.StreamingAssets + "/" + path.Remove(0,7));
            }

#else
            return File.Exists(AssetsConfig.StreamingAssets + "/" + path);
#endif
        }

        public static DateTime GetCreateTime(string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!path.StartsWith("assets/"))
                path = "assets/" + path;
            try
            {
                var entry = Archive.GetEntry(path);
                if (entry != null)
                {
                    return entry.LastWriteTime.UtcDateTime;
                }
                return DateTime.MinValue;
            }
            catch(System.Exception e)
            {
                Log.Error(e);
                return File.GetCreationTimeUtc(path.Remove(0,7));
            }
            
#else
            return File.GetCreationTimeUtc(path);
#endif
        }
    }
}