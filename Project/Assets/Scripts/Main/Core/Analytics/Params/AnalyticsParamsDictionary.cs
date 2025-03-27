using Panthea.Utils;

namespace EdgeStudio.Analytics
{
    public class AnalyticsParamsDictionary
    {
        public static DictionaryPool<string,string> Get()
        {
            var dict = DictionaryPool<string, string>.Create();
            /*dict.Add("id",Data_Player.MyData.Id);
            dict.Add("platform",NativeKit.Inst.Platform.ToString());
            dict.Add("channel",NativeKit.Inst.ChannelName);
            dict.Add("cur_level",Data_Player.MyData.Level.ZeroGCString());
            dict.Add("cur_star",Data_Prop.MyData.GetStoryPoint.ZeroGCString());
            dict.Add("cur_diamond",Data_Prop.MyData.GetGem.ZeroGCString());
            dict.Add("cur_purchase_num",Data_IAP.MyData.PurchaseNum.ZeroGCString());
            dict.Add("cur_purchase_time",Data_IAP.MyData.PurchaseTimes.ZeroGCString());
            dict.Add("cur_ad_num",Data_Player.MyData.WatchAdsTimes.ZeroGCString());
            dict.Add("cur_ad_jump_num",Data_Player.MyData.AdsSkipTimes.ZeroGCString());
            dict.Add("cur_strength",Data_Merge.MyData.CurEnergy.ZeroGCString());
            dict.Add("cur_lifetime",((int)TimeSpan.FromSeconds(TimeUtils.GetUtcTimeStamp() - Data_Player.MyData.FirstInitTime).TotalDays).ZeroGCString());*/
            return dict;
        }
    }
}