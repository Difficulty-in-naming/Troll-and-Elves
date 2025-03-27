using System;
using System.Collections.Generic;
using EdgeStudio.Damage;
using EdgeStudio.Odin;
using EdgeStudio.R3;
using MoreMountains.TopDownEngine;
using Panthea.Utils;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EdgeStudio.Weapons
{
    /// <summary>
    /// 一个专门用于创建各种投射物武器的武器类，从散弹枪到机关枪，再到等离子枪或火箭发射器
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Projectile Weapon")]
    public class ProjectileWeapon : Weapon
    {
        [ColorFoldout("Projectiles"), Tooltip("投射物生成的偏移位置")] 
        public Vector2 ProjectileSpawnOffset = Vector2.zero;

        [ColorFoldout("Projectiles"), Tooltip("在没有角色所有者的情况下，投射物的默认方向")]
        public Vector2 DefaultProjectileDirection = Vector2.right;

        [ColorFoldout("Projectiles"), Tooltip("每次射击生成的投射物数量")] 
        public int ProjectilesPerShot = 1;

        [ColorFoldout("Projectiles"), Header("Spawn Transforms"),
         Tooltip("可用作生成点的变换列表，替代ProjectileSpawnOffset。如果为空则忽略")]
        public List<Transform> SpawnTransforms = new List<Transform>();
        public enum SpawnTransformsModes { Random, Sequential }

        [ColorFoldout("Projectiles"),
         Tooltip("生成变换的选定模式。Sequential将按顺序遍历列表，而Random将在每次射击时随机选择一个")]
        public SpawnTransformsModes SpawnTransformsMode = SpawnTransformsModes.Sequential;

        [ColorFoldout("Projectiles"), Header("Spread"), Tooltip("生成投射物时在每个角度上随机（或非随机）应用的散布度（以度为单位）")]
        public Vector2 Spread = Vector2.zero;

        [ColorFoldout("Projectiles"), Tooltip("武器是否应该旋转以对齐散布角度")] 
        public bool RotateWeaponOnSpread = false;

        [ColorFoldout("Projectiles"), Tooltip("散布是否应该是随机的（如果不是，则会均匀分布）")] 
        public bool RandomSpread = true;

        [ColorFoldout("Projectiles"), ReadOnly, Tooltip("投射物的生成位置")]
        public Vector2 SpawnPosition = Vector2.zero;

        protected Vector2 mFlippedProjectileSpawnOffset;
        protected Vector2 mRandomSpreadDirection;
        protected bool mPoolInitialized = false;
        protected Transform mProjectileSpawnTransform;
        protected int mSpawnArrayIndex = 0;

        private Subject<Unit> OnProjectileFiredSubject = new Subject<Unit>();
        public Observable<Unit> OnProjectileFired => OnProjectileFiredSubject.AsObservable();
        /// <summary>
        /// 触发武器的测试方法
        /// </summary>
        [Button]
        protected virtual void TestShoot()
        {
            if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
            {
                WeaponInputStart();             
            }
            else
            {
                WeaponInputStop();
            }
        }
        
        /// <summary>
        /// 初始化此武器
        /// </summary>
        public override void Initialization()
        {
            base.Initialization();            
            mWeaponAim = GetComponent<WeaponAim>();

            if (!mPoolInitialized)
            {
                if (FlipWeaponOnCharacterFlip)
                {
                    mFlippedProjectileSpawnOffset = ProjectileSpawnOffset;
                    mFlippedProjectileSpawnOffset.y = -mFlippedProjectileSpawnOffset.y;
                }
                mPoolInitialized = true;
            }
        }

        /// <summary>
        /// 每次使用武器时调用
        /// </summary>
        public override void WeaponUse()
        {
            base.WeaponUse();

            DetermineSpawnPosition();

            for (int i = 0; i < ProjectilesPerShot; i++)
            {
                SpawnProjectile(SpawnPosition, i, ProjectilesPerShot, true);
                PlaySpawnFeedbacks();
                OnProjectileFiredSubject.OnNext(Unit.Default);
            }
        }

        /// <summary>
        /// 生成新对象并设置其位置/大小
        /// </summary>
        public virtual GameObject SpawnProjectile(Vector2 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            // 我们从池中获取下一个对象并确保它不为空
            //Todo 准备修复
            /*GameObject nextGameObject = ObjectPooler.GetPooledGameObject();

            // 必要的检查
            if (nextGameObject == null) { return null; }
            if (nextGameObject.GetComponent<MMPoolableObject>() == null)
            {
                throw new Exception(gameObject.name + " 正试图生成没有PoolableObject组件的对象。");
            }
            // 我们设置对象位置
            nextGameObject.transform.position = spawnPosition;
            if (mProjectileSpawnTransform != null)
            {
                nextGameObject.transform.position = mProjectileSpawnTransform.position;
            }
            // 我们设置其方向
            Projectile projectile = nextGameObject.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetWeapon(this);
                if (Owner != null)
                {
                    projectile.SetOwner(Owner.gameObject);
                }

                projectile.Speed = BulletSpeed;
            }
            var layer = LayerManager.ObstaclesLayerMask + (CharacterHandleWeapon.Character.CharacterType == CharacterTypes.AI ? LayerManager.PlayerLayerMask : LayerManager.EnemiesLayerMask);
            var damageOnTouch = nextGameObject.GetComponent<DamageOnTouch>();
            damageOnTouch.TargetLayerMask = layer;
            damageOnTouch.MinDamageCaused = damageOnTouch.MaxDamageCaused = Damage;
            
            nextGameObject.GetComponent<PreventPassingThrough2D>().ObstaclesLayerMask = layer;
            // 我们激活对象
            nextGameObject.gameObject.SetActive(true);

            if (projectile != null)
            {
                if (RandomSpread)
                {
                    mRandomSpreadDirection.x = UnityEngine.Random.Range(-Spread.x, Spread.x);
                    mRandomSpreadDirection.y = UnityEngine.Random.Range(-Spread.y, Spread.y);
                    mRandomSpreadDirection.z = UnityEngine.Random.Range(-Spread.z, Spread.z);
                }
                else
                {
                    if (totalProjectiles > 1)
                    {
                        mRandomSpreadDirection.x = MathUtils.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.x, Spread.x);
                        mRandomSpreadDirection.y = MathUtils.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.y, Spread.y);
                        mRandomSpreadDirection.z = MathUtils.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.z, Spread.z);
                    }
                    else
                    {
                        mRandomSpreadDirection = Vector2.zero;
                    }
                }

                Quaternion spread = Quaternion.Euler(mRandomSpreadDirection);

                if (Owner == null)
                {
                    projectile.SetDirection(spread * transform.rotation * DefaultProjectileDirection, transform.rotation, true);
                }
                else
                {
                    Vector2 newDirection = (spread * transform.right) * (Flipped ? -1 : 1);
                    projectile.SetDirection(newDirection, spread * transform.rotation, true);
                }                

                if (RotateWeaponOnSpread)
                {
                    this.transform.rotation = this.transform.rotation * spread;
                }
            }

            if (triggerObjectActivation)
            {
                if (nextGameObject.GetComponent<MMPoolableObject>() != null)
                {
                    nextGameObject.GetComponent<MMPoolableObject>().TriggerOnSpawnComplete();
                }
            }
            return (nextGameObject);*/
            return null;
        }

        /// <summary>
        /// 此方法负责在投射物生成时播放反馈
        /// </summary>
        protected virtual void PlaySpawnFeedbacks()
        {
            mSpawnArrayIndex++;
            if (mSpawnArrayIndex >= SpawnTransforms.Count)
            {
                mSpawnArrayIndex = 0;
            }
        }

        /// <summary>
        /// 设置强制投射物生成位置
        /// </summary>
        /// <param name="newSpawnTransform"></param>
        public virtual void SetProjectileSpawnTransform(Transform newSpawnTransform)
        {
            mProjectileSpawnTransform = newSpawnTransform;
        }

        /// <summary>
        /// 根据生成偏移和武器是否翻转来确定生成位置
        /// </summary>
        public virtual void DetermineSpawnPosition()
        {
            if (Flipped)
            {
                if (FlipWeaponOnCharacterFlip)
                {
                    SpawnPosition = this.transform.position - this.transform.rotation * mFlippedProjectileSpawnOffset;
                }
                else
                {
                    SpawnPosition = this.transform.position - this.transform.rotation * ProjectileSpawnOffset;
                }
            }
            else
            {
                SpawnPosition = this.transform.position + this.transform.rotation * ProjectileSpawnOffset;
            }

            if (WeaponUseTransform != null)
            {
                SpawnPosition = WeaponUseTransform.position;
            }

            if (SpawnTransforms.Count > 0)
            {
                if (SpawnTransformsMode == SpawnTransformsModes.Random)
                {
                    mSpawnArrayIndex = Random.Range(0, SpawnTransforms.Count);
                    SpawnPosition = SpawnTransforms[mSpawnArrayIndex].position;
                }
                else
                {
                    SpawnPosition = SpawnTransforms[mSpawnArrayIndex].position;
                }
            }
        }

        /// <summary>
        /// 当武器被选中时，在生成位置画一个圆
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            DetermineSpawnPosition();

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(SpawnPosition, 0.2f);  
        }

        public void OnMMEvent(TopDownEngineEvent engineEvent)
        {
            switch (engineEvent.EventType)
            {
                case TopDownEngineEventTypes.LevelStart:
                    mPoolInitialized = false;
                    Initialization();
                    break;
            }
        }
        
        protected virtual void OnEnable()
        {
            TopDownEngineEvent.Event.Subscribe(OnMMEvent).AddToDisable(CachedGameObject);
        }
    }
}