namespace EdgeStudio.Tools.EditorJson
{
#if UNITY_EDITOR && USE_MEMORYPACK
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EdgeStudio.Tools
{
    public class MemoryPackIgnoreContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (member.IsDefined(typeof(MemoryPackIgnoreAttribute), true))
            {
                property.Ignored = true;
            }
            return property;
        }
    }
}
#endif
}