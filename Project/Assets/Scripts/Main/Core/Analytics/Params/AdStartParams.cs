using Panthea.Utils;

namespace EdgeStudio.Analytics
{
    public readonly struct AdStartParams
    {
        public readonly int Ecpm;
        public readonly string AdUnit;
        public readonly string Uuid;
        public AdStartParams(int ecpm,string adUnit,string uuid)
        {
            Ecpm = ecpm;
            AdUnit = adUnit;
            Uuid = uuid;
        }

        public DictionaryPool<string, string> Get()
        {
            var dict = AnalyticsParamsDictionary.Get();
            dict.Add("ecpm", Ecpm.ToString());
            dict.Add("ad_Unit", AdUnit);
            dict.Add("uuid", Uuid);
            return dict;
        }
    }
}