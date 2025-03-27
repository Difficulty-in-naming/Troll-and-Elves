using System;

namespace EdgeStudio.DB
{
    public class DBIndexAttribute : Attribute
    {
        public int Index;

        public DBIndexAttribute(int index)
        {
            Index = index;
        }
    }
}