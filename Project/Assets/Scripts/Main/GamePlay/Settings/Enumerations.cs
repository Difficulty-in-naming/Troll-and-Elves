using EdgeStudio;
using Meziantou.Framework.Annotations;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// [assembly: FastEnumToString(typeof(PlayerId))]
namespace EdgeStudio
{
    public enum DBIndexMap { 
        Player = 0,
        Item = 1,
        Shop = 2,
    }
    
    public enum GuideId { }
    public enum FacingDirections { Left, Right }

    [Flags]
    public enum CharacterTeam
    {
        Player1 = 1 << 0,
        Player2 = 1 << 1,
        Player3 = 1 << 2,
        Player4 = 1 << 3,
        Player5 = 1 << 4,
        Player6 = 1 << 5,
        Player7 = 1 << 6,
        Player8 = 1 << 7,
        Player9 = 1 << 8,
        Player10 = 1 << 9,
        Player11 = 1 << 10,
        Player12 = 1 << 11,
    }

    [Flags]
    public enum CharacterTag
    {
        生物 = 1 << 0,
        召唤生物 = 1 << 1,
        中立 = 1 << 2,
        机械 = 1 << 3
    }

    public enum WeaponStates { WeaponIdle, WeaponStart, WeaponDelayBeforeUse, WeaponUse, WeaponDelayBetweenUses, WeaponStop, WeaponReloadNeeded, WeaponReloadStart, WeaponReload, WeaponReloadStop, WeaponInterrupted, WeaponSwitching }
    public enum DamageTypeModes { BaseDamage, TypedDamage }
    public enum LifeCycleEventTypes { Death, Revive }
    public enum TopDownEngineEventTypes
    {
        /// <summary> 当关卡开始时触发，由 LevelManager 触发 </summary>
        LevelStart,
        /// <summary> 当角色被交换时触发 </summary>
        CharacterSwap,
        /// <summary> 当角色被切换时触发 </summary>
        CharacterSwitch,
    }
    public enum CameraEventTypes { SetTargetCharacter, StartFollowing, StopFollowing, RefreshPosition}
    [Flags]
    public enum TriggerAndCollisionMask { IgnoreAll = 0, OnTriggerEnter2D = 1 << 0, OnTriggerStay2D = 1 << 1, All = OnTriggerEnter2D | OnTriggerStay2D}
    public enum KnockbackStyles { NoKnockback, AddForce }
    public enum KnockbackDirections { BasedOnOwnerPosition, BasedOnSpeed, BasedOnDirection, BasedOnScriptDirection }
    public enum DamageDirections { BasedOnOwnerPosition, BasedOnVelocity, BasedOnScriptDirection }
    public enum DashModes { Fixed, MainMovement, SecondaryMovement, MousePosition, Script }
    public enum DashSpaces { World, Local }
}