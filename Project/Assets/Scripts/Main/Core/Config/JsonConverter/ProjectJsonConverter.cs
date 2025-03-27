using System;
using Newtonsoft.Json;

namespace EdgeStudio.Config
{
    public class WeaponTypeConverter : JsonConverter<WeaponType>
    {
        public override void WriteJson(JsonWriter writer, WeaponType value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // 不需要实现 WriteJson
        }

        public override WeaponType ReadJson(JsonReader reader, Type objectType, WeaponType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.Value != null ? new WeaponType(reader.Value.ToString()) : WeaponType.Unknown;
        }
    }
    
    public class WeaponTagConverter : JsonConverter<WeaponTag[]>
    {
        public override void WriteJson(JsonWriter writer, WeaponTag[] value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // 不需要实现 WriteJson
        }

        public override WeaponTag[] ReadJson(JsonReader reader, Type objectType, WeaponTag[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value != null)
            {
                var split = reader.Value.ToString().Split(',');
                var s = new WeaponTag[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    s[i] = new WeaponTag(split[i]);
                }

                return s;
            }

            return Array.Empty<WeaponTag>();
        }
    }
}