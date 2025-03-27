namespace EdgeStudio.Server
{
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    public partial class AuthRequest
    {
        public string AuthCode { get; set; }
        public int PlayerId { get; set; }

        public AuthRequest(string authCode, int playerId)
        {
            AuthCode = authCode;
            PlayerId = playerId;
        }
    }
}