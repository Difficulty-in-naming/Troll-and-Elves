using System;

namespace EdgeStudio.Config
{
    [Serializable]
    public struct WeaponTag
    {
        public string Key;
        public WeaponTag(string key) => Key = key;

        public static WeaponTag PGW = new("PGW");
    }
}