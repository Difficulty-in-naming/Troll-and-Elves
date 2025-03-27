
using EdgeStudio.Manager;
using R3;
#if !NET
using System;
#endif
namespace EdgeStudio.DB
{
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    [Serializable]
    public partial class AdWatchRecord
    {
        public uint LastTime;
        public int Count;
#if !NET
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        private readonly Subject<bool> watchStateSubject = new();
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
#if USEJSON
        [Newtonsoft.Json.JsonIgnore]
#endif
        public Observable<bool> WatchStateChanged => watchStateSubject;
        public bool HasWatched()
        {
            DateTime lastTime = DateTimeOffset.FromUnixTimeSeconds(LastTime).Date;
            DateTime today = RealTimeManager.Inst.GetUtcDate().Date;
            return lastTime == today && Count <= 0;
        }

        // 标记今天已经看过广告
        public void MarkAdWatched()
        {
            LastTime = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Count--;
            watchStateSubject.OnNext(HasWatched());
        }

        public void Reset(int defaultCount)
        {
            DateTime lastTime = DateTimeOffset.FromUnixTimeSeconds(LastTime).Date;
            DateTime today = RealTimeManager.Inst.GetUtcDate().Date;
            if (lastTime != today)
            {
                Count = defaultCount;
            }
        }
#endif
    }
}