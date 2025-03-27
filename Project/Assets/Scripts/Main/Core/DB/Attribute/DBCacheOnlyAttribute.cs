using System;

namespace EdgeStudio.DB
{
    /// <summary>
    /// 标记数据只存储在内存中，不会保存到本地存档
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DBCacheOnlyAttribute : Attribute
    {
    }
}