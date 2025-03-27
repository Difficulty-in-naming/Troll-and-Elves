#if UNITY_EDITOR || USEJSON
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using EdgeStudio.DataStruct;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Panthea.Utils;
using UnityEngine;
using RangeInt = Panthea.Utils.RangeInt;


namespace EdgeStudio.Config
{
    public static class JsonDotNetUtility
    {
        public static string GetPosition(this JsonReader reader)
        {
            if (reader is JsonTextReader textReader)
                return $"{textReader.LineNumber},{textReader.LinePosition}):";
            else
                return $"{reader.Path}):";
        }
    }

    public class ConditionValueConverter : JsonConverter<ConditionValue>
    {
        public override void WriteJson(JsonWriter writer, ConditionValue value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override ConditionValue ReadJson(JsonReader reader, Type objectType, ConditionValue existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value != null)
            {
                var str = reader.Value.ToString();
                return new ConditionValue(str);
            }

            return new ConditionValue(null);
        }
    }
    
    public class LocalizedStringConverter : JsonConverter<LocalizedString>
    {
        public override void WriteJson(JsonWriter writer, LocalizedString value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override LocalizedString ReadJson(JsonReader reader, Type objectType, LocalizedString existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var str = reader.Value.ToString();
            return new LocalizedString(str);
        }
    }
    
    public class LocalizedStringArrayConverter : JsonConverter<LocalizedString[]>
    {
        public override void WriteJson(JsonWriter writer, LocalizedString[] value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // 不需要实现 WriteJson
        }

        public override LocalizedString[] ReadJson(JsonReader reader, Type objectType, LocalizedString[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var split = reader.Value.ToString().Split(',');
            var s = new LocalizedString[split.Length];
            for (int i = 0; i < split.Length; i++)
            {
                s[i] = split[i];
            }

            return s;
        }
    }
    
    public class LocalizedStringArrayListConverter : JsonConverter<List<LocalizedString[]>>
    {
        public override void WriteJson(JsonWriter writer, List<LocalizedString[]> value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // 不需要实现 WriteJson
        }

        public override List<LocalizedString[]> ReadJson(JsonReader reader, Type objectType, List<LocalizedString[]> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var result = new List<LocalizedString[]>();

            var jsonString = reader.Value.ToString();
            var arrayStrings = jsonString.Split(new[] { "],[" }, StringSplitOptions.None);
            foreach (var arrayString in arrayStrings)
            {
                var trimmedArrayString = arrayString.Trim('[', ']');
                var split = trimmedArrayString.Split(',');
                var localizedStringArray = new LocalizedString[split.Length];
                for (int i = 0; i < split.Length; i++)
                {
                    localizedStringArray[i] = new LocalizedString(split[i].Trim('"'));
                }
                result.Add(localizedStringArray);
            }

            return result;
        }
    }
    
    public class RangeItemQuantityConverter : JsonConverter<RangeItemQuantity>
    {
        public override RangeItemQuantity ReadJson(JsonReader reader, System.Type objectType, RangeItemQuantity existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return Handle(reader.Value?.ToString(),reader);
        }

        public static RangeItemQuantity Handle(string input,JsonReader reader)
        {
            if (string.IsNullOrEmpty(input))
                return new RangeItemQuantity { Id = 0, Num = new RangeInt(0,0),Weight = 1};
            string[] parts = input.Split(':');
            if (parts.Length == 2)
            {
                int id = int.Parse(parts[0]);
                var rangeInt = new RangeInt();
                if (parts[1].Contains("-"))
                {
                    var range = parts[1].Split("-", StringSplitOptions.RemoveEmptyEntries);
                    int min = int.Parse(range[0]);
                    int max = int.Parse(range[1]);
                    rangeInt.Min = min;
                    rangeInt.Max = max;
                }
                else
                {
                    int min = int.Parse(parts[1]);
                    rangeInt.Min = min;
                    rangeInt.Max = min;
                }

                return new RangeItemQuantity { Id = id, Num = rangeInt, Weight = 1 };
            }

            if (parts.Length == 3)
            {
                int id = int.Parse(parts[0]);
                var rangeInt = new RangeInt();
                int weight = int.Parse(parts[2]);
                if (parts[1].Contains("-"))
                {
                    var range = parts[1].Split("-", StringSplitOptions.RemoveEmptyEntries);
                    int min = int.Parse(range[0]);
                    int max = int.Parse(range[1]);
                    rangeInt.Min = min;
                    rangeInt.Max = max;
                }
                else
                {
                    int min = int.Parse(parts[1]);
                    rangeInt.Min = min;
                    rangeInt.Max = min;
                }

                return new RangeItemQuantity { Id = id, Num = rangeInt, Weight = weight };
            }

            throw new JsonSerializationException($"格式错误,错误的内容为{input} at {reader.GetPosition()}");
        }

        public override void WriteJson(JsonWriter writer, RangeItemQuantity value, JsonSerializer serializer) { }
    }
    
    public class RangeItemQuantityArrayConverter : JsonConverter<RangeItemQuantity[]>
    {
        public override void WriteJson(JsonWriter writer, RangeItemQuantity[] value, JsonSerializer serializer) { }

        public override RangeItemQuantity[] ReadJson(JsonReader reader, Type objectType, RangeItemQuantity[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = new List<RangeItemQuantity>();
            string input = reader.Value?.ToString();
            if (string.IsNullOrEmpty(input))
                return list.ToArray();
            var split = input.Split(',',StringSplitOptions.RemoveEmptyEntries);
            foreach (var node in split)
            {
                var item = RangeItemQuantityConverter.Handle(node, reader);
                list.Add(item);
            }

            return list.ToArray();
        }
    }
    
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            // 将 DateTime 序列化为数组格式
            writer.WriteStartArray();
            writer.WriteValue(value.Year);
            writer.WriteValue(value.Month);
            writer.WriteValue(value.Day);
            writer.WriteValue(value.Hour);
            writer.WriteValue(value.Minute);
            writer.WriteValue(value.Second);
            writer.WriteEndArray();
        }

        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // 读取字符串并解析为 DateTime
            string dateString = reader.Value?.ToString();
            if (string.IsNullOrEmpty(dateString))
            {
                throw new JsonSerializationException("Date string cannot be null or empty.");
            }

            string[] parts = dateString.Split(',');
            if (parts.Length != 6)
            {
                throw new JsonSerializationException("Expected a string in the format 'year,month,day,hour,minute,second'.");
            }

            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);
            int hour = int.Parse(parts[3]);
            int minute = int.Parse(parts[4]);
            int second = int.Parse(parts[5]);

            return new DateTime(year, month, day, hour, minute, second);
        }
    }
    
    public class ItemQuantityConverter : JsonConverter<ItemQuantity>
    {
        public override ItemQuantity ReadJson(JsonReader reader, System.Type objectType, ItemQuantity existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string input = reader.Value?.ToString();
            if (string.IsNullOrEmpty(input))
                return new ItemQuantity { Id = "", Num = 0 };
            string[] parts = input.Split(':');
            if (parts.Length != 2)
                throw new JsonSerializationException("Invalid input format");

            var id = parts[0];
            int num = int.Parse(parts[1]);

            return new ItemQuantity { Id = id, Num = num };
        }

        public override void WriteJson(JsonWriter writer, ItemQuantity value, JsonSerializer serializer) { }
    }
    
    public class ItemQuantityArrayConverter : JsonConverter<ItemQuantity[]>
    {
        public override void WriteJson(JsonWriter writer, ItemQuantity[] value, JsonSerializer serializer) { }

        public override ItemQuantity[] ReadJson(JsonReader reader, Type objectType, ItemQuantity[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = new List<ItemQuantity>();
            string input = reader.Value?.ToString();
            if (string.IsNullOrEmpty(input))
                return list.ToArray();
            var split = input.Split(',',StringSplitOptions.RemoveEmptyEntries);
            foreach (var node in split)
            {
                string[] parts = node.Split(':');
                if (parts.Length != 2)
                    throw new JsonSerializationException($"格式错误,错误的内容为{input} at {reader.GetPosition()}");

                var id = parts[0];
                int num = int.Parse(parts[1]);
                list.Add(new ItemQuantity{Id = id, Num = num});
            }

            return list.ToArray();
        }
    }
    
    
    public class KeyPairConverter<T1,T2> : JsonConverter<KeyPair<T1,T2>>
    {
        public override void WriteJson(JsonWriter writer, KeyPair<T1, T2> value, JsonSerializer serializer) { }

        public override KeyPair<T1, T2> ReadJson(JsonReader reader, Type objectType, KeyPair<T1, T2> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string input = reader.Value?.ToString();
            if (string.IsNullOrEmpty(input))
                return new KeyPair<T1, T2>(default, default);
            string[] parts = input.Split(':');
            if (parts.Length != 2)
                throw new JsonSerializationException($"Invalid input format at {reader.GetPosition()}");
            var t1 = TypeDescriptor.GetConverter(typeof(T1)).ConvertFromString(parts[0]);
            var t2 = TypeDescriptor.GetConverter(typeof(T2)).ConvertFromString(parts[1]);

            return new KeyPair<T1, T2>((T1)t1, (T2)t2);
        }
    }
    
    public class KeyPairArrayConverter<T1,T2> : JsonConverter<KeyPair<T1,T2>[]>
    {
        public override void WriteJson(JsonWriter writer, KeyPair<T1, T2>[] value, JsonSerializer serializer) { }

        public override KeyPair<T1, T2>[] ReadJson(JsonReader reader, Type objectType, KeyPair<T1, T2>[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = new List<KeyPair<T1, T2>>();
            string input = reader.Value?.ToString();
            if (string.IsNullOrEmpty(input))
                return Array.Empty<KeyPair<T1, T2>>();
            var split = input.Split(',',StringSplitOptions.RemoveEmptyEntries);
            foreach (var node in split)
            {
                string[] parts = node.Split(':');
                if (parts.Length != 2)
                    throw new JsonSerializationException($"Invalid input format at {reader.GetPosition()}");

                var t1 = TypeDescriptor.GetConverter(typeof(T1)).ConvertFromString(parts[0]);
                var t2 = TypeDescriptor.GetConverter(typeof(T2)).ConvertFromString(parts[1]);
                list.Add(new KeyPair<T1, T2>((T1)t1, (T2)t2));
            }

            return list.ToArray();
        }
    }
    
    public class DynamicParametersConverter : JsonConverter<DynamicParameters>
    {
        public override void WriteJson(JsonWriter writer, DynamicParameters value, JsonSerializer serializer) { }

        public override DynamicParameters ReadJson(JsonReader reader, Type objectType, DynamicParameters existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var str = reader.Value?.ToString();
            return new DynamicParameters{String = str};
        }
    }
    
    public class BitArrayConverter : JsonConverter<BitArray>
    {
        public override BitArray ReadJson(JsonReader reader, Type objectType, BitArray existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var bools = serializer.Deserialize<int[]>(reader);
            return bools == null ? null : BitArrayUtils.ToBitArray(bools);
        }

        public override void WriteJson(JsonWriter writer, BitArray value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToIntArray());
        }
    }
    
    public class BitArrayArrayConverter : JsonConverter<BitArray[]>
    {
        public override BitArray[] ReadJson(JsonReader reader, Type objectType, BitArray[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var bools = serializer.Deserialize<int[][]>(reader);
            if (bools == null)
                return null;
            var bitArray = new BitArray[bools.Length];
            for (var index = 0; index < bools.Length; index++)
            {
                var node = bools[index];
                bitArray[index] = BitArrayUtils.ToBitArray(node);
            }

            return bitArray;
        }

        public override void WriteJson(JsonWriter writer, BitArray[] value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            var list = value.Select(node => node?.ToIntArray());
            serializer.Serialize(writer, list);
        }
    }
    
    public class BitArrayListConverter : JsonConverter<List<BitArray>>
    {
        public override List<BitArray> ReadJson(JsonReader reader, Type objectType, List<BitArray> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var bools = serializer.Deserialize<byte[][]>(reader);
            if (bools == null)
                return null;
            var bitArray = new List<BitArray>(bools.Length);
            for (var index = 0; index < bools.Length; index++)
            {
                var node = bools[index];
                bitArray.Add(BitArrayUtils.ToBitArray(node));
            }

            return bitArray;
        }

        public override void WriteJson(JsonWriter writer, List<BitArray> value, JsonSerializer serializer)
        {
            var list = value.Select(node => node.ToByteArray());
            serializer.Serialize(writer, list);
        }
    }
    
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer) { }

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var input = reader.Value?.ToString();
            if (string.IsNullOrEmpty(input))
                return Vector2.zero;
            var parts = input.Split(',');
            if (parts.Length != 2)
                throw new JsonSerializationException($"Invalid input format at {reader.GetPosition()}");

            return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
        }
    }
    
    public class Vector2ArrayConverter : JsonConverter<Vector2[]>
    {
        public override void WriteJson(JsonWriter writer, Vector2[] value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var result = string.Join(",", value.Select(v => $"[{v.x},{v.y}]"));
            writer.WriteValue(result);
        }

        public override Vector2[] ReadJson(JsonReader reader, Type objectType, Vector2[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var input = reader.Value?.ToString();
            if (string.IsNullOrEmpty(input))
                return null;

            // 移除所有空格
            input = input.Replace(" ", "");
        
            // 分割每个向量
            var vectorStrings = input.Split(new[] { "],[" }, StringSplitOptions.RemoveEmptyEntries);
        
            var result = new List<Vector2>();
            foreach (var vectorStr in vectorStrings)
            {
                // 清理方括号
                var cleanStr = vectorStr
                    .TrimStart('[')
                    .TrimEnd(']');
            
                var components = cleanStr.Split(',');
                if (components.Length != 2)
                    throw new JsonSerializationException($"Invalid vector format: {vectorStr}");

                if (!float.TryParse(components[0], out float x) || !float.TryParse(components[1], out float y))
                    throw new JsonSerializationException($"Invalid number format in vector: {vectorStr}");

                result.Add(new Vector2(x, y));
            }

            return result.ToArray();
        }
    }
    
    
    public class Vector2IntConverter : JsonConverter<Vector2Int>
    {
        public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // 读取JSON数组
            JArray array = JArray.Load(reader);
        
            // 确保数组包含两个元素
            if (array.Count != 2)
            {
                throw new JsonException("Vector2Int格式不正确，应为[x,y]");
            }

            // 读取x和y值
            int x = array[0].Value<int>();
            int y = array[1].Value<int>();
        
            // 返回新的Vector2Int对象
            return new Vector2Int(x, y);
        }

        public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
        {
            // 写入为JSON数组格式[x,y]
            writer.WriteStartArray();
            writer.WriteValue(value.x);
            writer.WriteValue(value.y);
            writer.WriteEndArray();
        }
    }
    
    public class Vector2IntArrayConverter : JsonConverter<Vector2Int[]>
    {
        public override Vector2Int[] ReadJson(JsonReader reader, Type objectType, Vector2Int[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // 读取JSON数组
            JArray array = JArray.Load(reader);
            Vector2Int[] result = new Vector2Int[array.Count];

            for (int i = 0; i < array.Count; i++)
            {
                // 每个元素应该是一个包含两个整数的数组
                JArray item = (JArray)array[i];
                if (item.Count != 2)
                {
                    throw new JsonException($"Vector2Int数组的第{i}个元素格式不正确，应为[x,y]");
                }

                int x = item[0].Value<int>();
                int y = item[1].Value<int>();
                result[i] = new Vector2Int(x, y);
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, Vector2Int[] value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            foreach (Vector2Int v in value)
            {
                writer.WriteStartArray();
                writer.WriteValue(v.x);
                writer.WriteValue(v.y);
                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}
#endif
