using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace EdgeStudio.Config
{
    [Serializable]
    public class WeaponArgs
    {
        public float Damage;
        public float Accuracy;
        public float Recoil;
        public float Ergonomics;
        public float RateOfFire;
        public int Magazine;
        public string Caliber;
        public float[] Stock;
        public float[] Grip;
        public float[] Barrel;
        public float[] Handguard;
        public float[] AmmoStorage;
        public float[] Scope;
        public float[] MuzzleBrake;
        public float[] Accessory1;
        public float[] Accessory2;
        public float[] Accessory3;
        public float[] Accessory4;
        public WeaponType Type;
        public WeaponTag[] Tag;
        public int BurstCount;
        public float BurstBetweenShoot;
        public int ReloadTime;
        public int AmmoConsumedPerShot;
        public float ShootMovementMultiplier;
        public Vector2 WeaponAttachmentOffset;
        public Vector2 ProjectileSpawnOffset;
        public string UsePrefab;
        public float BulletSpeed;
        public float ExtendView;
        public string BulletPrefab;
        public string ShootSound;
        public string MagazineSound;
        public string CockingSound;
    }
}