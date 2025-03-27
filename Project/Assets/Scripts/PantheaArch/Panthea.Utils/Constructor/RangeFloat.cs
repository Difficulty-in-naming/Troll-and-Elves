using System;

namespace Panthea.Utils
{
    [Serializable]
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    public partial struct RangeFloat
    {
        public float Min;
        public float Max;
        public RangeFloat(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}