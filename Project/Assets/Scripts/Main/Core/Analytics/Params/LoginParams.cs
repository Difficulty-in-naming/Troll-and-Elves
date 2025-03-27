using Panthea.Utils;

namespace EdgeStudio.Analytics
{
    public readonly struct LoginParams
    {
        public DictionaryPool<string, string> Get()
        {
            var dict = AnalyticsParamsDictionary.Get();
            return dict;
        }
    }
}