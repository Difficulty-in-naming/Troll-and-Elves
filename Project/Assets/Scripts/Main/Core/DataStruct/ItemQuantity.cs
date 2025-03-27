using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
// using EdgeStudio.Config;

namespace EdgeStudio.DataStruct
{
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    [Serializable]
    public partial class ItemQuantity
    {
        public string Id = "";
        public int Num;
        public bool IsFree() => Num <= 0;
// #if !NETCOREAPP
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public GameItemProperty GetProperty() => GameItemProperty.Read(Id);
// #endif
        public static List<ItemQuantity> EmptyItems = new List<ItemQuantity>();

        public ItemQuantity(string id, int num)
        {
            Id = id;
            Num = num;
        }

        public ItemQuantity()
        {
        }

        public ItemQuantity Clone() => (ItemQuantity)MemberwiseClone();
    }
}