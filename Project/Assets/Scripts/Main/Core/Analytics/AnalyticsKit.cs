using System;
using System.Collections.Generic;
using UnityEngine;

namespace EdgeStudio.Analytics
{
    public class AnalyticsKit : IAnalytics
    {
        private Dictionary<string, double> TimeEvents = new Dictionary<string, double>();
        private readonly List<IAnalytics> analytics = new();
        private static AnalyticsKit mInst;
        public static AnalyticsKit Inst => mInst ??= new AnalyticsKit();
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
        static void Purge() => mInst = null;
#endif
        public void Register(IAnalytics ana) => analytics.Add(ana);
        public void UnRegister(Type type) {}
        public void Initialize() => Foreach(t1=>t1.Initialize());
        public void Login(LoginParams args) => Foreach(t1 => t1.Login(args));
        public void Guide(GuideChangedParams args) => Foreach(t1 => t1.Guide(args));
        public void PurchaseStart(PurchaseStart args) => Foreach(t1 => t1.PurchaseStart(args));
        public void PurchaseSuccess(PurchaseSuccess args) => Foreach(t1 => t1.PurchaseSuccess(args));
        public void PurchaseFailed(PurchaseFailed args) => Foreach(t1 => t1.PurchaseFailed(args));
        public void AdStart(AdStartParams args) => Foreach(t1 => t1.AdStart(args));
        public void AdEnd(AdEndParams args) => Foreach(t1 => t1.AdEnd(args));
        public void Custom(string key, Dictionary<string, string> args) => Foreach(t1 => t1.Custom(key, args));
        private void Foreach(Action<IAnalytics> call)
        {
            foreach (var node in analytics)
            {
                call(node);
            }
        }
        
        public void SetTimeEvent(string eventName) => TimeEvents.TryAdd(eventName, Time.realtimeSinceStartupAsDouble);

        public T GetAnalytics<T>()
        {
            foreach (var node in analytics)
            {
                if (node is T result)
                {
                    return result;
                }
            }
            return default;
        }
        
        public float GetTimeEvent(string eventName)
        {
            if (TimeEvents.TryGetValue(eventName, out double time))
            {
                TimeEvents.Remove(eventName);
                return Mathf.Abs((int)(Time.realtimeSinceStartupAsDouble - time));
            }
            return 0;
        }
    }
}