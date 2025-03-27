#if UNITY_EDITOR || USEJSON
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EdgeStudio.Config
{
    public static class EdgeStudioJsonConverter
    {
        public static readonly JsonConverter[] JsonConverters = {
            new StringEnumConverter(),
            new RangeItemQuantityConverter(),
            new RangeItemQuantityArrayConverter(),
            new ItemQuantityConverter(),
            new ItemQuantityArrayConverter(),
            new KeyPairConverter<int,int>(),
            new KeyPairConverter<string,int>(),
            new KeyPairConverter<int,string>(),
            new KeyPairArrayConverter<int,int>(),
            new KeyPairArrayConverter<string,int>(),
            new KeyPairArrayConverter<int,string>(),
            new Vector2Converter(),
            new Vector2ArrayConverter(),
            new DynamicParametersConverter(),
            new BitArrayConverter(),
            new BitArrayArrayConverter(),
            new BitArrayListConverter(),
            new DateTimeConverter(),
            new LocalizedStringConverter(),
            new LocalizedStringArrayConverter(),
            new LocalizedStringArrayListConverter(),
            new ConditionValueConverter(),
            new Vector2IntArrayConverter(),
            new Vector2IntConverter(),
            
            //////////这里开始是ProjectJsonConverter////////
            new WeaponTagConverter(),
            new WeaponTypeConverter(),
        };
    }
}
#endif