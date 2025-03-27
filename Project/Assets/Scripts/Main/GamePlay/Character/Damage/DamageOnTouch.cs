using System.Collections.Generic;
using EdgeStudio.Odin;
using Panthea.Common;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace EdgeStudio.Damage
{
    /// <summary>
    /// 将此组件添加到物体上,它将对与其发生碰撞的物体造成伤害
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Damage/Damage On Touch")]
    public class DamageOnTouch : BetterMonoBehaviour
    {
        public const TriggerAndCollisionMask AllowedTriggerCallbacks = TriggerAndCollisionMask.OnTriggerEnter2D | TriggerAndCollisionMask.OnTriggerStay2D;

        [ColorFoldout("目标")]
        [InfoBox("<size=11>该组件将使物体对与其碰撞的物体造成伤害。在这里你可以定义哪些层会受到伤害(对于标准敌人,选择Player),造成多少伤害,以及在受击时应该对受伤物体施加多大的力。你也可以指定受击后的无敌时间应持续多久(以秒为单位)。</size>")]
        [Tooltip("将受到此物体伤害的层")]
        public LayerMask TargetLayerMask;

        [ColorFoldout("目标"), ReadOnly] [Tooltip("DamageOnTouch区域的所有者")]
        public GameObject Owner;

        [ColorFoldout("目标"), Tooltip(
             "定义在什么触发器上应用伤害,默认在进入和停留时(2D和3D都适用),但此字段允许你在需要时排除触发器")]
        public TriggerAndCollisionMask TriggerFilter = AllowedTriggerCallbacks;

        [ColorFoldout("造成伤害"), Tooltip("从玩家生命值中减去的最小伤害值")]
        public float MinDamageCaused = 10f;

        [ColorFoldout("造成伤害"), Tooltip("从玩家生命值中减去的最大伤害值")] public float MaxDamageCaused = 10f;
        [ColorFoldout("造成伤害"), Tooltip("一个类型化伤害定义列表,将在基础伤害之上应用")] public List<TypedDamage> TypedDamages;

        [ColorFoldout("造成伤害"), Tooltip("如何确定传递给Health伤害方法的伤害方向,通常对于移动的伤害区域(投射物)使用速度,对于近战武器使用所有者位置")]
        public DamageDirections DamageDirectionMode = DamageDirections.BasedOnVelocity;

        [ColorFoldout("击退"), Tooltip("造成伤害时应用的击退类型")] 
        public KnockbackStyles DamageCausedKnockbackType = KnockbackStyles.AddForce;

        [ColorFoldout("击退"), Tooltip("应用击退的方向")] public KnockbackDirections DamageCausedKnockbackDirection = KnockbackDirections.BasedOnOwnerPosition;

        [ColorFoldout("击退"), Tooltip("对受伤物体施加的力 - 这个力将根据你的击退方向模式旋转。例如在3D中,如果你想被推回相反方向,关注z分量,比如力为0,0,20")]
        public Vector2 DamageCausedKnockbackForce = new Vector2(10, 10);

        [ColorFoldout("无敌时间"), Tooltip("受击后的无敌帧持续时间(以秒为单位)")] 
        public float InvincibilityDuration = 0.5f;

        [ColorFoldout("持续伤害"), Tooltip("此接触伤害区域是否应该应用持续伤害")] 
        public bool RepeatDamageOverTime;

        [ColorFoldout("持续伤害"), Tooltip("如果在持续伤害模式下,伤害应重复多少次?")] [ShowIf("RepeatDamageOverTime")]
        public int AmountOfRepeats = 3;

        [ColorFoldout("持续伤害"), Tooltip("如果在持续伤害模式下,两次伤害之间的持续时间(以秒为单位)")] [ShowIf("RepeatDamageOverTime")]
        public float DurationBetweenRepeats = 1f;

        [ColorFoldout("持续伤害"), Tooltip("如果在持续伤害模式下,是否可以被中断(通过调用Health:InterruptDamageOverTime方法)")] [ShowIf("RepeatDamageOverTime")]
        public bool DamageOverTimeInterruptible = true;

        [ColorFoldout("持续伤害"), Tooltip("如果在持续伤害模式下,重复伤害的类型")] [ShowIf("RepeatDamageOverTime")]
        public DamageType RepeatedDamageType;

        [ColorFoldout("受到的伤害")]
        [InfoBox(
            "<size=11>在对碰撞的物体施加伤害后,你可以让这个物体伤害自己。" +
            "例如,子弹在击中墙壁后会爆炸。在这里你可以定义每次击中时它将受到多少伤害," +
            "或者仅在击中可造成伤害的物体或不可造成伤害的物体时受到伤害。注意这个物体也需要一个Health组件才能使这个功能有用。</size>")]
        [Tooltip("要应用受到伤害的Health组件。如果留空,将尝试在此物体上获取一个。")]
        public Health DamageTakenHealth;

        [ColorFoldout("受到的伤害"), Tooltip("每次受到的伤害量,无论我们碰撞的是否可造成伤害")] public float DamageTakenEveryTime;
        [ColorFoldout("受到的伤害"), Tooltip("与可造成伤害的物体碰撞时受到的伤害量")] public float DamageTakenDamageable;
        [ColorFoldout("受到的伤害"), Tooltip("与不可造成伤害的物体碰撞时受到的伤害量")] public float DamageTakenNonDamageable;
        [ColorFoldout("受到的伤害"), Tooltip("受到伤害时应用的击退类型")] public KnockbackStyles DamageTakenKnockbackType = KnockbackStyles.NoKnockback;
        [ColorFoldout("受到的伤害"), Tooltip("对受伤物体施加的力")] public Vector2 DamageTakenKnockbackForce = Vector2.zero;
        [ColorFoldout("受到的伤害"), Tooltip("受击后的无敌帧持续时间(以秒为单位)")] public float DamageTakenInvincibilityDuration = 0.5f;

        [ColorFoldout("反馈")]
        public UnityEvent<Health> HitDamageableEvent;

        [ColorFoldout("反馈")]
        public UnityEvent<GameObject> HitAnythingEvent;

        // 存储变量
        protected Vector2 mLastPosition, mLastDamagePosition, mVelocity, mKnockbackForce, mDamageDirection;
        protected Health mColliderHealth;
        protected TopDownController mTopDownController;
        protected TopDownController mColliderTopDownController;
        protected List<GameObject> mIgnoredGameObjects;
        protected CircleCollider2D mCircleCollider2D;
        protected BoxCollider2D mBoxCollider2D;
        protected bool mTwoD;
        protected bool mInitializedFeedbacks;
        protected Vector2 mPositionLastFrame;
        protected Vector2 mKnockbackScriptDirection;
        protected Vector2 mRelativePosition;
        protected Vector2 mDamageScriptDirection;
        protected Health mCollidingHealth;

        #region 初始化

        protected virtual void Awake() => Initialization();

        /// <summary>
        /// 在OnEnable时将开始时间设置为当前时间戳
        /// </summary>
        protected virtual void OnEnable()
        {
            mLastPosition = CachedTransform.position;
            mLastDamagePosition = CachedTransform.position;
        }

        /// <summary>
        /// 初始化忽略列表、反馈、碰撞体并获取组件
        /// </summary>
        public virtual void Initialization()
        {
            InitializeIgnoreList();
            GrabComponents();
            InitializeColliders();
            InitializeFeedbacks();
        }

        /// <summary>
        /// 存储组件
        /// </summary>
        protected virtual void GrabComponents()
        {
            if (DamageTakenHealth == null)
            {
                DamageTakenHealth = GetComponent<Health>();
            }

            mTopDownController = GetComponent<TopDownController>();
            mBoxCollider2D = GetComponent<BoxCollider2D>();
            mCircleCollider2D = GetComponent<CircleCollider2D>();
            mLastDamagePosition = CachedTransform.position;
        }

        /// <summary>
        /// 初始化碰撞体,根据需要将其设置为触发器
        /// </summary>
        protected virtual void InitializeColliders()
        {
            mTwoD = mBoxCollider2D != null || mCircleCollider2D != null;
            if (mBoxCollider2D != null)
            {
                mBoxCollider2D.isTrigger = true;
            }

            if (mCircleCollider2D != null)
            {
                mCircleCollider2D.isTrigger = true;
            }
        }

        /// <summary>
        /// 如果需要,初始化_ignoredGameObjects列表
        /// </summary>
        protected virtual void InitializeIgnoreList() => mIgnoredGameObjects ??= new List<GameObject>();

        /// <summary>
        /// 初始化反馈
        /// </summary>
        public virtual void InitializeFeedbacks()
        {
            if (mInitializedFeedbacks) return;

            mInitializedFeedbacks = true;
        }

        /// <summary>
        /// 在禁用时清除忽略列表
        /// </summary>
        protected virtual void OnDisable() => ClearIgnoreList();

        /// <summary>
        /// 在验证时确保我们的检查器同步
        /// </summary>
        protected virtual void OnValidate() => TriggerFilter &= AllowedTriggerCallbacks;

        #endregion
        
        /// <summary>
        /// 当击退处于脚本方向模式时,让你指定击退的方向
        /// </summary>
        /// <param name="newDirection"></param>
        public virtual void SetKnockbackScriptDirection(Vector2 newDirection)
        {
            mKnockbackScriptDirection = newDirection;
        }

        /// <summary>
        /// 当伤害方向处于脚本模式时,让你指定伤害的方向
        /// </summary>
        /// <param name="newDirection"></param>
        public virtual void SetDamageScriptDirection(Vector2 newDirection)
        {
            mDamageDirection = newDirection;
        }

        /// <summary>
        /// 将参数中设置的游戏对象添加到忽略列表中
        /// </summary>
        /// <param name="newIgnoredGameObject">新的要忽略的游戏对象</param>
        public virtual void IgnoreGameObject(GameObject newIgnoredGameObject)
        {
            InitializeIgnoreList();
            mIgnoredGameObjects.Add(newIgnoredGameObject);
        }

        /// <summary>
        /// 从忽略列表中移除参数中设置的对象
        /// </summary>
        /// <param name="ignoredGameObject">要移除的被忽略的游戏对象</param>
        public virtual void StopIgnoringObject(GameObject ignoredGameObject)
        {
            if (mIgnoredGameObjects != null) mIgnoredGameObjects.Remove(ignoredGameObject);
        }

        /// <summary>
        /// 清空忽略列表
        /// </summary>
        public virtual void ClearIgnoreList()
        {
            InitializeIgnoreList();
            mIgnoredGameObjects.Clear();
        }

        #region Loop

        /// <summary>
        /// 在最后一次更新期间,我们存储对象的位置和速度
        /// </summary>
        protected virtual void Update()
        {
            ComputeVelocity();
        }

        /// <summary>
        /// 在LateUpdate中我们存储我们的位置
        /// </summary>
        protected void LateUpdate()
        {
            mPositionLastFrame = CachedTransform.position;
        }

        /// <summary>
        /// 基于对象的最后位置计算速度
        /// </summary>
        protected virtual void ComputeVelocity()
        {
            if (Time.deltaTime != 0f)
            {
                mVelocity = (mLastPosition - (Vector2)CachedTransform.position) / Time.deltaTime;

                if (Vector2.Distance(mLastDamagePosition, CachedTransform.position) > 0.5f)
                {
                    mLastDamagePosition = CachedTransform.position;
                }

                mLastPosition = CachedTransform.position;
            }
        }

        /// <summary>
        /// 确定要传递给Health Damage方法的伤害方向
        /// </summary>
        protected virtual void DetermineDamageDirection()
        {
            switch (DamageDirectionMode)
            {
                case DamageDirections.BasedOnOwnerPosition:
                    if (Owner == null)
                    {
                        Owner = gameObject;
                    }

                    if (mTwoD)
                    {
                        mDamageDirection = mCollidingHealth.CachedTransform.position - Owner.transform.position;
                    }
                    else
                    {
                        mDamageDirection = mCollidingHealth.CachedTransform.position - Owner.transform.position;
                    }

                    break;
                case DamageDirections.BasedOnVelocity:
                    mDamageDirection = (Vector2)CachedTransform.position - mLastDamagePosition;
                    break;
                case DamageDirections.BasedOnScriptDirection:
                    mDamageDirection = mDamageScriptDirection;
                    break;
            }

            mDamageDirection = mDamageDirection.normalized;
        }

        #endregion

        #region CollisionDetection

        /// <summary>
        /// 当与玩家发生碰撞触发时,我们对玩家造成伤害并击退
        /// </summary>
        /// <param name="c">与对象发生碰撞的物体</param>
        public virtual void OnTriggerStay2D(Collider2D c)
        {
            if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerStay2D)) return;
            Colliding(c.gameObject);
        }

        /// <summary>
        /// 在触发器进入2D时,我们调用碰撞终点
        /// </summary>
        /// <param name="c">碰撞体</param>
        public virtual void OnTriggerEnter2D(Collider2D c)
        {
            if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerEnter2D)) return;
            Colliding(c.gameObject);
        }

        #endregion

        /// <summary>
        /// 发生碰撞时,我们施加适当的伤害
        /// </summary>
        /// <param name="c">碰撞体</param>
        protected virtual void Colliding(GameObject c)
        {
            if (!EvaluateAvailability(c))
            {
                return;
            }
            // cache reset 
            mColliderTopDownController = null;

            // if what we're colliding with is damageable
            if (mColliderHealth != null)
            {
                if (mColliderHealth.CurrentHealth > 0)
                {
                    OnCollideWithDamageable(mColliderHealth);
                }
            }
            else // if what we're colliding with can't be damaged
            {
                // OnCollideWithNonDamageable();
                // HitNonDamageableEvent?.Invoke(collider);
            }

            OnAnyCollision(c);
            HitAnythingEvent?.Invoke(c);
        }

        /// <summary>
        /// 检查这一帧是否应该施加伤害
        /// </summary>
        /// <param name="c">碰撞体</param>
        /// <returns>是否可以施加伤害</returns>
        protected virtual bool EvaluateAvailability(GameObject c)
        {
            // if we're inactive, we do nothing
            if (!isActiveAndEnabled) return false;

            // if the object we're colliding with is part of our ignore list, we do nothing and exit
            if (mIgnoredGameObjects.Contains(c)) return false;

            // if what we're colliding with isn't part of the target layers, we do nothing and exit
            if (!LayerManager.LayerInLayerMask(c.layer, TargetLayerMask)) return false;

            // if we're on our first frame, we don't apply damage
            if (Time.time == 0f) return false;

            return true;
        }

        /// <summary>
        /// 描述与可造成伤害的对象碰撞时会发生什么
        /// </summary>
        /// <param name="health">生命值组件</param>
        protected virtual void OnCollideWithDamageable(Health health)
        {
            mCollidingHealth = health;

            if (health.CanTakeDamageThisFrame())
            {
                // if what we're colliding with is a TopDownController, we apply a knockback force
                mColliderTopDownController = health.gameObject.GetComponent<TopDownController>();
                if (mColliderTopDownController == null)
                {
                    mColliderTopDownController = health.gameObject.GetComponentInParent<TopDownController>();
                }

                HitDamageableEvent?.Invoke(mColliderHealth);

                // we apply the damage to the thing we've collided with
                float randomDamage = Random.Range(MinDamageCaused, Mathf.Max(MaxDamageCaused, MinDamageCaused));

                ApplyKnockback(randomDamage, TypedDamages);

                DetermineDamageDirection();

                if (RepeatDamageOverTime)
                {
                    mColliderHealth.DamageOverTime(randomDamage, gameObject, InvincibilityDuration,
                        InvincibilityDuration, mDamageDirection, TypedDamages, AmountOfRepeats, DurationBetweenRepeats,
                        DamageOverTimeInterruptible, RepeatedDamageType);
                }
                else
                {
                    mColliderHealth.Damage(randomDamage, gameObject, InvincibilityDuration, InvincibilityDuration,
                        mDamageDirection, TypedDamages);
                }
            }

            // we apply self damage
            if (DamageTakenEveryTime + DamageTakenDamageable > 0 && !mColliderHealth.PreventTakeSelfDamage)
            {
                SelfDamage(DamageTakenEveryTime + DamageTakenDamageable);
            }
        }

        #region Knockback

        /// <summary>
        /// 如果需要则施加击退效果
        /// </summary>
        protected virtual void ApplyKnockback(float damage, List<TypedDamage> typedDamages)
        {
            if (ShouldApplyKnockback(damage, typedDamages))
            {
                mKnockbackForce = DamageCausedKnockbackForce * mColliderHealth.KnockbackForceMultiplier;
                mKnockbackForce = mColliderHealth.ComputeKnockbackForce(mKnockbackForce, typedDamages);

                if (mTwoD) // if we're in 2D
                {
                    ApplyKnockback2D();
                }
                else // if we're in 3D
                {
                    ApplyKnockback3D();
                }
            }
        }

        /// <summary>
        /// 判断是否应该施加击退效果
        /// </summary>
        /// <returns>是否应该施加击退</returns>
        protected virtual bool ShouldApplyKnockback(float damage, List<TypedDamage> typedDamages)
        {
            if (mColliderHealth.ImmuneToKnockbackIfZeroDamage)
            {
                if (mColliderHealth.ComputeDamageOutput(damage, typedDamages) == 0)
                {
                    return false;
                }
            }

            return (mColliderTopDownController != null)
                   && (DamageCausedKnockbackForce != Vector2.zero)
                   && !mColliderHealth.Invulnerable
                   && mColliderHealth.CanGetKnockback(typedDamages);
        }

        /// <summary>
        /// 如果我们在2D环境中则施加击退
        /// </summary>
        protected virtual void ApplyKnockback2D()
        {
            switch (DamageCausedKnockbackDirection)
            {
                case KnockbackDirections.BasedOnSpeed:
                    var totalVelocity = mVelocity;
                    mKnockbackForce = Vector3.RotateTowards(mKnockbackForce,
                        totalVelocity.normalized, 10f, 0f);
                    break;
                case KnockbackDirections.BasedOnOwnerPosition:
                    if (Owner == null)
                    {
                        Owner = gameObject;
                    }

                    mRelativePosition = mColliderTopDownController.CachedTransform.position - Owner.transform.position;
                    mKnockbackForce = Vector3.RotateTowards(mKnockbackForce, mRelativePosition.normalized, 10f, 0f);
                    break;
                case KnockbackDirections.BasedOnDirection:
                    var direction = (Vector2)CachedTransform.position - mPositionLastFrame;
                    mKnockbackForce = direction * mKnockbackForce.magnitude;
                    break;
                case KnockbackDirections.BasedOnScriptDirection:
                    mKnockbackForce = mKnockbackScriptDirection * mKnockbackForce.magnitude;
                    break;
            }
        }

        /// <summary>
        /// 如果我们在3D环境中则施加击退
        /// </summary>
        protected virtual void ApplyKnockback3D()
        {
            switch (DamageCausedKnockbackDirection)
            {
                case KnockbackDirections.BasedOnSpeed:
                    var totalVelocity = mVelocity;
                    mKnockbackForce = mKnockbackForce * totalVelocity.magnitude;
                    break;
                case KnockbackDirections.BasedOnOwnerPosition:
                    if (Owner == null)
                    {
                        Owner = gameObject;
                    }

                    mRelativePosition = mColliderTopDownController.CachedTransform.position - Owner.transform.position;
                    mKnockbackForce = Quaternion.LookRotation(mRelativePosition) * mKnockbackForce;
                    break;
                case KnockbackDirections.BasedOnDirection:
                    var direction = (Vector2)CachedTransform.position - mPositionLastFrame;
                    mKnockbackForce = direction * mKnockbackForce.magnitude;
                    break;
                case KnockbackDirections.BasedOnScriptDirection:
                    mKnockbackForce = mKnockbackScriptDirection * mKnockbackForce.magnitude;
                    break;
            }
        }

        #endregion

        /// <summary>
        /// 描述与不可造成伤害的对象碰撞时会发生什么
        /// </summary>
        protected virtual void OnCollideWithNonDamageable()
        {
            float selfDamage = DamageTakenEveryTime + DamageTakenNonDamageable;
            if (selfDamage > 0)
            {
                SelfDamage(selfDamage);
            }
        }

        /// <summary>
        /// 描述与任何物体碰撞时可能发生的事情
        /// </summary>
        protected virtual void OnAnyCollision(GameObject other)
        {
        }

        /// <summary>
        /// 对自身施加伤害
        /// </summary>
        /// <param name="damage">伤害值</param>
        protected virtual void SelfDamage(float damage)
        {
            if (DamageTakenHealth != null)
            {
                mDamageDirection = Vector2.up;
                DamageTakenHealth.Damage(damage, gameObject, 0f, DamageTakenInvincibilityDuration, mDamageDirection);
            }
        }
    }
}