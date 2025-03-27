using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EdgeStudio.Config;
using EdgeStudio.GamePlay;
using EdgeStudio.Odin;
using EdgeStudio.Weapons;
using MoreMountains.TopDownEngine;
using Panthea.Asset;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
namespace EdgeStudio.Abilities
{
	/// <summary>
	/// 将此类添加到角色，以便它可以使用武器
	/// 注意，此组件将根据当前武器的动画触发动画
	/// </summary>
	[AddComponentMenu("Edge Studio/Character/Abilities/Character Handle Weapon")]
	[InfoBox("<size=12>此组件将允许您的角色拾取和使用武器。武器的功能由武器类定义。这只是描述持有武器的“手”的行为，而不是武器本身。在这里，您可以为角色设置初始武器，允许武器拾取，并指定武器附加（角色内部的变换，可以是一个空的子游戏对象，或模型的一个子部分）。</size>")]
	public class CharacterHandleWeapon : CharacterAbility
	{
		[Header("绑定"),ColorFoldout("武器")]
		[Tooltip("武器将附加到的位置。如果留空，将是this.transform。")]
		public Transform WeaponAttachment;
		[Tooltip("从中生成投射物的位置（可以安全地留空）"),ColorFoldout("武器")]
		public Transform ProjectileSpawn;

		[Tooltip("如果为true，角色将持续射击其武器"),ColorFoldout("武器")]
		public bool ForceAlwaysShoot;

		[Header("调试"), ColorFoldout("武器"), ReadOnly, Tooltip("角色当前装备的武器")]
		public Weapon CurrentWeapon;

		private CancellationTokenSource mInstantiateWeaponCancelToken;
		/// 使用武器时更新的动画器
		public virtual Animation CharacterAnimator { get; set; }
		/// 武器的武器瞄准组件（如果有）
		public virtual WeaponAim WeaponAimComponent => mWeaponAim;

		private readonly Subject<Unit> mOnWeaponChange = new();
		/// 您可以挂钩的委托，以便在武器更改时收到通知
		public Observable<Unit> OnWeaponChange => mOnWeaponChange;

		protected WeaponAim mWeaponAim;
		protected ProjectileWeapon mProjectileWeapon;
		
		/// <summary>
		/// 设置武器附加
		/// </summary>
		protected override void PreInitialization()
		{
			base.PreInitialization();
			// 如果WeaponAttachment尚未设置，则填充
			if (WeaponAttachment == null)
			{
				WeaponAttachment = transform;
			}
		}

		// 初始化
		protected override void Initialization()
		{
			base.Initialization();
			Setup();
			SetupEvent();
			ChangeWeapon(ItemProperty.Read("AK74U"));
		}

		private void SetupEvent()
		{
		}

		
		/// <summary>
		/// 获取各种组件并初始化
		/// </summary>
		public virtual void Setup()
		{
			// 如果WeaponAttachment尚未设置，则填充
			if (WeaponAttachment == null)
			{
				WeaponAttachment = transform;
			}
		}

		/// <summary>
		/// 每帧检查是否需要更新弹药显示
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleCharacterState();
			HandleFeedbacks();
		}

		/// <summary>
		/// 检查角色状态，如果不在正常状态则停止射击
		/// </summary>
		protected virtual void HandleCharacterState()
		{
			if (mCondition.CurrentState != CharacterStates.CharacterConditions.Normal)
			{
				ShootStop();
			}
		}

		/// <summary>
		/// 如果需要，触发武器使用反馈
		/// </summary>
		protected virtual void HandleFeedbacks()
		{
			if (CurrentWeapon != null)
			{
				if (CurrentWeapon.WeaponState.CurrentState == WeaponStates.WeaponUse)
				{
				}
			}
		}

		/// <summary>
		/// 获取输入并根据按下的内容触发方法
		/// </summary>
		protected override void HandleInput()
		{
			if (!AbilityAuthorized || mCondition.CurrentState != CharacterStates.CharacterConditions.Normal || CurrentWeapon == null) return;

			if (ForceAlwaysShoot)
			{
				ShootStart();
			}
		}
		
		/// <summary>
		/// 使角色开始射击
		/// </summary>
		public virtual void ShootStart()
		{
			// 如果在权限中启用了射击操作，我们继续，如果没有，我们什么也不做。如果玩家已死亡，我们也不做。
			if (!AbilityAuthorized || CurrentWeapon == null || mCondition.CurrentState != CharacterStates.CharacterConditions.Normal)
			{
				return;
			}

			CurrentWeapon.WeaponInputStart();
		}

		/// <summary>
		/// 使角色停止射击
		/// </summary>
		public virtual void ShootStop()
		{
			// 如果在权限中启用了射击操作，我们继续，如果没有，我们什么也不做
			if (!AbilityAuthorized || CurrentWeapon == null)
			{
				return;
			}

			switch (CurrentWeapon.WeaponState.CurrentState)
			{
				case WeaponStates.WeaponIdle:
				case WeaponStates.WeaponReload or WeaponStates.WeaponReloadStart or WeaponStates.WeaponReloadStop:
				case WeaponStates.WeaponDelayBeforeUse when !CurrentWeapon.DelayBeforeUseReleaseInterruption:
				case WeaponStates.WeaponDelayBetweenUses when !CurrentWeapon.TimeBetweenUsesReleaseInterruption:
				case WeaponStates.WeaponUse:
					return;
				default:
					ForceStop();
					break;
			}
		}

		/// <summary>
		/// 强制武器停止 
		/// </summary>
		public virtual void ForceStop()
		{
			if (CurrentWeapon != null)
			{
				CurrentWeapon.TurnWeaponOff();    
			}
		}

		/// <summary>
		/// 将角色当前武器更改为作为参数传递的新武器
		/// </summary>
		public virtual void ChangeWeapon(ItemProperty data, bool combo = false)
		{
			// 如果角色已经有武器，我们让它停止射击
			if (CurrentWeapon != null)
			{
				CurrentWeapon.TurnWeaponOff();
				if (!combo)
				{
					ShootStop();
					Destroy(CurrentWeapon.gameObject);
				}
			}

			InstantiateWeapon(data, combo).Forget();
			mOnWeaponChange.OnNext(Unit.Default);
		}

		/// <summary>
		/// 实例化指定的武器
		/// </summary>
		/// <param name="data"></param>
		/// <param name="combo"></param>
		protected virtual async UniTask InstantiateWeapon(ItemProperty data, bool combo = false)
		{
			if (!combo)
			{
				mInstantiateWeaponCancelToken?.Cancel();
				mInstantiateWeaponCancelToken = new CancellationTokenSource();
				UniTask<(bool IsCanceled, AssetObject Result)> task;
				if (data == null)
				{
					task = AssetsKit.Inst.Load<AssetObject>("prefabs/weapons/emptyweapon").AttachExternalCancellation(mInstantiateWeaponCancelToken.Token)
						.Timeout(TimeSpan.FromSeconds(5)).SuppressCancellationThrow();
				}
				else
				{
					task = AssetsKit.Inst.Load<AssetObject>($"prefabs/weapons/{data.WeaponArgs.UsePrefab.ToLower()}").AttachExternalCancellation(mInstantiateWeaponCancelToken.Token)
						.Timeout(TimeSpan.FromSeconds(5)).SuppressCancellationThrow();
				}
				var result = await task;
				if (result.IsCanceled) return;

				CurrentWeapon = Instantiate(result.Result.GetGameObject(),WeaponAttachment.transform).GetComponent<Weapon>();
			}
			CurrentWeapon.SetOwner(Character, this);
			mWeaponAim = CurrentWeapon.gameObject.GetComponent<WeaponAim>();
			CurrentWeapon.FlipWeapon();
			if (data != null)
			{
				RefreshWeaponAttribute(data);
			}
			else
			{
				CurrentWeapon.WeaponID = null;
			}

			// 处理（可选）逆向运动学（IK） 
			HandleWeaponIK();

			// 关闭枪的发射器。
			CurrentWeapon.Initialization();
		}

		private void RefreshWeaponAttribute(ItemProperty item)
		{
			CurrentWeapon.BulletSpeed = item.WeaponArgs.BulletSpeed;
			if (CurrentWeapon is ProjectileWeapon projectileWeapon)
			{
				projectileWeapon.ProjectileSpawnOffset = item.WeaponArgs.ProjectileSpawnOffset;
			}
			
			CurrentWeapon.TimeBetweenUses = 1 / item.WeaponArgs.RateOfFire / 60;
			CurrentWeapon.MagazineSize = item.WeaponArgs.Magazine;
			CurrentWeapon.Display.GetComponent<AsyncSpriteRenderer>().SpriteName.Value = item.Texture; 
			CurrentWeapon.BurstLength = item.WeaponArgs.BurstCount;
			CurrentWeapon.BurstTimeBetweenShots = item.WeaponArgs.BurstBetweenShoot;
			CurrentWeapon.ReloadTime = item.WeaponArgs.ReloadTime;
			CurrentWeapon.AmmoConsumedPerShot = item.WeaponArgs.AmmoConsumedPerShot;
			CurrentWeapon.MovementMultiplier = item.WeaponArgs.ShootMovementMultiplier;
			CurrentWeapon.WeaponAttachmentOffset = item.WeaponArgs.WeaponAttachmentOffset;
			CurrentWeapon.WeaponSwitchSpeed = Mathf.Clamp(1 - item.WeaponArgs.Ergonomics / 100, 0.3f, 2); 
			CurrentWeapon.ShootSound = UniTask.Lazy(() => AssetsKit.Inst.Load<AudioClip>(item.WeaponArgs.ShootSound));
			CurrentWeapon.MagazineSound = UniTask.Lazy(() => AssetsKit.Inst.Load<AudioClip>(item.WeaponArgs.MagazineSound));
		}

		/// <summary>
		/// 如果需要，设置IK手柄
		/// </summary>
		protected virtual void HandleWeaponIK()
		{
			mProjectileWeapon = CurrentWeapon.gameObject.GetComponent<ProjectileWeapon>();
			if (mProjectileWeapon != null)
			{
				mProjectileWeapon.SetProjectileSpawnTransform(ProjectileSpawn);
			}
		}

		/// <summary>
		/// 如果需要，翻转当前武器
		/// </summary>
		public override void Flip()
		{
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			ShootStop();
			if (CurrentWeapon != null)
			{
				ChangeWeapon(null);
			}
		}

		protected override void OnRespawn()
		{
			base.OnRespawn();
			Setup();
		}
	}
}
