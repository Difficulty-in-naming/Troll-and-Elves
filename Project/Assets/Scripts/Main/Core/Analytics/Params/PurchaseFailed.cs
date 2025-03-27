using Panthea.Utils;

namespace EdgeStudio.Analytics
{
    public readonly struct PurchaseFailed
    {
        public readonly int Id;
        public readonly int Time;
        /// <summary>
        /// 购买花费
        /// </summary>
        public readonly int Price;
        public readonly string Reason;

        public PurchaseFailed(int id, int time,int price, string reason)
        {
            Id = id;
            Time = time;
            Price = price;
            Reason = reason;
        }

        public DictionaryPool<string, string> Get()
        {
            var dict = AnalyticsParamsDictionary.Get();
            dict.Add("purchase_id", Id.ZeroGCString());
            dict.Add("notes", Reason);
            dict.Add("duration", Time.ToString());
            dict.Add("price", Price.ZeroGCString());
            return dict;
        }
    }
}