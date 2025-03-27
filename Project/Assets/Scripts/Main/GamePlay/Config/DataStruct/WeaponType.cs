using System;

namespace EdgeStudio.Config
{
    [Serializable]
    public struct WeaponType
    {
        public string Key;
        public WeaponType(string key)
        {
            Key = key;
        }
        public static WeaponType Unknown = new("Unknown");
        public static WeaponType SniperRifle = new("SniperRifle");
        public static WeaponType AssaultRifles = new("AssaultRifles");
        public static WeaponType SubmachineGuns = new("SubmachineGuns");
        public static WeaponType LightMachineGuns = new("LightMachineGuns");
        public static WeaponType Shotguns = new("Shotguns");
    }
}