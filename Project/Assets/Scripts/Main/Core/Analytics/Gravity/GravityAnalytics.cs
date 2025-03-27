/*
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EdgeStudio.Manager;

using GravityEngine;
using GravitySDK.PC.Constant;
using Panthea.Common;
using Panthea.Utils;
using UnityEngine;

namespace EdgeStudio.Analytics
{
    public class GravityAnalytics : IAnalytics
    {
        public bool IsInitialized { get; private set; }
        public GravityAnalytics()
        {
            new GameObject("GravityEngine", typeof(GravityEngineAPI));
            string accessToken = "xfCnlyDOMEivlkughiwdpqHam6yS0K5s"; // 项目通行证，在：网站后台-->设置-->应用列表中找到Access Token列 复制（首次使用可能需要先新增应用）
            string clientId = GameSettings.OpenId; // 通常是某一个用户的唯一标识，如产品为小游戏，则必须填用户的的 openId
            GravityEngineAPI.StartGravityEngine(accessToken, clientId);
            GravityEngineAPI.EnableAutoTrack(AUTO_TRACK_EVENTS.APP_ALL);
        }

        public async void Initialize()
        {
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    AutoResetUniTaskCompletionSource<bool> utcs = AutoResetUniTaskCompletionSource<bool>.Create();
                    GravityEngineAPI.Initialize("", PlayerManager.Inst.Name.Value, GameSettings.IntVersion, GameSettings.OpenId, false, new InitializeCallbackImpl(this,utcs));
                    var result = await utcs.Task;
                    if (result)
                        break;
                }
            }
            catch (Exception e)
            {
                //todo 这里应该接入自己的崩溃日志上报系统
                Log.Error("初始化引力引擎失败");
            }
        }

        public class InitializeCallbackImpl : IInitializeCallback
        {
            private readonly GravityAnalytics analytics;
            private AutoResetUniTaskCompletionSource<bool> Utcs;
            public InitializeCallbackImpl(GravityAnalytics gravityAnalytics,AutoResetUniTaskCompletionSource<bool> utcs)
            {
                analytics = gravityAnalytics;
                Utcs = utcs;
            }

            // 初始化失败之后回调，errorMsg为报错信息
            public void onFailed(string errorMsg)
            {
                Log.Error("initialize failed  with message " + errorMsg);
                Utcs?.TrySetResult(false);
            }

            // 初始化成功之后回调
            public void onSuccess(Dictionary<string, object> responseJson)
            {
                Log.Error("initialize success");
                Log.Error("initialize call end");
                GravityEngineAPI.Flush();
                analytics.IsInitialized = true;
                Utcs?.TrySetResult(true);
            }
        }
        

        public void Custom(string key, Dictionary<string, string> args) { }
        public void Login(LoginParams args) { }
        public void Guide(GuideChangedParams args) { }
        public void PurchaseStart(PurchaseStart args) { }
        public void PurchaseSuccess(PurchaseSuccess args) { }
        public void PurchaseFailed(PurchaseFailed args) { }
        public void AdStart(AdStartParams args) { }
        public void AdEnd(AdEndParams args) { }

        public static DictionaryPool<string, object> ConvertStringDictionaryToObjectDictionary(DictionaryPool<string, string> source)
        {
            var result = DictionaryPool<string,object>.Create();

            foreach (var kvp in source)
            {
                result[kvp.Key] = kvp.Value;
            }

            return result;
        }
    }
}
*/
