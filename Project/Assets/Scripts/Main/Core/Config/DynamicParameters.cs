

using UnityEngine;

namespace EdgeStudio.Config
{
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    public partial class DynamicParameters
    {
        public string String;
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        private int? mInt;

#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        public int Int
        {
            get
            {
                if (mInt.HasValue)
                {
                    return mInt.Value;
                }

                int.TryParse(String, out int result);
                mInt = result;
                return result;
            }
        }
        
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        private uint? mUint;

#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        public uint UInt
        {
            get
            {
                if (mUint.HasValue)
                {
                    return mUint.Value;
                }

                uint.TryParse(String, out uint result);
                mUint = result;
                return result;
            }
        }
        
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        private byte? mByte;

#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        public byte Byte
        {
            get
            {
                if (mByte.HasValue)
                {
                    return mByte.Value;
                }

                byte.TryParse(String, out var result);
                mByte = result;
                return result;
            }
        }

#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        private string[] mStringArray;

#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        public string[] StringArray
        {
            get
            {
                if (mStringArray != null)
                {
                    return mStringArray;
                }

                mStringArray = String.Split(GameSettings.StringSplit);
                return mStringArray;
            }
        }
        
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        private int[] mIntArray;

#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        public int[] IntArray
        {
            get
            {
                if (mIntArray != null)
                {
                    return mIntArray;
                }

                var split = String.Split(GameSettings.StringSplit);
                mIntArray = new int[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    mIntArray[i] = int.Parse(split[i]);
                }
                return mIntArray;
            }
        }
        
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        private float[] mfloatArray;

#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        public float[] FloatArray
        {
            get
            {
                if (mfloatArray != null)
                {
                    return mfloatArray;
                }

                var split = String.Split(GameSettings.StringSplit);
                mfloatArray = new float[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    mfloatArray[i] = float.Parse(split[i]);
                }
                return mfloatArray;
            }
        }
        
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        private float? mFloat;

#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        public float Float
        {
            get
            {
                if (mFloat.HasValue)
                {
                    return mFloat.Value;
                }

                float.TryParse(String, out var result);
                mFloat = result;
                return result;
            }
        }
        
        
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        private Vector2? mVector2;
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        public Vector2 Vector2
        {
            get
            {
                if (mVector2.HasValue)
                {
                    return mVector2.Value;
                }

                var split = String.Split(GameSettings.StringSplit);
                mVector2 = new Vector2(float.Parse(split[0]), float.Parse(split[1]));
                return mVector2.Value;
            }
        }
        
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        private Vector3? mVector3;
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackIgnore]
#endif
        public Vector3 Vector3
        {
            get
            {
                if (mVector3.HasValue)
                {
                    return mVector3.Value;
                }

                var split = String.Split(GameSettings.StringSplit);
                mVector3 = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
                return mVector3.Value;
            }
        }

#if USEJSON
        private object ReferenceObject { get; set; }

        /// <summary>
        /// 注意该方法使用Unity Json Converter 只能转换一些基础类型和List列表.不能转换字典或者DateTime等复杂类型
        /// </summary>
        public T Convert<T>() where T : class
        {
            if (ReferenceObject != null)
                return (T)ReferenceObject;
            ReferenceObject = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(String);
            return (T) ReferenceObject;
        }
#endif
    }
}