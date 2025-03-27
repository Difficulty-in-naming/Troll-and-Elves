using System;
using Cysharp.Threading.Tasks;

using Panthea.Common;
using UnityEngine;
using UnityEngine.Networking;

namespace EdgeStudio.Manager
{
    [Serializable]
    public class ServerConfig
    {
        public string ServerVersion;
        public bool OpenPrivatePolicy;
        public bool OpenNotice;
        public bool OpenIOSPay;
        public string Notice;
    }
    
    [Serializable]
    public class ServerConfigArray
    {
        public ServerConfig[] Configs;
    }
    
    public class ServerConfigManager : Singleton<ServerConfigManager>
    {
        public ServerConfig Data;

        public async UniTask Fetch()
        {
            #if UNITY_EDITOR
            Data = new ServerConfig
            {
                ServerVersion = GameSettings.Version,
                OpenNotice = true,
                OpenIOSPay = UnityEditor.EditorPrefs.GetBool(Application.productName + ".OpenIOSIAP"),
                Notice = "",
                OpenPrivatePolicy = true
            };
            return;
            #endif
            var tryCount = 0;
            while (tryCount <= 3)
            {
                try
                {
                    using var www = UnityWebRequest.Get("https://app.oneheartgame.com/storymerge/serverconfig/%E6%9C%8D%E5%8A%A1%E5%99%A8%E9%85%8D%E7%BD%AE.json");
                    www.downloadHandler = new DownloadHandlerBuffer();
                    await www.SendWebRequest();
                    string receivedData = www.downloadHandler.text;
                    var data = JsonUtility.FromJson<ServerConfigArray>(receivedData);
                    foreach (var node in data.Configs)
                    {
                        if (node.ServerVersion == GameSettings.Version)
                        {
                            Log.Print("找到匹配当前版本的服务器配置." + JsonUtility.ToJson(node));
                            Data = node;
                            return;
                        }
                    }
                    Log.Error("没有找到服务器配置.前端自己生成一个");
                    Data = new ServerConfig
                    {
                        ServerVersion = GameSettings.Version,
                        OpenNotice = false,
                        OpenIOSPay = false,
                        Notice = "",
                        OpenPrivatePolicy = false
                    };
                    return;
                }
                catch(Exception ex)
                {
                    Log.Error(ex);
                    tryCount++;
                }
            }
        }
    }
}