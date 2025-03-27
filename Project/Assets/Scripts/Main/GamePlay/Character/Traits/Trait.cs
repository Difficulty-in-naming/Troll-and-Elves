using EdgeStudio.Abilities;
using EdgeStudio.Odin;
using EdgeStudio.Weapons;
using UnityEngine;

namespace EdgeStudio.Traits
{
    [CreateAssetMenu(fileName = "New Trait", menuName = "Edge Studio/Traits/Trait")]
    public class Trait : ScriptableObject
    {
        [ColorFoldout("基本信息")]
        public string TraitName;
        [ColorFoldout("基本信息")]
        public string Description;
        [ColorFoldout("基本信息")]
        [Tooltip("特性的唯一标识符")]
        public string TraitID;
        
        [ColorFoldout("基本信息")]
        public enum TraitType { Innate, Acquired }
        [ColorFoldout("基本信息")]
        public TraitType Type;
        
        [ColorFoldout("基本信息")]
        public enum TraitRarity { Common, Uncommon, Rare, Epic, Legendary }
        [ColorFoldout("基本信息")]
        public TraitRarity Rarity;
        
        [ColorFoldout("图标")]
        public Sprite Icon;
        
        // 运行时引用
        protected Character character;
        protected CharacterTrait mTrait;
        
        // 初始化特性
        public virtual void Initialize(Character character, CharacterTrait trait)
        {
            this.character = character;
            this.mTrait = trait;
        }
        
        // 当特性被添加到角色时
        public virtual void OnTraitAdded() { }
        
        // 当特性被移除时
        public virtual void OnTraitRemoved() { }
        
        // 每帧更新
        public virtual void OnUpdate() { }
        
        // 事件响应方法
        public virtual void OnEnemyKilled(GameObject enemy, string enemyType) { }
        public virtual void OnDamageDealt(GameObject target, float damage) { }
        public virtual void OnDamageTaken(GameObject source, float damage) { }
        
        // 工具方法，修改角色属性
        protected void ModifyHealth(float amount)
        {
            if (character.CharacterHealth != null)
            {
                character.CharacterHealth.SetHealth(amount);
            }
        }
        
        protected void ModifyMaxHealth(float amount)
        {
            if (character.CharacterHealth != null)
            {
                var multiplier = character.CharacterHealth.MaximumHealth / character.CharacterHealth.CurrentHealth;
                character.CharacterHealth.MaximumHealth += amount;
                character.CharacterHealth.SetHealth(character.CharacterHealth.MaximumHealth * multiplier);
            }
        }
        
        protected void ModifyMovementSpeed(float multiplier)
        {
            CharacterMovement movement = character.FindAbility<CharacterMovement>();
            if (movement != null)
            {
                movement.MovementSpeedMultiplier *= multiplier;
            }
        }
        
        protected void ModifyProjectileCount(int additionalProjectiles)
        {
            CharacterHandleWeapon handleWeapon = character.FindAbility<CharacterHandleWeapon>();
            if (handleWeapon != null && handleWeapon.CurrentWeapon is ProjectileWeapon projectileWeapon)
            {
                projectileWeapon.ProjectilesPerShot += additionalProjectiles;
            }
        }
        
        protected void ModifyAttackSpeed(float multiplier)
        {
            CharacterHandleWeapon handleWeapon = character.FindAbility<CharacterHandleWeapon>();
            if (handleWeapon != null && handleWeapon.CurrentWeapon != null)
            {
                handleWeapon.CurrentWeapon.TimeBetweenUses /= multiplier;
            }
        }
        
        protected void ModifyDamage(float multiplier)
        {
            CharacterHandleWeapon handleWeapon = character.FindAbility<CharacterHandleWeapon>();
            if (handleWeapon != null && handleWeapon.CurrentWeapon != null)
            {
                handleWeapon.CurrentWeapon.Damage *= multiplier;
            }
        }
    }
}