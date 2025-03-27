using Panthea.Utils;

namespace EdgeStudio.Analytics
{
    public readonly struct PurchaseStart
    {
        //1为商店,2为自动弹出礼包
        public readonly int Id;
        public readonly float Price;

        public PurchaseStart(int id, float price)
        {
            Id = id;
            Price = price;
        }
        
        public DictionaryPool<string, string> Get()
        {
            var dict = AnalyticsParamsDictionary.Get();
            // dict.Add("purchase_way", Way.ToString());
            dict.Add("purchase_id", Id.ZeroGCString());
            dict.Add("price", Price.ZeroGCString());
            return dict;
        }
    }
}