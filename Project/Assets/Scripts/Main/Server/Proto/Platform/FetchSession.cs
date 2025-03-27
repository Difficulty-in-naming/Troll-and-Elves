namespace EdgeStudio.Server
{
    /// <summary>
    /// 根据第三方平台凭证获得用户Id
    /// </summary>
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    [ProtoRequest("api/[platform]", typeof(FetchSessionResponse))]
    public partial class FetchSessionRequest : DefaultRequest
    {
        public string Code { get; set; }
    }
    
    /// <summary>
    /// 错误码
    /// -1	系统繁忙，此时请开发者稍候再试	
    /// 0	请求成功	
    /// 40029	code 无效	
    /// 45011	频率限制，每个用户每分钟100次	
    /// 40226	高风险等级用户，小程序登录拦截 。风险等级详见用户安全解方案
    /// 1000   请求的Code异常
    /// 1001   服务器内部申请ID异常
    /// 1002   微信服务器验证失败
    /// </summary>
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    public partial class FetchSessionResponse
    {
        /// <summary>
        /// 平台ID.给前端做一些平台特性用的
        /// </summary>
        public string OpenId { get; set; }
    }
}