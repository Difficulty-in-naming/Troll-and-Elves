using Panthea.Utils;

namespace EdgeStudio.Analytics
{
    public readonly struct AdEndParams
    {
        public readonly int Ecpm;
        public readonly bool IsComplete;
        public readonly int Duration;
        public readonly string AdUnit;
        public readonly string Uuid;
        public AdEndParams(int ecpm,  bool isComplete, int duration,string adUnit,string uuid)
        {
            Ecpm = ecpm;
            IsComplete = isComplete;
            Duration = duration;
            AdUnit = adUnit;
            Uuid = uuid;
        }

        public DictionaryPool<string, string> Get()
        {
            var dict = AnalyticsParamsDictionary.Get();
            dict.Add("ecpm", Ecpm.ToString());
            dict.Add("view_complete", IsComplete.ToString());
            dict.Add("ad_duration", Duration.ToString());
            dict.Add("ad_Unit", AdUnit);
            dict.Add("uuid", Uuid);
            return dict;
        }
    }
}