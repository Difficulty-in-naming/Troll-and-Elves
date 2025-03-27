using EdgeStudio.Manager;

namespace EdgeStudio.DB
{
    public class TaskSaveInfo
    {
        public int Id;
        public long Record;
    }

#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    [DBIndex((int)DBIndexMap.Player)]
    public partial class Data_Player : DBDefine
    {
        /// <summary> 玩家名称 </summary>
        public string Name = "";
        /// <summary> 玩家头像 </summary>
        public string Avatar = "";
        /// <summary> 上次保存的时间用来对比哪个存档更新 </summary>
        public uint UpdateTime;
        /// <summary> 游戏版本号 </summary>
        public string Version;
        /// <summary> 当前任务 </summary>
        public TaskSaveInfo[] Tasks = new TaskSaveInfo[3];

        public AdWatchRecord DiamondAds = new();
        public AdWatchRecord CoinAds = new();
#if !NET
        public override void PostUpdate()
        {
            base.PostUpdate();
            UpdateTime = RealTimeManager.Inst.GetCurrentUtcTimeStamp();
        }
#endif
    }
}