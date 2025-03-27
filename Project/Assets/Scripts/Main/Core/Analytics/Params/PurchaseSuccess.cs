using Panthea.Utils;

namespace EdgeStudio.Analytics
{
    public readonly struct PurchaseSuccess
    {
        /// <summary>
        /// 礼包ID
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// 购买花费
        /// </summary>
        public readonly int Price;
        /// <summary>
        /// 购买花费的时间
        /// </summary>
        public readonly int Time;
        public PurchaseSuccess(int id, int price,int time)
        {
            Id = id;
            Price = price;
            Time = time;
        }

        public DictionaryPool<string, string> Get()
        {
            var dict = AnalyticsParamsDictionary.Get();
            dict.Add("purchase_id", Id.ZeroGCString());
            dict.Add("price", Price.ToString());
            dict.Add("duration", Time.ToString());
            return dict;
        }
    }
}