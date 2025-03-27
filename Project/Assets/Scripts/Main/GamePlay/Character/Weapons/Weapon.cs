using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using EdgeStudio;
using EdgeStudio.Abilities;
using EdgeStudio.Odin;
using EdgeStudio.Weapons;
using LitMotion;
using LitMotion.Extensions;
using Panthea.Common;
using R3;
using Sirenix.OdinInspector;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// 这个基类，意在被扩展（见ProjectileWeapon.cs作为例子），处理开火频率（实际上是使用频率），和弹药重装
	/// </summary>
	[SelectionBase]
	public class Weapon : BetterMonoBehaviour
	{
		[ReadOnly, Tooltip("武器当前是否处于激活状态"), ColorFoldout("ID")]  
		public bool WeaponCurrentlyActive = true;

		[ColorFoldout("Use"), Tooltip("如果为true，此武器将能够读取输入（通常通过CharacterHandleWeapon能力），否则玩家输入将被禁用")] 
		public bool InputAuthorized = true;

		[ColorFoldout("Use"),Tooltip("使用前的延迟，将应用于每次射击")] public float DelayBeforeUse;

		[ColorFoldout("Use"),Tooltip("使用前的延迟是否可以通过释放射击按钮来中断（如果为true，释放按钮将取消延迟射击）")]
		public bool DelayBeforeUseReleaseInterruption = true;

		[ColorFoldout("Use"),Tooltip("两次射击之间的时间（以秒为单位）")] public float TimeBetweenUses = 1f;

		[ColorFoldout("Use"),Tooltip("使用间隔是否可以通过释放射击按钮来中断（如果为true，释放按钮将取消使用间隔）")]
		public bool TimeBetweenUsesReleaseInterruption = true;

		[ColorFoldout("Use"), Header("Burst Mode"), Tooltip("如果为true，武器将为每个射击请求重复激活")] 
		public bool UseBurstMode;

		[ColorFoldout("Use"), Tooltip("连发序列中的'射击'数量")] public int BurstLength = 3;
		[ColorFoldout("Use"), Tooltip("连发序列中射击之间的时间（以秒为单位）")] public float BurstTimeBetweenShots = 0.1f;
		[ColorFoldout("Use"), Tooltip("伤害")] public float Damage = 0;
		[ColorFoldout("Use"), Tooltip("伤害")] public float BulletSpeed = 0;
		[ColorFoldout("Use"), Tooltip("武器切换速度")] public float WeaponSwitchSpeed = 0;


		[ColorFoldout("Magazine"), Tooltip("武器是否基于弹匣。如果不是，它将从全局弹药池中获取弹药")] 
		public bool MagazineBased;

		[ColorFoldout("Magazine"), Tooltip("弹匣大小")] public int MagazineSize = 30;

		[ColorFoldout("Magazine"), Tooltip("如果为true，当需要装弹时按下开火按钮将装弹武器。否则你需要按装弹按钮")]
		public bool AutoReload;

		[ColorFoldout("Magazine"), Tooltip("如果为true，装弹将在最后一颗子弹射出后自动发生，无需输入")]
		public bool NoInputReload;

		[ColorFoldout("Magazine"), Tooltip("武器装弹所需的时间")] public float ReloadTime = 2f;
		[ColorFoldout("Magazine"), Tooltip("每次武器开火消耗的弹药量")] public int AmmoConsumedPerShot = 1;
		[ColorFoldout("Magazine"), Tooltip("如果设置为true，当没有弹药时武器将自动销毁")] public bool AutoDestroyWhenEmpty;
		[ColorFoldout("Magazine"), Tooltip("弹药耗尽时武器销毁前的延迟（以秒为单位）")] public float AutoDestroyWhenEmptyDelay = 1f;

		[ColorFoldout("Magazine"), Tooltip("如果为true，当使用WeaponAmmo时，武器在弹药为空时不会尝试装弹")]
		public bool PreventReloadIfAmmoEmpty;

		[ColorFoldout("Magazine"), ReadOnly, Tooltip("当前装入武器的弹药量")]  public int CurrentAmmoLoaded;

		[ColorFoldout("Position"), Tooltip("一旦附加到WeaponAttachment变换的中心，将应用于武器的偏移量")] 
		public Vector2 WeaponAttachmentOffset = Vector2.zero;

		[ColorFoldout("Position"), Tooltip("当角色翻转时，该武器是否应该翻转？")] public bool FlipWeaponOnCharacterFlip = true;

		[ColorFoldout("Position"), Tooltip("一个变换，用作武器使用的生成点（如果为null，只考虑偏移量，否则考虑不带偏移量的变换）")]
		public Transform WeaponUseTransform;

		[ColorFoldout("Position"), Tooltip("如果为true，武器将翻转以匹配角色的方向")] public bool WeaponShouldFlip = true;

		[ColorFoldout("Settings")] public Transform Display;
		
		[ColorFoldout("Movement"), Tooltip("如果为true，当武器激活时将对移动应用乘数")] 
		public bool ModifyMovementWhileAttacking;

		[ColorFoldout("Movement"), Tooltip("攻击时应用于移动的乘数")] public float MovementMultiplier;

		[ColorFoldout("Movement"), Tooltip("如果为true，当武器使用时将阻止所有移动（甚至翻转）")]
		public bool PreventAllMovementWhileInUse;

		[ColorFoldout("Movement"), Tooltip("如果为true，当武器使用时将阻止所有瞄准")] public bool PreventAllAimWhileInUse;

		[ColorFoldout("Recoil"), Tooltip("射击时推动角色后退的力 - 正值将推动角色后退，负值将向前推进它，将后座力转化为推进力")] 
		public float RecoilForce;

		[ColorFoldout("Settings"), Tooltip("如果为true，武器将在启动时自行初始化，否则它必须手动初始化，通常由CharacterHandleWeapon类完成")] 
		public bool InitializeOnStart;

		[ColorFoldout("Settings"), Tooltip("此武器是否可以被中断")] public bool Interruptable;
		[ColorFoldout("Settings"), ReadOnly, Tooltip("如果为true，武器当前已翻转")] public bool Flipped;
		[ColorFoldout("音频")] public AudioSource AudioSource;
		[ColorFoldout("音频")] public AsyncLazy<AudioClip> ShootSound;
		[ColorFoldout("音频")] public AsyncLazy<AudioClip> MagazineSound;
		
		[ShowInInspector,ColorFoldout("Debug"),ReadOnly]
		public virtual string WeaponID { get; set; }
		public virtual Character Owner { get; protected set; }
		public virtual CharacterHandleWeapon CharacterHandleWeapon { get; set; }
		
		public StateMachine<WeaponStates> WeaponState;
		
		protected WeaponAim mWeaponAim;
		protected float mMovementMultiplierStorage = 1f;

		public float MovementMultiplierStorage
		{
			get => mMovementMultiplierStorage;
			set => mMovementMultiplierStorage = value;
		}

		public bool IsComboWeapon { get; set; }
		public bool IsAutoComboWeapon { get; set; }

		protected Animation mOwnerAnimator;
		protected WeaponPreventShooting mWeaponPreventShooting;
		protected float mDelayBeforeUseCounter;
		protected float mDelayBetweenUsesCounter;
		protected float mReloadingCounter;
		protected bool mTriggerReleased;
		protected bool mReloading;
		protected TopDownController mController;
		protected CharacterMovement mCharacterMovement;
		protected Vector2 mWeaponOffset;
		protected Vector2 mWeaponAttachmentOffset;
		protected float mLastShootRequestAt = -float.MaxValue;
		protected float mLastTurnWeaponOnAt = -float.MaxValue;
		protected bool mMovementSpeedMultiplierSet;
		/// <summary>
		/// 在开始时初始化我们的武器
		/// </summary>
		protected virtual void Start()
		{
			if (InitializeOnStart)
			{
				Initialization();
			}
		}

		/// <summary>
		/// 初始化此武器。
		/// </summary>
		public virtual void Initialization()
		{
			mReloading = false;
			mTriggerReleased = false;
			mDelayBeforeUseCounter = 0f;
			mDelayBetweenUsesCounter = 0f;
			mReloadingCounter = 0f;
			Flipped = false;
			mWeaponPreventShooting = this.gameObject.GetComponent<WeaponPreventShooting>();
			WeaponState = new StateMachine<WeaponStates>(gameObject, true);
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
			mWeaponAim = GetComponent<WeaponAim>();
		}
		
		/// <summary>
		/// 设置武器的所有者
		/// </summary>
		/// <param name="newOwner">新所有者。</param>
		/// <param name="handleWeapon"></param>
		public virtual void SetOwner(Character newOwner, CharacterHandleWeapon handleWeapon)
		{
			Owner = newOwner;
			if (Owner != null)
			{
				CharacterHandleWeapon = handleWeapon;
				mCharacterMovement = Owner.GetComponent<Character>()?.FindAbility<CharacterMovement>();
				mController = Owner.GetComponent<TopDownController>();
				if (CharacterHandleWeapon.CharacterAnimator != null)
				{
					mOwnerAnimator = CharacterHandleWeapon.CharacterAnimator;
				}
			}
		}

		/// <summary>
		/// 由输入调用，开启武器
		/// </summary>
		public virtual void WeaponInputStart()
		{
			if (mReloading)
			{
				return;
			}

			if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
			{
				mTriggerReleased = false;
				TurnWeaponOn();
			}
		}

		/// <summary>
		/// 描述当武器的输入被释放时发生的情况
		/// </summary>
		public virtual void WeaponInputReleased()
		{

		}

		/// <summary>
		/// 描述当武器开始时发生的情况
		/// </summary>
		public virtual void TurnWeaponOn()
		{
			if (!InputAuthorized && (Time.time - mLastTurnWeaponOnAt < TimeBetweenUses))
			{
				return;
			}

			mLastTurnWeaponOnAt = Time.time;

			TriggerWeaponStartFeedback();
			WeaponState.ChangeState(WeaponStates.WeaponStart);
			if ((mCharacterMovement != null) && (ModifyMovementWhileAttacking))
			{
				mMovementMultiplierStorage = mCharacterMovement.MovementSpeedMultiplier;
				mCharacterMovement.MovementSpeedMultiplier = MovementMultiplier;
				mMovementSpeedMultiplierSet = true;
			}

			if (PreventAllMovementWhileInUse && (mCharacterMovement != null) && (mController != null))
			{
				mCharacterMovement.SetMovement(Vector2.zero);
				mCharacterMovement.MovementForbidden = true;
			}
		}

		/// <summary>
		/// 在Update中，我们检查武器是否被使用或应该被使用
		/// </summary>
		protected virtual void Update()
		{
			FlipWeapon();
			ApplyOffset();
		}

		/// <summary>
		/// 在LateUpdate中，处理武器状态
		/// </summary>
		protected virtual void LateUpdate()
		{
			ProcessWeaponState();
		}

		/// <summary>
		/// 每个lastUpdate调用，处理武器的状态机
		/// </summary>
		protected virtual void ProcessWeaponState()
		{
			if (WeaponState == null)
			{
				return;
			}

			switch (WeaponState.CurrentState)
			{
				case WeaponStates.WeaponIdle:
					CaseWeaponIdle();
					break;

				case WeaponStates.WeaponStart:
					CaseWeaponStart();
					break;

				case WeaponStates.WeaponDelayBeforeUse:
					CaseWeaponDelayBeforeUse();
					break;

				case WeaponStates.WeaponUse:
					CaseWeaponUse();
					break;

				case WeaponStates.WeaponDelayBetweenUses:
					CaseWeaponDelayBetweenUses();
					break;

				case WeaponStates.WeaponStop:
					CaseWeaponStop();
					break;

				case WeaponStates.WeaponReloadNeeded:
					CaseWeaponReloadNeeded();
					break;

				case WeaponStates.WeaponReloadStart:
					CaseWeaponReloadStart();
					break;

				case WeaponStates.WeaponReload:
					CaseWeaponReload();
					break;

				case WeaponStates.WeaponReloadStop:
					CaseWeaponReloadStop();
					break;

				case WeaponStates.WeaponInterrupted:
					CaseWeaponInterrupted();
					break;
					
				case WeaponStates.WeaponSwitching:
					CaseWeaponSwitching();
					break;
			}
		}

		/// <summary>
		/// 如果武器处于空闲状态，我们重置移动乘数
		/// </summary>
		public virtual void CaseWeaponIdle()
		{
			ResetMovementMultiplier();
		}
		
		/// <summary>
		/// 当武器处于切换状态时的处理
		/// </summary>
		public virtual void CaseWeaponSwitching()
		{
			// 在切换状态下，重置移动乘数
			ResetMovementMultiplier();
		}

		/// <summary>
		/// 当武器开始时，我们根据武器设置切换到延迟或射击状态
		/// </summary>
		public virtual void CaseWeaponStart()
		{
			if (DelayBeforeUse > 0)
			{
				mDelayBeforeUseCounter = DelayBeforeUse;
				WeaponState.ChangeState(WeaponStates.WeaponDelayBeforeUse);
			}
			else
			{
				ShootRequestCo().Forget();
			}
		}

		/// <summary>
		/// 如果我们处于使用前延迟状态，我们等待直到延迟过去然后请求射击
		/// </summary>
		public virtual void CaseWeaponDelayBeforeUse()
		{
			mDelayBeforeUseCounter -= Time.deltaTime;
			if (mDelayBeforeUseCounter <= 0)
			{
				ShootRequestCo().Forget();
			}
		}

		/// <summary>
		/// 在武器使用时，我们使用我们的武器然后切换到使用间隔延迟状态
		/// </summary>
		public virtual void CaseWeaponUse()
		{
			WeaponUse();
			mDelayBetweenUsesCounter = TimeBetweenUses;
			WeaponState.ChangeState(WeaponStates.WeaponDelayBetweenUses);
		}

		/// <summary>
		/// 当处于使用间隔延迟状态时，我们要么关闭武器，要么发出射击请求
		/// </summary>
		public virtual void CaseWeaponDelayBetweenUses()
		{
			if (mTriggerReleased && TimeBetweenUsesReleaseInterruption)
			{
				TurnWeaponOff();
				return;
			}

			mDelayBetweenUsesCounter -= Time.deltaTime;
			if (mDelayBetweenUsesCounter <= 0)
			{
				ShootRequestCo().Forget();
			}
		}

		/// <summary>
		/// 在武器停止时，我们切换到空闲状态
		/// </summary>
		public virtual void CaseWeaponStop()
		{
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

		/// <summary>
		/// 如果需要装弹，我们提及它并切换到空闲状态
		/// </summary>
		public virtual void CaseWeaponReloadNeeded()
		{
			ReloadNeeded();
			ResetMovementMultiplier();
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

		/// <summary>
		/// 在装弹开始时，我们装弹武器并切换到装弹状态
		/// </summary>
		public virtual void CaseWeaponReloadStart()
		{
			ReloadWeapon();
			mReloadingCounter = ReloadTime;
			WeaponState.ChangeState(WeaponStates.WeaponReload);
		}

		/// <summary>
		/// 在装弹时，我们重置移动乘数，并在装弹延迟过去后切换到装弹停止状态
		/// </summary>
		public virtual void CaseWeaponReload()
		{
			ResetMovementMultiplier();
			mReloadingCounter -= Time.deltaTime;
			if (mReloadingCounter <= 0)
			{
				WeaponState.ChangeState(WeaponStates.WeaponReloadStop);
			}
		}

		/// <summary>
		/// 在装弹停止时，我们切换到空闲状态并装入弹药
		/// </summary>
		public virtual void CaseWeaponReloadStop()
		{
			mReloading = false;
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

		/// <summary>
		/// 在武器被中断时，我们关闭武器并切换回空闲状态
		/// </summary>
		public virtual void CaseWeaponInterrupted()
		{
			TurnWeaponOff();
			ResetMovementMultiplier();
			if ((WeaponState.CurrentState == WeaponStates.WeaponReload)
			    || (WeaponState.CurrentState == WeaponStates.WeaponReloadStart)
			    || (WeaponState.CurrentState == WeaponStates.WeaponReloadStop))
			{
				return;
			}

			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

		/// <summary>
		/// 调用此方法以中断武器
		/// </summary>
		public virtual void Interrupt()
		{
			if ((WeaponState.CurrentState == WeaponStates.WeaponReload)
			    || (WeaponState.CurrentState == WeaponStates.WeaponReloadStart)
			    || (WeaponState.CurrentState == WeaponStates.WeaponReloadStop))
			{
				return;
			}

			if (Interruptable)
			{
				WeaponState.ChangeState(WeaponStates.WeaponInterrupted);
			}
		}

		/// <summary>
		/// 确定武器是否可以开火
		/// </summary>
		public virtual async UniTask ShootRequestCo()
		{
			if (Time.time - mLastShootRequestAt < TimeBetweenUses)
			{
				return;
			}

			int remainingShots = UseBurstMode ? BurstLength : 1;
			float interval = UseBurstMode ? BurstTimeBetweenShots : 1;

			while (remainingShots > 0)
			{
				ShootRequest();
				mLastShootRequestAt = Time.time;
				remainingShots--;
				var error = await UniTask.Delay(TimeSpan.FromSeconds(interval),cancellationToken:destroyCancellationToken,cancelImmediately:true).SuppressCancellationThrow();
				if (error) return;
			}
		}

		public virtual void ShootRequest()
		{
			// 如果我们有武器弹药组件，我们判断是否有足够的弹药射击
			if (mReloading)
			{
				return;
			}

			if (mWeaponPreventShooting != null)
			{
				if (!mWeaponPreventShooting.ShootingAllowed())
				{
					return;
				}
			}

			if (MagazineBased)
			{
				if (CurrentAmmoLoaded > 0)
				{
					WeaponState.ChangeState(WeaponStates.WeaponUse);
					CurrentAmmoLoaded -= AmmoConsumedPerShot;
				}
				else
				{
					if (AutoReload)
					{
						InitiateReloadWeapon();
					}
					else
					{
						WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
					}
				}
			}
			else
			{
				WeaponState.ChangeState(WeaponStates.WeaponUse);
			}
		}

		/// <summary>
		/// 当武器被使用时，播放相应的声音
		/// </summary>
		public virtual void WeaponUse()
		{
			TriggerWeaponUsedFeedback();
		}
		
		/// <summary>
		/// 开始武器切换动画
		/// </summary>
		/// <param name="onComplete">动画完成后的回调</param>
		public virtual void StartWeaponSwitchingAnimation(System.Action onComplete = null)
		{
			// 如果武器已经在特殊状态，如装弹或切换中，则不允许切换
			if (mReloading || WeaponState.CurrentState == WeaponStates.WeaponSwitching)
			{
				return;
			}
			
			// 如果武器当前正在使用，中断当前操作
			if (WeaponState.CurrentState != WeaponStates.WeaponIdle && 
			    WeaponState.CurrentState != WeaponStates.WeaponStop)
			{
				Interrupt();
			}
			
			// 切换到武器切换状态
			WeaponState.ChangeState(WeaponStates.WeaponSwitching);
			
			// 启动旋转动画
			Transform weaponTransform = CachedTransform;
			if (weaponTransform != null)
			{
				LMotion.Create(weaponTransform.rotation, Quaternion.Euler(0, 0, 90), WeaponSwitchSpeed)
					.WithOnComplete(() => {
						// 动画完成后，切换回空闲状态
						WeaponState.ChangeState(WeaponStates.WeaponIdle);
						
						// 如果有回调，执行回调
						onComplete?.Invoke();
					})
					.WithEase(Ease.Linear)
					.AddTo(this)
					.BindToRotation(weaponTransform);
			}
			else
			{
				// 如果没有Display变换，直接执行回调
				WeaponState.ChangeState(WeaponStates.WeaponIdle);
				onComplete?.Invoke();
			}
		}

		/// <summary>
		/// 由输入调用，如果在自动模式下则关闭武器
		/// </summary>
		public virtual void WeaponInputStop()
		{
			if (mReloading)
			{
				return;
			}

			mTriggerReleased = true;
			if ((mCharacterMovement != null) && (ModifyMovementWhileAttacking))
			{
				mCharacterMovement.MovementSpeedMultiplier = mMovementMultiplierStorage;
				mMovementMultiplierStorage = 1f;
			}
		}

		/// <summary>
		/// 关闭武器
		/// </summary>
		public virtual void TurnWeaponOff()
		{
			if ((WeaponState.CurrentState == WeaponStates.WeaponIdle || WeaponState.CurrentState == WeaponStates.WeaponStop))
			{
				return;
			}

			mTriggerReleased = true;

			TriggerWeaponStopFeedback();
			WeaponState.ChangeState(WeaponStates.WeaponStop);
			ResetMovementMultiplier();
			if (PreventAllMovementWhileInUse && (mCharacterMovement != null))
			{
				mCharacterMovement.MovementForbidden = false;
			}

			if (NoInputReload)
			{
				var needToReload = CurrentAmmoLoaded <= 0;

				if (needToReload)
				{
					InitiateReloadWeapon();
				}
			}
		}

		protected virtual void ResetMovementMultiplier()
		{
			if ((mCharacterMovement != null) && (ModifyMovementWhileAttacking) && mMovementSpeedMultiplierSet)
			{
				mCharacterMovement.MovementSpeedMultiplier = mMovementMultiplierStorage;
				mMovementMultiplierStorage = 1f;
				mMovementSpeedMultiplierSet = false;
			}
		}

		/// <summary>
		/// 描述武器需要装填时会发生什么
		/// </summary>
		public virtual void ReloadNeeded()
		{
			TriggerWeaponReloadNeededFeedback();
		}

		/// <summary>
		/// 开始装填
		/// </summary>
		public virtual void InitiateReloadWeapon()
		{
			// 如果武器正在切换中，则不允许装弹
			if (WeaponState.CurrentState == WeaponStates.WeaponSwitching)
			{
				return;
			}
			
			if (PreventReloadIfAmmoEmpty)
			{
				return;
			}

			if (mReloading || !MagazineBased)
			{
				return;
			}

			if (PreventAllMovementWhileInUse && (mCharacterMovement != null))
			{
				mCharacterMovement.MovementForbidden = false;
			}

			WeaponState.ChangeState(WeaponStates.WeaponReloadStart);
			mReloading = true;
		}

		/// <summary>
		/// 装填武器
		/// </summary>
		protected virtual void ReloadWeapon()
		{
			if (MagazineBased)
			{
				TriggerWeaponReloadFeedback();
			}
		}

		/// <summary>
		/// 翻转武器
		/// </summary>
		public virtual void FlipWeapon()
		{
			if (!WeaponShouldFlip)
			{
				return;
			}

			if (Display != null)
			{
				Flipped = false;
				Display.localScale = new Vector2(1f, CharacterHandleWeapon.WeaponAttachment.localRotation.eulerAngles.z is > 90 and < 270 ? -1f : 1f);
			}
		}

		/// <summary>
		/// 销毁武器
		/// </summary>
		/// <returns>销毁过程</returns>
		public virtual async UniTask WeaponDestruction()
		{
			var error = await UniTask.Delay(TimeSpan.FromSeconds(AutoDestroyWhenEmptyDelay), cancellationToken: destroyCancellationToken, cancelImmediately: true).SuppressCancellationThrow();
			if (error) return;
			// 如果我们没有更多弹药，并且需要销毁我们的武器，我们执行销毁
			TurnWeaponOff();
			Destroy(this.gameObject);
		}

		/// <summary>
		/// 应用在检查器中指定的偏移量
		/// </summary>
		public virtual void ApplyOffset()
		{
			if (!WeaponCurrentlyActive)
			{
				return;
			}

			mWeaponAttachmentOffset = WeaponAttachmentOffset;

			if (Owner == null)
			{
				return;
			}
			
			if (transform.parent != null)
			{
				mWeaponOffset = mWeaponAttachmentOffset;
				Display.localPosition = mWeaponOffset;
			}
		}

		/// <summary>
		/// 播放武器的启动音效
		/// </summary>
		protected virtual void TriggerWeaponStartFeedback()
		{
		}

		/// <summary>
		/// 播放武器的使用音效
		/// </summary>
		protected virtual void TriggerWeaponUsedFeedback()
		{
			ShootSound?.Task.ContinueWith(t1 =>
			{
				if (t1 && this)
					AudioSource.PlayOneShot(t1);
			});
		}

		/// <summary>
		/// 播放武器的停止音效
		/// </summary>
		protected virtual void TriggerWeaponStopFeedback()
		{
		}

		/// <summary>
		/// 播放武器需要装填的音效
		/// </summary>
		protected virtual void TriggerWeaponReloadNeededFeedback()
		{
		}

		/// <summary>
		/// 播放武器装填的音效
		/// </summary>
		protected virtual void TriggerWeaponReloadFeedback()
		{
			MagazineSound?.Task.ContinueWith(t1 =>
			{
				if(t1 && this)
					AudioSource.PlayOneShot(t1);
			});
		}
	}
}