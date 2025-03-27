namespace EdgeStudio.Server
{
    /// <summary>
    /// 获取云存档
    /// </summary>
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    [ProtoRequest("api/[platform]", typeof(FetchSavesResponse))]
    public partial class FetchSavesRequest : AuthRequest
    {
        public FetchSavesRequest(string authCode, int playerId) : base(authCode, playerId)
        {
        }
    }
    
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    public partial class FetchSavesResponse
    {
        public string Json { get; set; }
    }
    
    /// <summary>
    /// 上传云存档
    /// </summary>
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    [ProtoRequest("api/[platform]", typeof(UploadSavesResponse))]
    public partial class UploadSavesRequest : AuthRequest
    {
        public string Json { get; set; }
        public int Rank { get; set; }
        public UploadSavesRequest(string authCode, int playerId) : base(authCode, playerId)
        {
        }
    }
    
    /// <summary>
    /// 错误码
    /// 1000   数据错误.数据无法被反序列化
    /// </summary>
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    public partial class UploadSavesResponse
    {
        public long UpdateTime { get; set; }
    }
}