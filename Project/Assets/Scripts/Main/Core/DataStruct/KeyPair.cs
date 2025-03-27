namespace EdgeStudio.DataStruct
{
#if USE_MEMORYPACK
    [MemoryPack.MemoryPackable]
#endif
    public partial class KeyPair<T1,T2>
    {
        public T1 Key;
        public T2 Value;
#if USE_MEMORYPACK
        [MemoryPack.MemoryPackConstructor]
#elif USEJSON
        [Newtonsoft.Json.JsonConstructor]
#endif
        public KeyPair()
        {
        }

        public KeyPair(T1 key, T2 value)
        {
            Key = key;
            Value = value;
        }
    }
}