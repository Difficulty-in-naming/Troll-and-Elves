using EdgeStudio.Odin;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EdgeStudio.Abilities
{
	/// <summary>
	/// 将此能力添加到角色后，角色将能够在2D环境中进行冲刺，在特定时间内移动特定距离
	/// </summary>
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Dash")]
	public class CharacterDash : CharacterAbility
	{
		[ColorFoldout("冲刺"), Tooltip("应用冲刺的模式")]
		public DashModes DashMode = DashModes.MainMovement;

		[ColorFoldout("冲刺")]
		public DashSpaces DashSpace = DashSpaces.World;

		[ColorFoldout("冲刺"), Tooltip("冲刺方向")] 
		public Vector2 DashDirection = Vector2.right;

		[ColorFoldout("冲刺"), Tooltip("冲刺应持续的距离")]
		public float DashDistance = 6f;

		[ColorFoldout("冲刺"), Tooltip("冲刺的持续时间，以秒为单位")]
		public float DashDuration = 0.2f;

		[ColorFoldout("冲刺"), Tooltip("应用于冲刺加速度的动画曲线")]
		public AnimationCurve DashCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

		[Header("Cooldown")]
		[ColorFoldout("冲刺"), Tooltip("此能力的冷却时间")]
		public Cooldown Cooldown;

		[ColorFoldout("冲刺"), Header("Damage")]
		[Tooltip("如果为true，则此角色在冲刺过程中不会受到任何伤害")]
		public bool InvincibleWhileDashing = false;

		protected bool mDashing;
		protected float mDashTimer;
		protected Vector2 mDashOrigin;
		protected Vector2 mDashDestination;
		protected Vector2 mNewPosition;
		protected Vector2 mOldPosition;
		protected Vector2 mInputPosition;
		protected Camera mMainCamera;
		

		/// <summary>
		/// 在初始化时，我们停止粒子效果，并初始化冲刺条
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			Cooldown.Initialization();

			mMainCamera = Camera.main;
		}

		/// <summary>
		/// 监视冲刺输入
		/// </summary>
		protected override void HandleInput()
		{
			base.HandleInput();
			if (!AbilityAuthorized || !Cooldown.Ready() || mCondition.CurrentState != CharacterStates.CharacterConditions.Normal)
			{
				return;
			}

			if (InputManager.Inst.Dash.action.WasPressedThisFrame())
			{
				DashStart();
			}
		}

		/// <summary>
		/// 启动冲刺
		/// </summary>
		public virtual void DashStart()
		{
			if (!Cooldown.Ready())
			{
				return;
			}

			Cooldown.Start();
			mMovement.ChangeState(CharacterStates.MovementStates.Dashing);
			mDashing = true;
			mDashTimer = 0f;
			mDashOrigin = CachedTransform.position;

			if (InvincibleWhileDashing)
			{
				mHealth.DamageDisabled();
			}

			HandleDashMode();
		}

		/// <summary>
		/// 处理冲刺模式
		/// </summary>
		protected virtual void HandleDashMode()
		{
			switch (DashMode)
			{
				case DashModes.MainMovement:
					mDashDestination = (Vector2)CachedTransform.position + mController.CurrentDirection.normalized * DashDistance;
					break;

				case DashModes.Fixed:
					mDashDestination = (Vector2)CachedTransform.position + DashDirection.normalized * DashDistance;
					break;

				case DashModes.SecondaryMovement:
					mDashDestination = (Vector2)CachedTransform.position + InputManager.Inst.SecondaryMovementInput.normalized * DashDistance;
					break;

				case DashModes.MousePosition:
					mInputPosition = mMainCamera.ScreenToWorldPoint(InputManager.Inst.MousePosition);
					mDashDestination = (Vector2)CachedTransform.position + (mInputPosition - (Vector2)CachedTransform.position).normalized * DashDistance;
					break;

				case DashModes.Script:
					mDashDestination = (Vector2)CachedTransform.position + DashDirection.normalized * DashDistance;
					break;
			}
		}

		/// <summary>
		/// 停止冲刺
		/// </summary>
		public virtual void DashStop()
		{
			if (InvincibleWhileDashing)
			{
				mHealth.DamageEnabled();
			}

			mMovement.ChangeState(CharacterStates.MovementStates.Idle);
			mDashing = false;
		}

		/// <summary>
		/// 在更新时，根据需要移动角色
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			Cooldown.Update();
			UpdateDashBar();

			if (mDashing)
			{
				if (mDashTimer < DashDuration)
				{
					if (DashSpace == DashSpaces.World)
					{
						mNewPosition = Vector2.Lerp(mDashOrigin, mDashDestination, DashCurve.Evaluate(mDashTimer / DashDuration));
						mDashTimer += Time.deltaTime;
						mController.MovePosition(mNewPosition);
					}
					else
					{
						mOldPosition = mDashTimer == 0 ? mDashOrigin : mNewPosition;
						mNewPosition = Vector2.Lerp(mDashOrigin, mDashDestination, DashCurve.Evaluate(mDashTimer / DashDuration));
						mDashTimer += Time.deltaTime;
						mController.MovePosition((Vector2)CachedTransform.position + mNewPosition - mOldPosition);
					}
				}
				else
				{
					DashStop();
				}
			}
		}

		/// <summary>
		/// 更新GUI冲刺条
		/// </summary>
		protected virtual void UpdateDashBar()
		{
		}
	}
}

