using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Panthea.Common;
using UnityEngine.Networking;
namespace EdgeStudio.Server
{
    public partial class NetworkManager : Singleton<NetworkManager>
    {
#if UNITY_EDITOR
        private static string mBaseUrl => UnityEditor.EditorPrefs.GetString("NetworkConnectionString", "https://www.oneheartgame.com:5000/");
#elif WECHAT
        // private const string mBaseUrl = "http://192.168.50.12:5000/";
        private const string mBaseUrl = "https://www.oneheartgame.com:5000/";
#endif
#if UNITY_EDITOR
        private static string mPlatformUrl => mBaseUrl + "api/editor/";
#elif WECHAT
        private const string mPlatformUrl = mBaseUrl + "api/wx/";
#endif
        private async UniTask<T2> SendPlatformRequest<T1,T2>(string apiKey,T1 request) where T2 : class
        {
            Log.Print("Send Post " + mPlatformUrl + apiKey);
            using UnityWebRequest www = new UnityWebRequest(mPlatformUrl + apiKey, "POST");
            return await SendRequest<T1, T2>(request, www);
        }
        
        private async UniTask<T2> SendCommonRequest<T1,T2>(string apiKey,T1 request) where T2 : class
        {
            Log.Print("Send Post " + mBaseUrl + apiKey);
            using UnityWebRequest www = new UnityWebRequest(mBaseUrl + apiKey, "POST");
            return await SendRequest<T1, T2>(request, www);
        }
        
        private async UniTask SendCommonRequest<T1>(string apiKey,T1 request)
        {
            Log.Print("Send Post " + mBaseUrl + apiKey);
            using UnityWebRequest www = new UnityWebRequest(mBaseUrl + apiKey, "POST");
            await SendRequest(request, www);
        }
        
        private async UniTask SendPlatformRequest<T1>(string apiKey,T1 request)
        {
            Log.Print("Send Post " + mPlatformUrl + apiKey);
            using UnityWebRequest www = new UnityWebRequest(mPlatformUrl + apiKey, "POST");
            await SendRequest(request, www);
        }

        private static async UniTask<T2> SendRequest<T1, T2>(T1 request, UnityWebRequest www) where T2 : class
        {
#if USE_MEMORYPACK_FOR_PROTOCOL_API
            www.SetRequestHeader("Content-Type", "application/x-memorypack");
            www.uploadHandler = new UploadHandlerRaw(MemoryPack.MemoryPackSerializer.Serialize(request));
#else
            www.SetRequestHeader("Content-Type", "application/json");
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(request)));
#endif
            www.downloadHandler = new DownloadHandlerBuffer();
            try
            {
                await www.SendWebRequest();
#if USE_MEMORYPACK_FOR_PROTOCOL_API
                byte[] receivedData = www.downloadHandler.data;
                return MemoryPack.MemoryPackSerializer.Deserialize<T2>(receivedData);
#else
                string receivedData = www.downloadHandler.text;
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T2>(receivedData);
#endif
            }
            catch (UnityWebRequestException)
            {
                if (www.downloadHandler.data != null)
                {
#if USE_MEMORYPACK_FOR_PROTOCOL_API
                    byte[] receivedData = www.downloadHandler.data;
                    var data = MemoryPack.MemoryPackSerializer.Deserialize<int>(receivedData);
#else
                    var data = www.downloadHandler.text;
#endif
                    throw new Exception($"{www.error}\n发送协议{typeof(T1).Name}报错,错误码为:{data},错误码对应的文字内容可以查询{typeof(T2).Name}的注释");
                }
                throw new Exception($"{www.error}\n发送协议{typeof(T1).Name}报错");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }
        
        private static async UniTask SendRequest<T1>(T1 request, UnityWebRequest www)
        {
#if USE_MEMORYPACK_FOR_PROTOCOL_API
            www.SetRequestHeader("Content-Type", "application/x-memorypack");
            www.uploadHandler = new UploadHandlerRaw(MemoryPack.MemoryPackSerializer.Serialize(request));
#else
            www.SetRequestHeader("Content-Type", "application/json");
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(request)));
#endif
            await www.SendWebRequest();
        }
    }
}