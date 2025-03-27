using System;
using System.Collections.Generic;
using EdgeStudio.Odin;
using UnityEngine;

namespace EdgeStudio.Abilities
{
    public class CharacterAbility : TopDownMonoBehaviour
    {
        [ColorFoldout("权限"), Tooltip("如果为真，则该能力可以正常执行；如果为假，则将被忽略。可以用来逐步解锁能力")]
        public bool AbilityPermitted = true;
        
        [ColorFoldout("权限"), Tooltip("包含所有阻止移动状态的数组。如果角色处于这些状态之一并尝试触发此能力，则将不被允许。用于防止在闲置或游泳时使用此能力。")]
        public CharacterStates.MovementStates[] BlockingMovementStates;
        [ColorFoldout("权限"), Tooltip("包含所有阻止条件状态的数组。如果角色处于这些状态之一并尝试触发此能力，则将不被允许。用于防止在死亡时使用此能力。")]
        public CharacterStates.CharacterConditions[] BlockingConditionStates;
        [ColorFoldout("权限"), Tooltip("包含所有阻止武器状态的数组。如果角色的武器处于这些状态之一而角色仍尝试触发此能力，则将不被允许。用于防止在攻击时使用此能力。")]
        public WeaponStates[] BlockingWeaponStates;

        public virtual bool AbilityAuthorized
        {
            get
            {
                if (Character != null)
                {
                    if (BlockingMovementStates is { Length: > 0 })
                    {
                        for (int i = 0; i < BlockingMovementStates.Length; i++)
                        {
                            if (BlockingMovementStates[i] == (Character.MovementState.CurrentState))
                            {
                                return false;
                            }    
                        }
                    }

                    if (BlockingConditionStates is { Length: > 0 })
                    {
                        for (int i = 0; i < BlockingConditionStates.Length; i++)
                        {
                            if (BlockingConditionStates[i] == (Character.ConditionState.CurrentState))
                            {
                                return false;
                            }    
                        }
                    }
                    
                    if (BlockingWeaponStates is { Length: > 0 })
                    {
                        for (int i = 0; i < BlockingWeaponStates.Length; i++)
                        {
                            foreach (CharacterHandleWeapon handleWeapon in mHandleWeaponList)
                            {
                                if (handleWeapon.CurrentWeapon != null)
                                {
                                    if (BlockingWeaponStates[i] == handleWeapon.CurrentWeapon.WeaponState.CurrentState)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                return AbilityPermitted;
            }
        }
        
        /// 此能力是否已初始化
        public virtual bool AbilityInitialized => mAbilityInitialized;

        [NonSerialized] public Character Character;
        protected TopDownController mController;
        protected GameObject mModel;
        protected Health mHealth;
        protected CharacterMovement mCharacterMovement;
        protected Animator mAnimator;
        protected CharacterStates mState;
        protected bool mAbilityInitialized;
        protected float mVerticalInput;
        protected float mHorizontalInput;
        protected List<CharacterHandleWeapon> mHandleWeaponList;
        protected StateMachine<CharacterStates.MovementStates> mMovement;
        protected StateMachine<CharacterStates.CharacterConditions> mCondition;
        /// <summary>
        /// 在 Awake 时，我们进行能力的预初始化
        /// </summary>
        protected virtual void Awake() => PreInitialization ();

        /// <summary>
        /// 在 Start() 中，我们调用能力的初始化
        /// </summary>
        protected virtual void Start () => Initialization();

        /// <summary>
        /// 可以重写的方法，以在实际初始化之前进行初始化
        /// </summary>
        protected virtual void PreInitialization()
        {
            Character = CachedGameObject.GetComponentInParent<Character>();
            BindAnimator();
        }

        /// <summary>
        /// 获取并存储组件以供进一步使用
        /// </summary>
        protected virtual void Initialization()
        {
            mController = CachedGameObject.GetComponentInParent<TopDownController>();
            mModel = Character.CharacterModel;
            mCharacterMovement = Character?.FindAbility<CharacterMovement>();
            mHealth = Character.CharacterHealth;
            mHandleWeaponList = Character?.FindAbilities<CharacterHandleWeapon>();
            mState = Character.CharacterState;
            mMovement = Character.MovementState;
            mCondition = Character.ConditionState;
            mAbilityInitialized = true;
        }

        /// <summary>
        /// 每当您想强制此能力初始化（再次）时调用此方法
        /// </summary>
        public virtual void ForceInitialization()
        {
            Initialization();
        }

        /// <summary>
        /// 绑定角色的动画器并初始化动画器参数
        /// </summary>
        protected virtual void BindAnimator()
        {
            mAnimator = Character.Animator;
            InitializeAnimatorParameters();
        }
        
        /// <summary>
        /// 内部方法检查输入管理器是否存在
        /// </summary>
        protected virtual void InternalHandleInput()
        {
            if (Character.Team is not (CharacterTeam.Player11 or CharacterTeam.Player12))
            {
                mHorizontalInput = InputManager.Inst.PrimaryMovementInput.x;
                mVerticalInput = InputManager.Inst.PrimaryMovementInput.y;
                HandleInput();
            }

        }

        /// <summary>
        /// 在能力周期的开始时调用，旨在被重写，查找输入并在条件满足时调用方法
        /// </summary>
        protected virtual void HandleInput()
        {

        }

        /// <summary>
        /// 重置此能力的所有输入。可以重写以实现特定于能力的指令
        /// </summary>
        public virtual void ResetInput()
        {
            mHorizontalInput = 0f;
            mVerticalInput = 0f;
        }


        /// <summary>
        /// 您可以在能力中进行的三次传递中的第一次。可以将其视为 EarlyUpdate() 如果它存在
        /// </summary>
        public virtual void EarlyProcessAbility()
        {
            InternalHandleInput();
        }

        /// <summary>
        /// 您可以在能力中进行的三次传递中的第二次。可以将其视为 Update()
        /// </summary>
        public virtual void ProcessAbility()
        {
            
        }

        /// <summary>
        /// 您可以在能力中进行的三次传递中的最后一次。可以将其视为 LateUpdate()
        /// </summary>
        public virtual void LateProcessAbility()
        {
            
        }

        /// <summary>
        /// 重写此方法以将参数发送到角色的动画器。此方法在每个周期中调用，由角色类在 Early、正常和 Late 处理后调用。
        /// </summary>
        public virtual void UpdateAnimator()
        {

        }

        /// <summary>
        /// 更改能力权限的状态
        /// </summary>
        /// <param name="abilityPermitted">如果设置为 <c>true</c> 则允许能力。</param>
        public virtual void PermitAbility(bool abilityPermitted)
        {
            AbilityPermitted = abilityPermitted;
        }

        /// <summary>
        /// 重写此方法以指定角色翻转时此能力应发生的事情
        /// </summary>
        public virtual void Flip()
        {
            
        }

        /// <summary>
        /// 重写此方法以重置此能力的参数。当角色被杀死时，它将自动调用，以便为其重生做准备。
        /// </summary>
        public virtual void ResetAbility()
        {
            
        }
        
        /// <summary>
        /// 播放能力开始的音效
        /// </summary>
        public virtual void AbilityStart()
        {
        }    

        /// <summary>
        /// 播放能力使用的音效
        /// </summary>
        public virtual void AbilityUsed()
        {
        }    

        /// <summary>
        /// 停止能力使用的音效
        /// </summary>
        public virtual void StopAbility()
        {
        }    

        /// <summary>
        /// 重写此方法以描述角色重生时此能力应发生的事情
        /// </summary>
        protected virtual void OnRespawn()
        {
        }

        /// <summary>
        /// 重写此方法以描述角色死亡时此能力应发生的事情
        /// </summary>
        protected virtual void OnDeath()
        {
            StopAbility();
        }

        /// <summary>
        /// 重写此方法以描述角色受到攻击时此能力应发生的事情
        /// </summary>
        protected virtual void OnHit()
        {

        }

        /// <summary>
        /// 在启用时，我们绑定重生委托
        /// </summary>
        protected virtual void OnEnable()
        {
            if (mHealth == null)
            {
                mHealth = CachedGameObject.GetComponentInParent<Character>().CharacterHealth;
            }

            if (mHealth == null)
            {
                mHealth = CachedGameObject.GetComponentInParent<Health>();
            }

            if (mHealth != null)
            {
                mHealth.OnRevive += OnRespawn;
                mHealth.OnDeath += OnDeath;
                mHealth.OnHit += OnHit;
            }
        }

        /// <summary>
        /// 在禁用时，我们解绑重生委托 
        /// </summary>
        protected virtual void OnDisable()
        {
            if (mHealth != null)
            {
                mHealth.OnRevive -= OnRespawn;
                mHealth.OnDeath -= OnDeath;
                mHealth.OnHit -= OnHit;
            }    
        }

        protected virtual void InitializeAnimatorParameters()
        {
        }
    }
}
