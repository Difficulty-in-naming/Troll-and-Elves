using EdgeStudio.Odin;
using EdgeStudio.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EdgeStudio.Abilities
{
    /// <summary>
    ///     将此组件添加到角色上，它就能够奔跑
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Run")]
    [InfoBox("此组件允许你的角色在按下奔跑按钮时改变速度（在此定义）。")]
    public class CharacterRun : CharacterAbility
    {
        [ColorFoldout("速度")] [Tooltip("角色奔跑时的速度")]
        public float RunSpeed = 16f;
        protected int mRunningAnimationParameter;
        protected bool mRunningStarted;
        /// <summary>
        ///     在每个周期开始时，我们检查是否按下或释放了奔跑按钮
        /// </summary>
        protected override void HandleInput()
        {
            if (InputManager.Inst.Run.action.IsPressed()) RunStart();

            if (mRunningStarted)
            {
                if (!InputManager.Inst.Run.action.IsPressed())
                {
                    RunStop();
                }
            }
        }

        /// <summary>
        ///     每帧我们都确保不应该退出奔跑状态
        /// </summary>
        public override void ProcessAbility()
        {
            base.ProcessAbility();
            HandleRunningExit();
        }

        /// <summary>
        ///     检查我们是否应该退出奔跑状态
        /// </summary>
        protected virtual void HandleRunningExit()
        {
            if (mCondition.CurrentState != CharacterStates.CharacterConditions.Normal) StopAbility();

            // 如果我们移动得不够快，我们回到空闲状态
            if (Mathf.Abs(mController.CurrentMovement.magnitude) < RunSpeed / 10 && mMovement.CurrentState == CharacterStates.MovementStates.Running)
            {
                mMovement.ChangeState(CharacterStates.MovementStates.Idle);
                StopFeedbacks();
                StopSfx();
            }
        }

        /// <summary>
        ///     使角色开始奔跑。
        /// </summary>
        public virtual void RunStart()
        {
            if (!AbilityAuthorized || mCondition.CurrentState != CharacterStates.CharacterConditions.Normal || mMovement.CurrentState != CharacterStates.MovementStates.Walking)
                return;

            // 如果玩家按下奔跑按钮，并且我们在地面上，没有蹲下，可以自由移动，
            // 那么我们在控制器的参数中改变移动速度。
            if (mCharacterMovement != null) mCharacterMovement.MovementSpeed = RunSpeed;

            // 如果我们还没有在奔跑，我们触发我们的声音
            if (mMovement.CurrentState != CharacterStates.MovementStates.Running)
            {
                AbilityStart();
                AbilityUsed();
                mRunningStarted = true;
            }

            mMovement.ChangeState(CharacterStates.MovementStates.Running);
        }

        /// <summary>
        ///     使角色停止奔跑。
        /// </summary>
        public virtual void RunStop()
        {
            if (mRunningStarted)
            {
                // 如果释放了奔跑按钮，我们恢复到行走速度。
                if (mCharacterMovement != null)
                {
                    mCharacterMovement.ResetSpeed();
                    mMovement.ChangeState(CharacterStates.MovementStates.Idle);
                }

                StopFeedbacks();
                StopSfx();
                mRunningStarted = false;
            }
        }

        /// <summary>
        ///     停止所有奔跑反馈
        /// </summary>
        protected virtual void StopFeedbacks()
        {
        }

        /// <summary>
        ///     停止所有奔跑声音
        /// </summary>
        protected virtual void StopSfx()
        {
        }

        protected override void InitializeAnimatorParameters()
        {
            base.InitializeAnimatorParameters();
            mRunningAnimationParameter = Animator.StringToHash("Running");
        }

        public override void UpdateAnimator()
        {
            base.UpdateAnimator();
            mAnimator.SetBool(mRunningAnimationParameter, mMovement.CurrentState == CharacterStates.MovementStates.Running);
        }
    }
}