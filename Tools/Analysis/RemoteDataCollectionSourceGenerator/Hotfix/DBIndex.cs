using System;

namespace EdgeStudio.DB
{
    public enum DBIndexMap { Player = 10 }

    public class DBIndexAttribute : Attribute
    {
        public int Index;

        public DBIndexAttribute(int index)
        {
            Index = index;
        }
    }
    
    /// <summary>
    /// 标记数据只存储在内存中，不会保存到本地存档
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DBCacheOnlyAttribute : Attribute
    {
    }
}


