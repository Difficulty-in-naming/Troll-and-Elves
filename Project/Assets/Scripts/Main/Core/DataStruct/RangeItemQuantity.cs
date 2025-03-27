using System;
using RangeInt = Panthea.Utils.RangeInt;

namespace EdgeStudio.DataStruct
{
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    [Serializable]
    public partial class RangeItemQuantity
    {
        public int Id;
        public RangeInt Num;
        public int Weight;
        public bool IsFree() => Num is { Min: <= 0, Max: <= 0 };
/*#if !NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameItemProperty GetProperty() => GameItemProperty.Read(Id);
#endif*/
    }
}