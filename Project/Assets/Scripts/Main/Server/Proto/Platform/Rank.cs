using System.Collections.Generic;

namespace EdgeStudio.Server
{
    /// <summary> 获取排行榜 </summary>
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    [ProtoRequest("api/[platform]", typeof(FetchRankResponse))]
    public partial class FetchRankRequest : AuthRequest
    {
        public FetchRankRequest(string authCode, int playerId) : base(authCode, playerId)
        {
        }
    }

#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    public partial class FetchRankResponse
    {
        public int Index { get; set; }
        public int Score { get; set; }
        public List<FetchRankInfo> Ranks { get; set; }
    }

#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    public partial class FetchRankInfo
    {
        public string Name { get; set; }
        public string Avatar { get; set; }
        public int Score { get; set; }
    } 
}