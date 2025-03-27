namespace EdgeStudio.Server
{
    /// <summary>
    /// 根据玩家ID登录服务器
    /// </summary>
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    [ProtoRequest("api/[platform]", typeof(LoginResponse))]
    public partial class LoginRequest : DefaultRequest
    {
        /// <summary>
        /// 通过FetchSession获得的OpenId用来登录
        /// </summary>
        public string OpenId { get; set; }
    }
    
    /// <summary>
    /// 错误码
    /// 0	请求成功	
    /// 87009	无效的签名	
    /// 1000   请求的Code异常
    /// 1001   用户不存在
    /// 1002   SessionKey为空,请调用wx.login重新登录
    /// 5000   发生未知错误
    /// </summary>
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    public partial class LoginResponse
    {
        /// <summary>
        /// 用户Id,后续跟服务器通信时如果没有特别提示默认都应该用这个ID
        /// </summary>
        public int PlayerId { get; set; }
        /// <summary>
        /// 授权码.玩家必须持有该码才能与服务器进行后续的交互.该码在客户端最后一次协议的两个小时后失效
        /// </summary>
        public string AuthCode { get; set; }
    }
}