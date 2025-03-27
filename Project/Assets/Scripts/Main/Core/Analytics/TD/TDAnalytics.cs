/*
using HotFix.Analytics;
using HotFix.Analytics.Params;
using HotFix.HotFix.Scripts.Analytics;
using System.Collections.Generic;
using EdgeStudio.Manager;
using Hotfix;
using HotFix;
using Hotfix.Analytics.Params;
using ThinkingData.Analytics;
using ThinkingSDK.PC.Constant;
using StoryEnd = HotFix.Analytics.Params.StoryEnd;
using StoryStart = HotFix.Analytics.Params.StoryStart;
#if WECHAT
using WeChatWASM;
#endif
using SystemInfo = UnityEngine.Device.SystemInfo;

namespace EdgeStudio.Main.Analytics
{
    public class TDAnalytics : IAnalytics
    {
        private const string AppId = "48732f302d4a44c08aa7643604272cb9";
        private const string ServerUrl = "https://ta-receive.xiqmax.com";
        private Dictionary<string, object> presetParamsDict = new();
        public string Account { get; private set; }
        
        public TDAnalytics()
        {
            ThinkingData.Analytics.TDAnalytics.Init(new ThinkingData.Analytics.TDConfig(AppId, ServerUrl));
// #if UNITY_EDITOR
            ThinkingData.Analytics.TDAnalytics.EnableLog(true);
/*#else
            ThinkingData.Analytics.TDAnalytics.EnableLog(false);
#endif#1#
            // #if DEVELOPMENT_BUILD
            const TDAutoTrackEventType @event = TDAutoTrackEventType.AppInstall | TDAutoTrackEventType.AppStart | TDAutoTrackEventType.AppEnd /*| TDAutoTrackEventType.AppCrash#1#;
            // #else
            // const TDAutoTrackEventType @event = TDAutoTrackEventType.AppInstall | TDAutoTrackEventType.AppStart | TDAutoTrackEventType.AppEnd;
            // #endif
            ThinkingData.Analytics.TDAnalytics.EnableAutoTrack(@event);
#if UNITY_EDITOR
            presetParamsDict.Add("os", SystemInfo.operatingSystem);
            presetParamsDict.Add("device_model", SystemInfo.deviceModel);
            presetParamsDict.Add("os_version", SystemInfo.operatingSystem);
            ThinkingData.Analytics.TDAnalytics.SetSuperProperties(presetParamsDict);
#elif WECHAT
            WX.GetSystemInfo(new GetSystemInfoOption{success = info =>
            {
                presetParamsDict.Add("#os", info.platform);
                presetParamsDict.Add("#device_model", info.model + "(" + info.version + ")");
                presetParamsDict.Add("#os_version", info.system);
            },fail = _ =>
            {

            }});
            ThinkingData.Analytics.TDAnalytics.SetSuperProperties(presetParamsDict);
            //数数在微信小游戏中获取的数据部分是有异常的这里我们进行补全
#endif
        }

        public void Initialize()
        {
        }

        public void Login(LoginParams args) => ThinkingData.Analytics.TDAnalytics.Login(Account = PlayerManager.Inst.PlayerId.ToString());
        public void Guide(GuideChangedParams args) => Send("complete_guide", args.Get());
        public void PurchaseStart(PurchaseStart args) => Send("purchase_start", args.Get());
        public void PurchaseSuccess(PurchaseSuccess args) => Send("purchase_success", args.Get());
        public void PurchaseFailed(PurchaseFailed args) => Send("purchase_fail", args.Get());
        public void AdStart(AdStartParams args) => Send("ad_start", args.Get());
        public void AdEnd(AdEndParams args) => Send("ad_end", args.Get());
        public void LevelStart(LevelStart args)
        {
        }

        public void LevelEnd(LevelEnd args)
        {
        }

        public void LevelTime(LevelTime args)
        {
        }

        public void ExitMerge(ExitMergeParams args) => Send("exit_merge", args.Get());
        public void StrengthGain(StrengthGainParams args) => Send("strength_gain", args.Get());
        public void LevelStart(LevelEnd args) => Send("level_up", args.Get());
        public void StoryStart(StoryStart args) => Send("story_start", args.Get());
        public void StoryEnd(StoryEnd args) => Send("story_end", args.Get());
        public void BuyProp(BuyPropParams args) => Send("buy_prop", args.Get());
        
        private void Send(string key, DictionaryPool<string,string> dict)
        {
            using var cache = ConvertDictToObjectValue(dict);
            ThinkingData.Analytics.TDAnalytics.Track(key,cache);
            dict.Dispose();
        }

        private DictionaryPool<string, object> ConvertDictToObjectValue(DictionaryPool<string,string> dict)
        {
            var pool = DictionaryPool<string, object>.Create();
            foreach (var node in dict) pool.Add(node.Key, node.Value);
            return pool;
        }
    }
}
*/
