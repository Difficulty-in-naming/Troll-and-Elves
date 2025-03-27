using System.Collections.Generic;

namespace EdgeStudio.Analytics
{
    public interface IAnalytics
    {
        void Initialize();
        /// <summary> 自定义 </summary>
        void Custom(string key,Dictionary<string, string> args);
        /// <summary> 登录 </summary>
        void Login(LoginParams args);
        /// <summary> 引导(未实现）</summary>
        void Guide(GuideChangedParams args);
        /// <summary> 内购拉起 </summary>
        void PurchaseStart(PurchaseStart args);
        /// <summary> 内购成功 </summary>
        void PurchaseSuccess(PurchaseSuccess args);
        /// <summary> 内购失败 </summary>
        void PurchaseFailed(PurchaseFailed args);
        /// <summary> 广告播放开始 </summary>
        void AdStart(AdStartParams args);
        /// <summary> 广告播放结束 </summary>
        void AdEnd(AdEndParams args);
    }
}