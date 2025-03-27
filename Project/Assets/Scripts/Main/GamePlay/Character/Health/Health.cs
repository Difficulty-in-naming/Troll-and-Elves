using System.Collections.Generic;
using EdgeStudio.Abilities;
using EdgeStudio.Damage;
using EdgeStudio.Odin;
using Panthea.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace EdgeStudio
{
	/// <summary>
	/// 该类管理对象的生命值，控制其潜在的生命值条，处理受伤时发生的事情，
	/// 以及死亡时发生的事情。
	/// </summary>
	[AddComponentMenu("Edge Studio/Character/Core/Health")] 
	public class Health : TopDownMonoBehaviour
	{
		[ColorFoldout("绑定")]
		[Tooltip("要禁用的模型（如果设置了）")]
		public GameObject Model;

		[ColorFoldout("状态"), ReadOnly, Tooltip("角色当前的生命值")]
		public float CurrentHealth ;
		[ReadOnly, Tooltip("如果为真，则该对象此时无法受到伤害"), ColorFoldout("状态")]
		public bool Invulnerable;

		[ColorFoldout("生命值"), InfoBox("<size=11>将此组件添加到对象上，它将具有生命值，能够受到伤害并可能死亡。</size>"), Tooltip("对象的初始生命值")]
		public float InitialHealth = 10;
		[ColorFoldout("生命值"), Tooltip("对象的最大生命值")]
		public float MaximumHealth = 10;
		[ColorFoldout("生命值"), Tooltip("如果为真，则每次启用此角色时生命值将重置（通常在场景开始时）")]
		public bool ResetHealthOnEnable = true;

		[ColorFoldout("伤害"), InfoBox("<size=11>在这里，您可以指定在对象受到伤害时实例化的效果和声音效果，以及对象在受到攻击时应闪烁的时间（仅适用于精灵）。</size>"), Tooltip("此生命对象是否可以受到伤害")]
		public bool ImmuneToDamage;

		[ColorFoldout("伤害"), Tooltip("如果将此设置为真，其他对象对该对象造成伤害时不会造成任何自我伤害")]
		public bool PreventTakeSelfDamage;

		[ColorFoldout("击退"), Tooltip("此对象是否免疫于伤害击退")]
		public bool ImmuneToKnockback;
		[ColorFoldout("击退"), Tooltip("如果受到的伤害为零，此对象是否免疫于伤害击退")]
		public bool ImmuneToKnockbackIfZeroDamage;
		[ColorFoldout("击退"), Tooltip("施加于传入击退力的乘数。0将取消所有击退，0.5将减半，1将没有效果，2将加倍击退力，等等")]
		public float KnockbackForceMultiplier = 1f;

		[ColorFoldout("死亡"), InfoBox("<size=11>在这里，您可以设置在对象死亡时实例化的效果，施加于它的力量（需要顶视控制器），添加到游戏分数的点数，以及角色应重生的位置（仅适用于非玩家角色）。</size>"), Tooltip("此对象是否应在死亡时被销毁")]
		public bool DestroyOnDeath = true;
		[ColorFoldout("死亡"), Tooltip("角色被销毁或禁用之前的时间（以秒为单位）")]
		public float DelayBeforeDestruction;
		[ColorFoldout("死亡"), Tooltip("当对象的生命值降至零时，玩家获得的点数")]
		public int PointsWhenDestroyed;
		[ColorFoldout("死亡"), Tooltip("如果设置为假，角色将在死亡位置重生，否则将移动到其初始位置（场景开始时）")]
		public bool RespawnAtInitialLocation;
		[ColorFoldout("死亡"), Tooltip("如果为真，控制器将在死亡时被禁用")]
		public bool DisableControllerOnDeath = true;
		[ColorFoldout("死亡"), Tooltip("如果为真，模型将在死亡时立即禁用（如果设置了模型）")]
		public bool DisableModelOnDeath = true;
		[ColorFoldout("死亡"), Tooltip("如果为真，角色死亡时将关闭碰撞")]
		public bool DisableCollisionsOnDeath = true;
		[ColorFoldout("死亡"), Tooltip("如果为真，角色死亡时子碰撞体也将关闭")]
		public bool DisableChildCollisionsOnDeath;
		[ColorFoldout("死亡"), Tooltip("此对象是否应在死亡时更改层")]
		public bool ChangeLayerOnDeath;
		[ColorFoldout("死亡"), Tooltip("此对象是否应在死亡时递归更改层")]
		public bool ChangeLayersRecursivelyOnDeath;
		[ColorFoldout("死亡"), Tooltip("我们应在死亡时将此角色移动到的层")]
		public LayerMask LayerOnDeath;

		[ColorFoldout("死亡"), Tooltip("如果为真，复活时颜色将重置")]
		public bool ResetColorOnRevive = true;

		[ColorFoldout("死亡"), Tooltip("定义其颜色的渲染器着色器上的属性名称"), ShowIf("ResetColorOnRevive")]
		public string ColorMaterialPropertyName = "_Color";

		[ColorFoldout("共享健康组件"), Tooltip("另一个健康组件（通常在另一个角色上），所有生命值将重定向到该组件")]
		public Health MasterHealth;
		[ColorFoldout("共享健康组件"), Tooltip("此生命将使用的DamageResistanceProcessor来处理收到的伤害")]
		public DamageResistanceProcessor TargetDamageResistanceProcessor;

		[ColorFoldout("动画"), Tooltip("目标动画器，用于传递死亡动画参数。如果留空，健康组件将尝试自动绑定")]
		public Animator TargetAnimator;
        
		public virtual float LastDamage { get; set; }
		public virtual Vector2 LastDamageDirection { get; set; }
		public virtual bool Initialized => mInitialized;

		// 受击委托
		public delegate void OnHitDelegate();
		public OnHitDelegate OnHit;

		// 复活委托
		public delegate void OnReviveDelegate();
		public OnReviveDelegate OnRevive;

		// 死亡委托
		public delegate void OnDeathDelegate();
		public OnDeathDelegate OnDeath;

		protected Vector2 mInitialPosition;
		protected Renderer mRenderer;
		protected Character mCharacter;
		protected CharacterMovement mCharacterMovement;
		protected TopDownController mController;
		
		protected Collider2D mCollider2D;
		protected CharacterController mCharacterController;
		protected bool mInitialized;
		protected Color mInitialColor;
		protected int mInitialLayer;


		// 修改为支持UniTask取消
		protected class InterruptiblesDamageOverTimeCoroutine
		{
			public UniTaskCompletionSource TaskCompletionSource;
			public DamageType DamageOverTimeType;
		}
		
		protected List<InterruptiblesDamageOverTimeCoroutine> mInterruptiblesDamageOverTimeCoroutines;
		protected List<InterruptiblesDamageOverTimeCoroutine> mDamageOverTimeCoroutines;

		#region Initialization
		
		/// <summary>
		/// 在Awake时，我们初始化我们的生命值
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
			InitializeCurrentHealth();
		}

		/// <summary>
		/// 在Start时，我们获取我们的动画器
		/// </summary>
		protected virtual void Start()
		{
			GrabAnimator();
		}
		
		/// <summary>
		/// 获取有用的组件，启用伤害并获取初始颜色
		/// </summary>
		public virtual void Initialization()
		{
			mCharacter = this.gameObject.GetComponentInParent<Character>(); 

			if (Model != null)
			{
				Model.SetActive(true);
			}        
            
			if (gameObject.GetComponentInParent<Renderer>() != null)
			{
				mRenderer = GetComponentInParent<Renderer>();				
			}
			if (mCharacter != null)
			{
				mCharacterMovement = mCharacter.FindAbility<CharacterMovement>();
				if (mCharacter.CharacterModel != null)
				{
					if (mCharacter.CharacterModel.GetComponentInChildren<Renderer> ()!= null)
					{
						mRenderer = mCharacter.CharacterModel.GetComponentInChildren<Renderer> ();	
					}
				}	
			}
			if (mRenderer != null)
			{
				if (ResetColorOnRevive)
				{
					if (mRenderer.material.HasProperty(ColorMaterialPropertyName))
					{
						mInitialColor = mRenderer.material.GetColor(ColorMaterialPropertyName);
					} 
				}
			}

			mInterruptiblesDamageOverTimeCoroutines = new List<InterruptiblesDamageOverTimeCoroutine>();
			mDamageOverTimeCoroutines = new List<InterruptiblesDamageOverTimeCoroutine>();
			mInitialLayer = gameObject.layer;

			mController = this.gameObject.GetComponentInParent<TopDownController>();
			mCharacterController = this.gameObject.GetComponentInParent<CharacterController>();
			mCollider2D = this.gameObject.GetComponentInParent<Collider2D>();

			StoreInitialPosition();
			mInitialized = true;
			
			DamageEnabled();
		}
		
		/// <summary>
		/// 获取目标动画器
		/// </summary>
		protected virtual void GrabAnimator()
		{
			if (TargetAnimator == null)
			{
				BindAnimator();
			}
		}

		/// <summary>
		/// 查找并绑定动画器（如果可能）
		/// </summary>
		protected virtual void BindAnimator()
		{
			if (mCharacter != null)
				TargetAnimator = mCharacter.Animator;
		}

		/// <summary>
		/// 存储初始位置以供进一步使用
		/// </summary>
		public virtual void StoreInitialPosition() => mInitialPosition = this.transform.position;

		/// <summary>
		/// 将生命值初始化为初始值或当前值
		/// </summary>
		public virtual void InitializeCurrentHealth()
		{
			if (MasterHealth == null)
			{
				SetHealth(InitialHealth);	
			}
			else
			{
				SetHealth(MasterHealth.Initialized ? MasterHealth.CurrentHealth : MasterHealth.InitialHealth);
			}
		}

		/// <summary>
		/// 当对象被启用时（例如在复活时），我们恢复其初始生命值
		/// </summary>
		protected virtual void OnEnable()
		{
			if (ResetHealthOnEnable)
			{
				InitializeCurrentHealth();
			}
			if (Model != null)
			{
				Model.SetActive(true);
			}            
			DamageEnabled();
		}
		
		/// <summary>
		/// 在禁用时，我们防止任何延迟销毁运行
		/// </summary>
		protected virtual void OnDisable()
		{
			CancelInvoke();
		}

		#endregion

		/// <summary>
		/// 如果此健康组件可以在此帧受到伤害，则返回true，否则返回false
		/// </summary>
		/// <returns></returns>
		public virtual bool CanTakeDamageThisFrame()
		{
			// 如果对象是无敌的，我们什么都不做并退出
			if (Invulnerable || ImmuneToDamage)
			{
				return false;
			}

			if (!this.enabled)
			{
				return false;
			}
			
			// 如果我们已经低于零，我们什么都不做并退出
			if ((CurrentHealth <= 0) && (InitialHealth != 0))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// 当对象受到伤害时调用
		/// </summary>
		/// <param name="damage">将损失的生命值点数。</param>
		/// <param name="instigator">造成伤害的对象。</param>
		/// <param name="flickerDuration">对象在受到伤害后应闪烁的时间（以秒为单位） - 不再使用，保留以不破坏向后兼容性</param>
		/// <param name="invincibilityDuration">受到攻击后短暂无敌的持续时间。</param>
		/// <param name="damageDirection"></param>
		/// <param name="typedDamages"></param>
		public virtual void Damage(float damage, GameObject instigator, float flickerDuration, float invincibilityDuration, Vector2 damageDirection, List<TypedDamage> typedDamages = null)
		{
			if (!CanTakeDamageThisFrame())
			{
				return;
			}

			damage = ComputeDamageOutput(damage, typedDamages, true);
			
			// 我们减少角色的生命值
			float previousHealth = CurrentHealth;
			if (MasterHealth != null)
			{
				previousHealth = MasterHealth.CurrentHealth;
				MasterHealth.SetHealth(MasterHealth.CurrentHealth - damage);
			}
			else
			{
				SetHealth(CurrentHealth - damage);	
			}

			LastDamage = damage;
			LastDamageDirection = damageDirection;
			if (OnHit != null)
			{
				OnHit();
			}

			// 我们防止角色与子弹、玩家和敌人碰撞
			if (invincibilityDuration > 0)
			{
				DamageDisabled();
				// 改为异步调用
				DamageEnabled(invincibilityDuration).Forget();
			}
            
			// 我们触发一个受到伤害的事件
			DamageTakenEvent.Trigger(this, instigator, CurrentHealth, damage, previousHealth, typedDamages);

			// 我们更新生命值条
			UpdateHealthBar(true);
			
			// 我们处理任何条件状态变化
			ComputeCharacterConditionStateChanges(typedDamages);
			ComputeCharacterMovementMultipliers(typedDamages);

			// 如果生命值已达到零，我们将其生命值设置为零（对生命值条有用）
			if (MasterHealth != null)
			{
				if (MasterHealth.CurrentHealth <= 0)
				{
					MasterHealth.CurrentHealth = 0;
					MasterHealth.Kill();
				}
			}
			else
			{
				if (CurrentHealth <= 0)
				{
					CurrentHealth = 0;
					Kill();
				}
					
			}
		}

		/// <summary>
		/// 中断所有伤害，忽略类型
		/// </summary>
		public virtual void InterruptAllDamageOverTime()
		{
			foreach (InterruptiblesDamageOverTimeCoroutine coroutine in mInterruptiblesDamageOverTimeCoroutines)
			{
				// 取消UniTask而不是停止协程
				if (coroutine.TaskCompletionSource != null && !coroutine.TaskCompletionSource.Task.Status.IsCompleted())
				{
					coroutine.TaskCompletionSource.TrySetCanceled();
				}
			}
			mInterruptiblesDamageOverTimeCoroutines.Clear();
		}

		/// <summary>
		/// 中断所有伤害，甚至是不可中断的（通常在死亡时）
		/// </summary>
		public virtual void StopAllDamageOverTime()
		{
			foreach (InterruptiblesDamageOverTimeCoroutine coroutine in mDamageOverTimeCoroutines)
			{
				// 取消UniTask而不是停止协程
				if (coroutine.TaskCompletionSource != null && !coroutine.TaskCompletionSource.Task.Status.IsCompleted())
				{
					coroutine.TaskCompletionSource.TrySetCanceled();
				}
			}
			mDamageOverTimeCoroutines.Clear();
		}

		/// <summary>
		/// 中断所有指定类型的伤害
		/// </summary>
		/// <param name="damageType"></param>
		public virtual void InterruptAllDamageOverTimeOfType(DamageType damageType)
		{
			foreach (InterruptiblesDamageOverTimeCoroutine coroutine in mInterruptiblesDamageOverTimeCoroutines)
			{
				if (coroutine.DamageOverTimeType == damageType)
				{
					// 取消UniTask而不是停止协程
					if (coroutine.TaskCompletionSource != null && !coroutine.TaskCompletionSource.Task.Status.IsCompleted())
					{
						coroutine.TaskCompletionSource.TrySetCanceled();
					}
				}
			}
			TargetDamageResistanceProcessor?.InterruptDamageOverTime(damageType);
		}

		/// <summary>
		/// 施加持续伤害，指定重复次数（包括第一次施加的伤害，便于在检查器中快速计算，以及指定的间隔）。
		/// 可选地，您可以决定您的伤害是可中断的，在这种情况下，调用InterruptAllDamageOverTime()将停止这些伤害的施加，方便治疗毒药等。
		/// </summary>
		public virtual void DamageOverTime(float damage, GameObject instigator, float flickerDuration,
			float invincibilityDuration, Vector2 damageDirection, List<TypedDamage> typedDamages = null,
			int amountOfRepeats = 0, float durationBetweenRepeats = 1f, bool interruptible = true, DamageType damageType = null)
		{
			if (ComputeDamageOutput(damage, typedDamages) == 0)
			{
				return;
			}
			
			InterruptiblesDamageOverTimeCoroutine damageOverTime = new InterruptiblesDamageOverTimeCoroutine();
			damageOverTime.DamageOverTimeType = damageType;
			
			// 创建UniTaskCompletionSource来管理任务
			damageOverTime.TaskCompletionSource = new UniTaskCompletionSource();
			
			// 启动异步任务而不是协程
			DamageOverTimeCo(damage, instigator, flickerDuration, invincibilityDuration, damageDirection, 
				typedDamages, amountOfRepeats, durationBetweenRepeats, interruptible, damageType, 
				damageOverTime.TaskCompletionSource).Forget();
			
			mDamageOverTimeCoroutines.Add(damageOverTime);
			if (interruptible)
			{
				mInterruptiblesDamageOverTimeCoroutines.Add(damageOverTime);
			}
		}

		/// <summary>
		/// 用于施加持续伤害的异步方法，取代协程
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="instigator"></param>
		/// <param name="flickerDuration"></param>
		/// <param name="invincibilityDuration"></param>
		/// <param name="damageDirection"></param>
		/// <param name="typedDamages"></param>
		/// <param name="amountOfRepeats"></param>
		/// <param name="durationBetweenRepeats"></param>
		/// <param name="interruptible"></param>
		/// <param name="damageType"></param>
		/// <param name="completionSource"></param>
		/// <returns></returns>
		protected virtual async UniTask DamageOverTimeCo(float damage, GameObject instigator, float flickerDuration,
			float invincibilityDuration, Vector2 damageDirection, List<TypedDamage> typedDamages = null,
			int amountOfRepeats = 0, float durationBetweenRepeats = 1f, bool interruptible = true, 
			DamageType damageType = null, UniTaskCompletionSource completionSource = null)
		{
			try
			{
				for (int i = 0; i < amountOfRepeats; i++)
				{
					Damage(damage, instigator, flickerDuration, invincibilityDuration, damageDirection, typedDamages);
					await UniTask.Delay((int)(durationBetweenRepeats * 1000)); // 转换秒到毫秒
				}
				completionSource?.TrySetResult();
			}
			catch (System.OperationCanceledException)
			{
				// 任务被取消，正常返回
			}
			catch (System.Exception ex)
			{
				completionSource?.TrySetException(ex);
			}
		}

		/// <summary>
		/// 返回此生命应承受的伤害，经过潜在抗性处理
		/// </summary>
		public virtual float ComputeDamageOutput(float damage, List<TypedDamage> typedDamages = null, bool damageApplied = false)
		{
			if (Invulnerable || ImmuneToDamage)
			{
				return 0;
			}
			
			float totalDamage = 0f;
			// 我们通过潜在的抗性处理我们的伤害
			if (TargetDamageResistanceProcessor != null)
			{
				if (TargetDamageResistanceProcessor.isActiveAndEnabled)
				{
					totalDamage = TargetDamageResistanceProcessor.ProcessDamage(damage, typedDamages, damageApplied);	
				}
			}
			else
			{
				totalDamage = damage;
				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						totalDamage += typedDamage.DamageCaused;
					}
				}
			}
			return totalDamage;
		}

		/// <summary>
		/// 遍历抗性并在需要时应用条件状态变化
		/// </summary>
		/// <param name="typedDamages"></param>
		protected virtual void ComputeCharacterConditionStateChanges(List<TypedDamage> typedDamages)
		{
			if ((typedDamages == null) || (mCharacter == null))
			{
				return;
			}
			
			foreach (TypedDamage typedDamage in typedDamages)
			{
				if (typedDamage.ForceCharacterCondition)
				{
					if (TargetDamageResistanceProcessor != null)
					{
						if (TargetDamageResistanceProcessor.isActiveAndEnabled)
						{
							bool checkResistance =
								TargetDamageResistanceProcessor.CheckPreventCharacterConditionChange(typedDamage.AssociatedDamageType);
							if (checkResistance)
							{
								continue;		
							}
						}
					}
					_ = mCharacter.ChangeCharacterConditionTemporarily(typedDamage.ForcedCondition, typedDamage.ForcedConditionDuration, typedDamage.ResetControllerForces, typedDamage.DisableGravity);	
				}
			}
			
		}

		/// <summary>
		/// 遍历抗性列表并在需要时应用移动乘数
		/// </summary>
		/// <param name="typedDamages"></param>
		protected virtual void ComputeCharacterMovementMultipliers(List<TypedDamage> typedDamages)
		{
			if ((typedDamages == null) || (mCharacter == null))
			{
				return;
			}
			
			foreach (TypedDamage typedDamage in typedDamages)
			{
				if (typedDamage.ApplyMovementMultiplier)
				{
					if (TargetDamageResistanceProcessor != null)
					{
						if (TargetDamageResistanceProcessor.isActiveAndEnabled)
						{
							bool checkResistance =
								TargetDamageResistanceProcessor.CheckPreventMovementModifier(typedDamage.AssociatedDamageType);
							if (checkResistance)
							{
								continue;		
							}
						}
					}
					mCharacterMovement?.ApplyMovementMultiplier(typedDamage.MovementMultiplier,
						typedDamage.MovementMultiplierDuration);
				}
			}
		}
		
		/// <summary>
		/// 通过抗性处理确定新的击退力
		/// </summary>
		/// <param name="knockbackForce"></param>
		/// <param name="typedDamages"></param>
		/// <returns></returns>
		public virtual Vector2 ComputeKnockbackForce(Vector2 knockbackForce, List<TypedDamage> typedDamages = null) => TargetDamageResistanceProcessor == null ? knockbackForce : TargetDamageResistanceProcessor.ProcessKnockbackForce(knockbackForce, typedDamages);

		/// <summary>
		/// 如果此生命可以被击退，则返回true，否则返回false
		/// </summary>
		/// <param name="typedDamages"></param>
		/// <returns></returns>
		public virtual bool CanGetKnockback(List<TypedDamage> typedDamages) 
		{
			if (ImmuneToKnockback)
			{
				return false;
			}
			if (TargetDamageResistanceProcessor != null)
			{
				if (TargetDamageResistanceProcessor.isActiveAndEnabled)
				{
					bool checkResistance = TargetDamageResistanceProcessor.CheckPreventKnockback(typedDamages);
					if (checkResistance)
					{
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// 杀死角色，实例化死亡效果，处理点数等
		/// </summary>
		public virtual void Kill()
		{
			if (ImmuneToDamage)
			{
				return;
			}
	        
			if (mCharacter != null)
			{
				// 我们将其死亡状态设置为真
				mCharacter.ConditionState.ChangeState(CharacterStates.CharacterConditions.Dead);
				mCharacter.Reset();
			}
			SetHealth(0);

			// 我们防止进一步的伤害
			StopAllDamageOverTime();
			DamageDisabled();

			// 我们从现在开始使其忽略碰撞
			if (DisableCollisionsOnDeath)
			{
				if (mCollider2D != null)
				{
					mCollider2D.enabled = false;
				}

				// 如果我们有控制器，移除碰撞，恢复参数以便可能重生，并施加死亡力量
				if (mController != null)
				{				
					mController.CollisionsOff();						
				}

				if (DisableChildCollisionsOnDeath)
				{
					foreach (Collider2D c in this.gameObject.GetComponentsInChildren<Collider2D>())
					{
						c.enabled = false;
					}
				}
			}

			if (ChangeLayerOnDeath)
			{
				gameObject.layer = LayerOnDeath.value;
				if (ChangeLayersRecursivelyOnDeath)
				{
					this.transform.ChangeLayersRecursively(LayerOnDeath.value);
				}
			}
            
			OnDeath?.Invoke();
			LifeCycleEvent.Trigger(this);

			if (DisableControllerOnDeath && (mController != null))
			{
				mController.enabled = false;
			}

			if (DisableControllerOnDeath && (mCharacterController != null))
			{
				mCharacterController.enabled = false;
			}

			if (DisableModelOnDeath && (Model != null))
			{
				Model.SetActive(false);
			}

			if (DelayBeforeDestruction > 0f)
			{
				Invoke (nameof(DestroyObject), DelayBeforeDestruction);
			}
			else
			{
				// 最后我们销毁对象
				DestroyObject();	
			}
		}

		/// <summary>
		/// 复活此对象。
		/// </summary>
		public virtual void Revive()
		{
			if (!mInitialized)
			{
				return;
			}

			if (mCollider2D != null)
			{
				mCollider2D.enabled = true;
			}
			if (DisableChildCollisionsOnDeath)
			{
				foreach (Collider2D c in this.gameObject.GetComponentsInChildren<Collider2D>())
				{
					c.enabled = true;
				}
			}
			if (ChangeLayerOnDeath)
			{
				gameObject.layer = mInitialLayer;
				if (ChangeLayersRecursivelyOnDeath)
				{
					this.transform.ChangeLayersRecursively(mInitialLayer);
				}
			}
			if (mCharacterController != null)
			{
				mCharacterController.enabled = true;
			}
			if (mController != null)
			{
				mController.enabled = true;
				mController.CollisionsOn();
				mController.Reset();
			}
			if (mCharacter != null)
			{
				mCharacter.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
			}
			if (ResetColorOnRevive && (mRenderer != null))
			{
				mRenderer.material.SetColor(ColorMaterialPropertyName, mInitialColor);
			}            

			if (RespawnAtInitialLocation)
			{
				transform.position = mInitialPosition;
			}

			Initialization();
			InitializeCurrentHealth();
			OnRevive?.Invoke();
			LifeCycleEvent.Trigger(this, LifeCycleEventTypes.Revive);
		}

		/// <summary>
		/// 销毁对象，或根据角色的设置尝试销毁
		/// </summary>
		protected virtual void DestroyObject()
		{
		}

		#region HealthManipulationAPIs
		

		/// <summary>
		/// 将当前生命值设置为指定的新值，并更新生命值条
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetHealth(float newValue)
		{
			CurrentHealth = newValue;
			UpdateHealthBar(false);
			HealthChangeEvent.Trigger(this, newValue);
		}
		
		/// <summary>
		/// 当角色获得生命值（例如来自刺激包）时调用
		/// </summary>
		/// <param name="health">角色获得的生命值。</param>
		/// <param name="instigator">给予角色生命值的事物。</param>
		public virtual void ReceiveHealth(float health,GameObject instigator)
		{
			// 此函数将生命值添加到角色的生命值中，并防止其超过最大生命值。
			if (MasterHealth != null)
			{
				MasterHealth.SetHealth(Mathf.Min (CurrentHealth + health,MaximumHealth));	
			}
			else
			{
				SetHealth(Mathf.Min (CurrentHealth + health,MaximumHealth));	
			}
			UpdateHealthBar(true);
		}
		
		/// <summary>
		/// 将角色的生命值重置为最大值
		/// </summary>
		public virtual void ResetHealthToMaxHealth()
		{
			SetHealth(MaximumHealth);
		}
		
		/// <summary>
		/// 强制刷新角色的生命值条
		/// </summary>
		public virtual void UpdateHealthBar(bool show)
		{
		}
		#endregion
		
		#region DamageDisablingAPIs

		/// <summary>
		/// 防止角色受到任何伤害
		/// </summary>
		public virtual void DamageDisabled()
		{
			Invulnerable = true;
		}

		/// <summary>
		/// 允许角色受到伤害
		/// </summary>
		public virtual void DamageEnabled()
		{
			Invulnerable = false;
		}

		/// <summary>
		/// 在指定延迟后使角色能够再次受到伤害（使用UniTask代替协程）
		/// </summary>
		public virtual async UniTask DamageEnabled(float delay)
		{
			await UniTask.Delay((int)(delay * 1000)); // 将秒转换为毫秒
			Invulnerable = false;
		}

		#endregion
	}
}