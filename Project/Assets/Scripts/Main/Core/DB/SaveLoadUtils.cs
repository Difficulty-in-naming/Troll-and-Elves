using System;
using System.IO;
using EdgeStudio.Config;
using Newtonsoft.Json;
using Panthea.Common;

namespace EdgeStudio.DB
{
    ///注意该方法不是线程安全方法.不可以在Async中调用该方法
    public static class SaveLoadUtils
    {
#if USE_MEMORYPACK && !USE_PLAYER_PREFS && !UNITY_EDITOR
    private static byte[] AesIV256 { get; } = { 17, 243, 74, 187, 226, 250, 127, 90, 53, 42, 96, 13, 152, 155, 47, 193 };

    private static byte[] AesKey256 { get; } =
    {
        161, 36, 168, 189, 234, 133, 73, 189, 21, 60, 168, 29, 186, 216, 240, 6, 73, 255, 34, 203, 86, 90, 103, 237, 221, 147, 136, 228, 245,
        26, 245, 65
    };

    private static AesCryptoServiceProvider Aes { get; } =
        new AesCryptoServiceProvider { BlockSize = 128, KeySize = 256, Key = AesKey256, IV = AesIV256 };
    private static Dictionary<string, Stream> CacheStreams { get; } = new();

    private static Stream GetStream(string path)
    {
        if (CacheStreams.TryGetValue(path, out var stream))
        {
            return stream;
        }

        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        CacheStreams[path] = stream;
        return stream;
    }
#endif

        public static object Save<T>(string path, T value)
        {
#if USE_MEMORYPACK && !UNITY_EDITOR
        var fsStream = GetStream(path);
        fsStream.SetLength(0);
        var message = MemoryPack.MemoryPackSerializer.Serialize(value);
        using var encryptor = Aes.CreateEncryptor();
        var bytes = encryptor.TransformFinalBlock(message, 0, message.Length);
#if !USE_PLAYER_PREFS || UNITY_EDITOR
        fsStream.Write(bytes);
        fsStream.Flush();
        return bytes;
#else
        var base64 = Convert.ToBase64String(bytes);
        PlayerPrefs.SetString(path, base64);
        return base64;
#endif
#elif USEJSON || UNITY_EDITOR
            var json = JsonConvert.SerializeObject(value, Formatting.None);
#if UNITY_EDITOR && !SIMULATE_WECHAT
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(path, json);
            return json;
#elif !USE_PLAYER_PREFS
        var fsStream = GetStream(path);
        fsStream.SetLength(0);
        fsStream.Write(Encoding.UTF8.GetBytes(json));
        return json;
#else
        PlayerPrefs.SetString(path, json);
        return json;
#endif

#endif
        }
    
        public static void Delete<T>(string path)
        {
#if WECHAT && !UNITY_EDITOR
        return;
#endif
#if USE_MEMORYPACK && !USE_PLAYER_PREFS && !UNITY_EDITOR
        var fsStream = GetStream(path);
        fsStream.SetLength(0);
        fsStream.Dispose();
        File.Delete(path);
        return;
#else
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                return;
            }

            File.Delete(path);
#endif
        }
    
        public static T Load<T>(string path)
        {
            return (T)Load(path, typeof(T));
        }
    
        public static object Load(string path,Type type)
        {
            try
            {
#if USE_MEMORYPACK && !UNITY_EDITOR
#if !USE_PLAYER_PREFS || UNITY_EDITOR
            var fsStream = GetStream(path);
            fsStream.Position = 0;
            var bytes = ReadFully(fsStream);
#else
            var bytes = Convert.FromBase64String(PlayerPrefs.GetString(path, string.Empty));
#endif
            if(bytes == null || bytes.Length == 0)
                return null;
            using var decryptor = Aes.CreateDecryptor();
            bytes = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            var message = MemoryPack.MemoryPackSerializer.Deserialize(type, bytes);
            return message;
#elif USEJSON || UNITY_EDITOR
#if !USE_PLAYER_PREFS || (UNITY_EDITOR && !SIMULATE_WECHAT)
                var text = File.ReadAllText(path);
#else
            var text = PlayerPrefs.GetString(path, string.Empty);
#endif
                if (string.IsNullOrEmpty(text))
                    return null;
                return JsonConvert.DeserializeObject(text, type,EdgeStudioJsonConverter.JsonConverters);
#endif
            }
            catch (Exception e)
            {
                Log.Print($"没有找到该文件 {path}\n{e}");
            }

            return null;
        }

#if USE_MEMORYPACK
    public static byte[] ReadFully(Stream input)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
#endif
    }
}
