using EdgeStudio.Config;

namespace EdgeStudio.DataStruct
{
    public struct LocalizedString
    {
        public string Key;
        public LocalizedString(string key) => Key = key;
        public static implicit operator string(LocalizedString localizedString) => LocalizationProperty.Read(localizedString.Key);
        public static implicit operator LocalizedString(string key) => new(key);
        
        public LocalizationProperty GetProperty => LocalizationProperty.Get(Key);

        public override string ToString() => LocalizationProperty.Read(Key);
    }
}