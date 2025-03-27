using System;

namespace Panthea.Utils
{
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    [Serializable]

    public partial struct RangeInt
    {
#if !NETCOREAPP
        [Sirenix.OdinInspector.HorizontalGroup("H",0.25f),Sirenix.OdinInspector.LabelWidth(40),Sirenix.OdinInspector.LabelText("最小值")]
#endif
        public int Min;
#if !NETCOREAPP
        [Sirenix.OdinInspector.HorizontalGroup("H",0.25f),Sirenix.OdinInspector.LabelWidth(40),Sirenix.OdinInspector.LabelText("最大值")]
#endif
        public int Max;
        
        public RangeInt(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public int Random() => UnityEngine.Random.Range(Min, Max + 1);
        public bool Contains(int value) => value >= Min && value <= Max;
    }
}