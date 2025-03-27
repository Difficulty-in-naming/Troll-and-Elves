namespace EdgeStudio.Tools
{
    public static class ObjectHelper
    {
        public static T DeepClone<T>(this T obj)
        {
#if USE_MEMORYPACK
            return (T)MemoryPack.MemoryPackSerializer.Deserialize<T>(MemoryPack.MemoryPackSerializer.Serialize<T>(obj));
#elif USEJSON
            return (T)Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
#endif
        }
    }
}