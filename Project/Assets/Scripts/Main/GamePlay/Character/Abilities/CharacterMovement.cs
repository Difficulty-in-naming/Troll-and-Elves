using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EdgeStudio.Odin;
using UnityEngine;

namespace EdgeStudio.Abilities
{
    /// <summary> 将此能力添加到角色以处理地面移动（行走，可能还有奔跑、爬行等） </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Movement")]
    public class CharacterMovement : CharacterAbility
    {
        public virtual float MovementSpeed { get; set; }
        public virtual bool MovementForbidden { get; set; }

        [Header("设置")] [ColorFoldout("移动")] [Tooltip("当前是否授权移动输入")]
        public bool InputAuthorized = true;

        [Header("速度")] [ColorFoldout("移动")] [Tooltip("角色行走时的速度")]
        public float WalkSpeed = 6f;

        [Tooltip("角色不再被视为闲置的速度阈值")] [ColorFoldout("移动")]
        public float IdleThreshold = 0.05f;

        public virtual float MovementSpeedMaxMultiplier { get; set; } = float.MaxValue;
        private float mMovementSpeedMultiplier;

        public float MovementSpeedMultiplier
        {
            get => Mathf.Min(mMovementSpeedMultiplier, MovementSpeedMaxMultiplier);
            set => mMovementSpeedMultiplier = value;
        }

        public readonly Stack<float> ContextSpeedStack = new();
        public virtual float ContextSpeedMultiplier => ContextSpeedStack.Count > 0 ? ContextSpeedStack.Peek() : 1;

        [Header("行走反馈")] [ColorFoldout("移动")] [Tooltip("行走时触发的粒子效果")]
        public ParticleSystem[] WalkParticles;

        protected float mMovementSpeed;
        protected float mHorizontalMovement;
        protected float mVerticalMovement;
        protected Vector3 mMovementVector;
        protected Vector2 mCurrentInput = Vector2.zero;
        protected bool mWalkParticlesPlaying;
        protected int mWalkingAnimationParameter;
        protected int mIdleAnimationParameter;

        protected override void Initialization()
        {
            base.Initialization();
            ResetAbility();
        }

        /// <summary>
        ///     重置角色移动状态和速度
        /// </summary>
        public override void ResetAbility()
        {
            base.ResetAbility();
            MovementSpeed = WalkSpeed;
            ContextSpeedStack?.Clear();
            mMovement?.ChangeState(CharacterStates.MovementStates.Idle);
            MovementSpeedMultiplier = 1f;
            MovementForbidden = false;

            foreach (var system in WalkParticles)
                if (system != null)
                    system.Stop();
        }
        
        public override void ProcessAbility()
        {
            base.ProcessAbility();

            HandleFrozen();

            if (!AbilityAuthorized
                || (mCondition.CurrentState != CharacterStates.CharacterConditions.Normal &&
                    mCondition.CurrentState != CharacterStates.CharacterConditions.ControlledMovement))
            {
                return;
            }

            HandleMovement();
            Feedbacks();
        }
        
        protected override void HandleInput()
        {
            if (InputAuthorized)
            {
                mHorizontalMovement = mHorizontalInput;
                mVerticalMovement = mVerticalInput;
            }
            else
            {
                mHorizontalMovement = 0f;
                mVerticalMovement = 0f;
            }
        }

        /// <summary>设置水平移动值。</summary>
        public virtual void SetMovement(Vector2 value)
        {
            mHorizontalMovement = value.x;
            mVerticalMovement = value.y;
        }

        /// <summary>设置移动的水平部分</summary>
        public virtual void SetHorizontalMovement(float value)
        {
            mHorizontalMovement = value;
        }

        /// <summary>设置移动的垂直部分</summary>
        public virtual void SetVerticalMovement(float value)
        {
            mVerticalMovement = value;
        }

        /// <summary>应用指定持续时间的移动乘数</summary>
        public virtual void ApplyMovementMultiplier(float movementMultiplier, float duration) => ApplyMovementMultiplierAsync(movementMultiplier, duration).Forget();

        /// <summary>用于在特定持续时间内应用移动乘数的协程</summary>
        protected virtual async UniTask ApplyMovementMultiplierAsync(float movementMultiplier, float duration)
        {
            if (mCharacterMovement == null) return;

            SetContextSpeedMultiplier(movementMultiplier);
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: destroyCancellationToken, cancelImmediately: true).SuppressCancellationThrow();
            ResetContextSpeedMultiplier();
        }

        /// <summary>堆叠新的上下文速度乘数</summary>
        public virtual void SetContextSpeedMultiplier(float newMovementSpeedMultiplier) => ContextSpeedStack.Push(newMovementSpeedMultiplier);

        /// <summary>将上下文速度乘数恢复到其先前值</summary>
        public virtual void ResetContextSpeedMultiplier()
        {
            if (ContextSpeedStack.Count <= 0) return;
            ContextSpeedStack.Pop();
        }

        /// <summary>在 Update() 中调用，处理水平移动</summary>
        protected virtual void HandleMovement()
        {
            // 如果移动被阻止，或者角色死亡/冻结/无法移动，我们退出并什么也不做
            if (!AbilityAuthorized || mCondition.CurrentState != CharacterStates.CharacterConditions.Normal)
                return;

            if (MovementForbidden)
            {
                mHorizontalMovement = 0f;
                mVerticalMovement = 0f;
            }

            if ((mController.CurrentMovement.magnitude > IdleThreshold || (Character.Pathfinder != null && Character.Pathfinder.velocity != Vector3.zero && Character.Pathfinder.reachedDestination == false)) &&
                mMovement.CurrentState == CharacterStates.MovementStates.Idle)
            {
                mMovement.ChangeState(CharacterStates.MovementStates.Walking);
            }

            // 如果我们在行走但不再移动，我们回到闲置状态
            if (mMovement.CurrentState == CharacterStates.MovementStates.Walking && mController.CurrentMovement.magnitude <= IdleThreshold &&
                (Character.Pathfinder == null || Character.Pathfinder.velocity == Vector3.zero || Character.Pathfinder.reachedDestination))
            {
                mMovement.ChangeState(CharacterStates.MovementStates.Idle);
            }

            SetMovement();
        }

        /// <summary>描述角色处于冻结状态时发生的事情</summary>
        protected virtual void HandleFrozen()
        {
            if (!AbilityAuthorized) return;
            if (mCondition.CurrentState == CharacterStates.CharacterConditions.Frozen)
            {
                mHorizontalMovement = 0f;
                mVerticalMovement = 0f;
                SetMovement();
            }
        }

        /// <summary>移动控制器</summary>
        protected virtual void SetMovement()
        {
            mCurrentInput = Vector2.zero;

            mCurrentInput.x = mHorizontalMovement;
            mCurrentInput.y = mVerticalMovement;

            mMovementVector = mCurrentInput.normalized;

            mMovementSpeed = MovementSpeed * MovementSpeedMultiplier * ContextSpeedMultiplier;

            mMovementVector *= mMovementSpeed;

            if (mMovementVector.magnitude > MovementSpeed * ContextSpeedMultiplier * MovementSpeedMultiplier)
                mMovementVector = Vector3.ClampMagnitude(mMovementVector, MovementSpeed);

            if (mCurrentInput.magnitude <= IdleThreshold && mController.CurrentMovement.magnitude < IdleThreshold) mMovementVector = Vector3.zero;

            mController.SetMovement(mMovementVector);
        }

        /// <summary>行走时播放粒子，着陆时播放粒子和声音</summary>
        protected virtual void Feedbacks()
        {
            if (mController.CurrentMovement.magnitude > IdleThreshold || (Character.Pathfinder != null && Character.Pathfinder.velocity != Vector3.zero && Character.Pathfinder.reachedDestination == false))
                foreach (var system in WalkParticles)
                {
                    if (!mWalkParticlesPlaying && system != null) system.Play();
                    mWalkParticlesPlaying = true;
                }
            else
                foreach (var system in WalkParticles)
                    if (mWalkParticlesPlaying && system != null)
                    {
                        system.Stop();
                        mWalkParticlesPlaying = false;
                    }
        }

        /// <summary>重置此角色的速度</summary>
        public virtual void ResetSpeed() => MovementSpeed = WalkSpeed;

        /// <summary>在重生时重置速度</summary>
        protected override void OnRespawn()
        {
            ResetSpeed();
            MovementForbidden = false;
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            DisableWalkParticles();
        }

        /// <summary>禁用所有可能正在播放的行走粒子系统</summary>
        protected virtual void DisableWalkParticles()
        {
            if (WalkParticles.Length > 0)
                foreach (var walkParticle in WalkParticles)
                    if (walkParticle != null)
                        walkParticle.Stop();
        }

        /// <summary>在禁用时确保关闭任何可能仍在播放的内容 </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            DisableWalkParticles();
            StopAbility();
        }

        protected override void InitializeAnimatorParameters()
        {
            mWalkingAnimationParameter = Animator.StringToHash("Walking");
            mIdleAnimationParameter = Animator.StringToHash("Idle");
        }

        public override void UpdateAnimator()
        {
            mAnimator.SetBool(mWalkingAnimationParameter, mMovement.CurrentState == CharacterStates.MovementStates.Walking);
            mAnimator.SetBool(mIdleAnimationParameter, mMovement.CurrentState == CharacterStates.MovementStates.Idle);
        }
    }
}