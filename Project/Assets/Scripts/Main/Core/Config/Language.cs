using UnityEngine;

namespace EdgeStudio.Config
{
    public struct Language
    {
        public string Key { get; }
        public string Name { get; }
        public int Id { get; }
#if UNITY_EDITOR
        public string EditorName { get; }
#endif
        private Language(string key, string name, int id, string editorName)
        {
            Key = key;
            Name = name;
            Id = id;
#if UNITY_EDITOR
            EditorName = editorName;
#endif
        }

        public static Language None = new("none", "none", 0, "错误");
        public static Language English = new("en", "English", 1, "英语");
        public static Language Chinese = new("zh-cn", "简体中文", 2, "简体中文");
        public static Language Chinese_tw = new("zh-tw", "繁體中文", 3, "繁体中文");
        public static Language French = new("fr", "français", 4, "法语");
        public static Language Italian = new("it", "italiano", 5, "意大利语");
        public static Language German = new("de", "Deutsche", 6, "德语");
        public static Language Spanish = new("es", "Español", 7, "西班牙语");
        public static Language Dutch = new("en", "Dutch", 8, "荷兰语");
        public static Language Russian = new("en", "Russian", 9, "俄语");
        public static Language Korean = new("en", "Korean", 10, "韩语");
        public static Language Japanese = new("en", "Japanese", 11, "日语");
        public static Language Hungarian = new("en", "Hungarian", 12, "匈牙利语");
        public static Language Portugese = new("pt", "Português", 13, "葡萄牙语");
        public static Language Thai = new("th", "ไทย", 14, "泰语"); //th
        public static Language Indonesian = new("id", "bahasa Indonesia", 15, "印度尼西亚语");
        public static readonly Language[] All = { Chinese, Chinese_tw, English /*, Italian, Spanish, French, German, Portugese, Thai, Indonesian*/ };

        public static Language CurrentLanguage { get; set; } = Chinese;

        public static Language Map(int id)
        {
            foreach (var node in All)
            {
                if (node.Id == id)
                    return node;
            }

            return CurrentLanguage;
        }

        public static Language Map(string key)
        {
            foreach (var node in All)
            {
                if (node.Key == key)
                    return node;
            }

            return CurrentLanguage;
        }

        public static Language MapFromName(string name)
        {
            foreach (var node in All)
            {
                if (node.Name == name)
                    return node;
            }

            return None;
        }

        public static void UseSystemLanguage()
        {
            if (Application.systemLanguage is SystemLanguage.Chinese or SystemLanguage.ChineseSimplified)
                CurrentLanguage = Chinese;
            else if (Application.systemLanguage is SystemLanguage.ChineseTraditional)
                CurrentLanguage = Chinese_tw;
            else
                CurrentLanguage = English;
        }
        
        public static bool operator ==(Language left, Language right) => left.Id == right.Id;

        public static bool operator !=(Language left, Language right) => left.Id != right.Id;

        public override bool Equals(object obj)
        {
            if (obj is Language other)
            {
                return this.Id == other.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}