using System.Collections.Generic;

namespace EdgeStudio.Server
{
    /// <summary> 获取每日排行榜 </summary>
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    [ProtoRequest("api/[platform]", typeof(FetchDailyRankResponse))]
    public partial class FetchDailyRankRequest : AuthRequest
    {
        public FetchDailyRankRequest(string authCode, int playerId) : base(authCode, playerId)
        {
        }
    }

#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    public partial class FetchDailyRankResponse
    {
        public int Index { get; set; }
        public int Score { get; set; }

        public List<FetchDailyRankInfo> Ranks { get; set; }
    }
    
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    public partial class FetchDailyRankInfo
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public string Avatar { get; set; }
    } 
    
    /// <summary> 获取每日排行榜 </summary>
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    [ProtoRequest("api/[platform]", typeof(UpdateDailyRankResponse))]
    public partial class UpdateDailyRankRequest : AuthRequest
    {
        public UpdateDailyRankRequest(string authCode, int playerId) : base(authCode, playerId)
        {
        }
    }
    
    [ProtoRequest("api/[platform]", typeof(FetchDailyRankResponse))]
    public partial class UpdateDailyRankResponse
    {
    }
}