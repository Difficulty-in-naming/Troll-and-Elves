namespace EdgeStudio.Server
{
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    [ProtoRequest("api/[platform]", typeof(FetchSavesResponse))]
    public partial class PayRequest : AuthRequest
    {
        public PayRequest(string authCode, int playerId) : base(authCode, playerId)
        {
        }
    }
}