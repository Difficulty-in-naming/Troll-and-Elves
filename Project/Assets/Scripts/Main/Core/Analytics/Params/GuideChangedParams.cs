using Panthea.Utils;

namespace EdgeStudio.Analytics
{
    public struct GuideChangedParams
    {
        /// <summary>
        /// 后台填写的引导ID,如guide_1
        /// </summary>
        public string Cid;
        public DictionaryPool<string, string> Get()
        {
            var dict = AnalyticsParamsDictionary.Get();
            dict.Add("cid",Cid);
            return dict;
        }
    }
}