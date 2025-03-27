using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EdgeStudio.Abilities;
using EdgeStudio.Odin;
using Panthea.Utils;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace EdgeStudio
{
	[AddComponentMenu("Edge Studio/Character/Core/Character"),DefaultExecutionOrder(int.MinValue)]
	public class Character : TopDownMonoBehaviour
	{
		[LabelText("队伍"), ColorFoldout("基础信息")]
		public CharacterTeam Team = CharacterTeam.Player1;
		
		[ColorFoldout("绑定")]public FollowerEntity Pathfinder;

		[FormerlySerializedAs("Animation")] [ColorFoldout("绑定"), InfoBox("<size=11>常规精灵角色若SpriteRenderer与角色组件在同一GameObject上可不绑定。若角色模型需要独立移动翻转，请将模型对象拖拽至下方绑定。碰撞体会保持原位不受模型变换影响。</size>")]
		public Animator Animator;
		[ColorFoldout("绑定")]public GameObject CharacterModel;
		[ColorFoldout("绑定")]public Health CharacterHealth;

		[ColorFoldout("事件"), InfoBox("<size=11>启用此选项后角色状态变化时将触发事件，详细信息请参考MMTools状态机文档。</size>")]
		public bool SendStateChangeEvents = true;
		[ColorFoldout("能力")] public List<GameObject> AdditionalAbilityNodes;
		public CharacterStates CharacterState;
		public StateMachine<CharacterStates.MovementStates> MovementState;
		public StateMachine<CharacterStates.CharacterConditions> ConditionState;
		
		public virtual GameObject CameraTarget { get; set; }
		public virtual Vector2 CameraDirection { get; protected set; }

		protected CharacterAbility[] mCharacterAbilities;
		protected bool mAbilitiesCachedOnce;
		protected TopDownController mController;

		protected bool mOnReviveRegistered;
		protected CharacterStates.CharacterConditions mLastState;
		protected CancellationTokenSource mConditionChangeCancellation;

		protected int mAliveAnimationParameter;
		protected int mCurrentSpeedAnimationParameter;
		protected int mXSpeedAnimationParameter;
		protected int mYSpeedAnimationParameter;
		protected int mIdleAnimationParameter;

		/// <summary> 初始化角色的实例 </summary>
		protected virtual void Awake() => Initialization();

		/// <summary>
		/// 获取并存储输入管理器、相机和组件
		/// </summary>
		protected virtual void Initialization()
		{
			mController = CachedGameObject.GetComponentNoAlloc<TopDownController>();
			
			MovementState = new StateMachine<CharacterStates.MovementStates>(CachedGameObject, SendStateChangeEvents);
			ConditionState = new StateMachine<CharacterStates.CharacterConditions>(CachedGameObject, SendStateChangeEvents);

			CharacterState = new CharacterStates();
			if (CharacterHealth == null)
			{
				CharacterHealth = CachedGameObject.GetComponent<Health>();
			}

			CacheAbilitiesAtInit();
			InitializeAnimatorParameters();

			if (CameraTarget == null)
			{
				CameraTarget = new GameObject();
			}

			CameraTarget.transform.SetParent(this.transform);
			CameraTarget.transform.localPosition = Vector2.zero;
			CameraTarget.name = "CameraTarget";
		}

		private void InitializeAnimatorParameters()
		{
			mAliveAnimationParameter = Animator.StringToHash("Alive");
			mCurrentSpeedAnimationParameter = Animator.StringToHash("Speed");
			mXSpeedAnimationParameter = Animator.StringToHash("xSpeed");
			mYSpeedAnimationParameter = Animator.StringToHash("ySpeed");
			mIdleAnimationParameter = Animator.StringToHash("Idle");
		}
		
		/// <summary>
		/// 如果需要，缓存能力
		/// </summary>
		protected virtual void CacheAbilitiesAtInit()
		{
			if (mAbilitiesCachedOnce)
			{
				return;
			}

			CacheAbilities();
		}

		/// <summary>
		/// 获取能力并缓存以供进一步使用
		/// 确保在运行时添加能力时调用此方法
		/// 您应该避免在运行时添加组件，因为这很耗费性能，
		/// 最好是激活/禁用组件。
		/// </summary>
		public virtual void CacheAbilities()
		{
			// 我们获取所有在我们层级上的能力
			mCharacterAbilities = this.gameObject.GetComponents<CharacterAbility>();

			// 如果用户指定了更多节点
			if (AdditionalAbilityNodes is { Count: > 0 })
			{
				// 创建一个临时列表
				using var tempAbilityList = ListPool<CharacterAbility>.Create();

				// 将我们已经找到的所有能力放入列表中
				for (int i = 0; i < mCharacterAbilities.Length; i++)
				{
					tempAbilityList.Add(mCharacterAbilities[i]);
				}

				// 从节点中添加能力
				for (int j = 0; j < AdditionalAbilityNodes.Count; j++)
				{
					CharacterAbility[] tempArray = AdditionalAbilityNodes[j].GetComponentsInChildren<CharacterAbility>();
					foreach (CharacterAbility ability in tempArray)
					{
						tempAbilityList.Add(ability);
					}
				}

				mCharacterAbilities = tempAbilityList.ToArray();
			}

			mAbilitiesCachedOnce = true;
		}

		/// <summary>
		/// 强制（重新）初始化角色的能力
		/// </summary>
		public virtual void ForceAbilitiesInitialization()
		{
			for (int i = 0; i < mCharacterAbilities.Length; i++)
			{
				mCharacterAbilities[i].ForceInitialization();
			}

			for (int j = 0; j < AdditionalAbilityNodes.Count; j++)
			{
				CharacterAbility[] tempArray = AdditionalAbilityNodes[j].GetComponentsInChildren<CharacterAbility>();
				foreach (CharacterAbility ability in tempArray)
				{
					ability.ForceInitialization();
				}
			}
		}

		/// <summary>
		/// 检查角色是否具有某种能力的方法
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T FindAbility<T>() where T : CharacterAbility
		{
			CacheAbilitiesAtInit();

			foreach (CharacterAbility ability in mCharacterAbilities)
			{
				if (ability is T characterAbility)
				{
					return characterAbility;
				}
			}

			return null;
		}

		/// <summary>
		/// 检查角色是否具有某种能力的方法
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public List<T> FindAbilities<T>() where T : CharacterAbility
		{
			CacheAbilitiesAtInit();

			List<T> resultList = new List<T>();

			foreach (CharacterAbility ability in mCharacterAbilities)
			{
				if (ability is T characterAbility)
				{
					resultList.Add(characterAbility);
				}
			}

			return resultList;
		}

		/// <summary>
		/// 重置所有能力的输入
		/// </summary>
		public virtual void ResetInput()
		{
			if (mCharacterAbilities == null)
			{
				return;
			}

			foreach (CharacterAbility ability in mCharacterAbilities)
			{
				ability.ResetInput();
			}
		}

		protected virtual void Update() => EveryFrame();

		protected virtual void EveryFrame()
		{
			EarlyProcessAbilities();
			ProcessAbilities();
			LateProcessAbilities();
			
			UpdateAnimators();
		}

		protected virtual void UpdateAnimators()
		{
			Animator.SetBool(mAliveAnimationParameter, ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead);
			Animator.SetFloat(mCurrentSpeedAnimationParameter,Mathf.Abs(mController.CurrentMovement.magnitude));
			Animator.SetFloat(mXSpeedAnimationParameter,mController.CurrentMovement.x);
			Animator.SetFloat(mYSpeedAnimationParameter,mController.CurrentMovement.y);
			Animator.SetBool(mIdleAnimationParameter, MovementState.CurrentState == CharacterStates.MovementStates.Idle);

			foreach (CharacterAbility ability in mCharacterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{	
					ability.UpdateAnimator();
				}
			}
		}
		
		/// <summary>
		/// 调用所有注册能力的早期处理方法
		/// </summary>
		protected virtual void EarlyProcessAbilities()
		{
			foreach (CharacterAbility ability in mCharacterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{
					ability.EarlyProcessAbility();
				}
			}
		}

		/// <summary>
		/// 调用所有注册能力的处理方法
		/// </summary>
		protected virtual void ProcessAbilities()
		{
			foreach (CharacterAbility ability in mCharacterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{
					ability.ProcessAbility();
				}
			}
		}

		/// <summary>
		/// 调用所有注册能力的后期处理方法
		/// </summary>
		protected virtual void LateProcessAbilities()
		{
			foreach (CharacterAbility ability in mCharacterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{
					ability.LateProcessAbility();
				}
			}
		}

		public virtual void RespawnAt(Vector2 spawnPosition, FacingDirections facingDirection)
		{
			transform.position = spawnPosition;

			if (!gameObject.activeInHierarchy)
			{
				gameObject.SetActive(true);
			}

			// 将角色从死亡状态恢复（如果角色已死亡）
			ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
			// 重新启用2D碰撞器
			if (this.gameObject.GetComponentNoAlloc<Collider2D>() != null)
			{
				this.gameObject.GetComponentNoAlloc<Collider2D>().enabled = true;
			}

			// 重新启用碰撞检测
			mController.enabled = true;
			mController.CollisionsOn();
			mController.Reset();

			Reset();
			UnFreeze();

			if (CharacterHealth != null)
			{
				CharacterHealth.StoreInitialPosition();
				CharacterHealth.ResetHealthToMaxHealth();
				CharacterHealth.Revive();
			}
		}

		/// <summary>
		/// 使玩家在指定位置重生
		/// </summary>
		/// <param name="spawnPoint">重生位置</param>
		/// <param name="facingDirection">朝向</param>
		public virtual void RespawnAt(Transform spawnPoint, FacingDirections facingDirection) => RespawnAt(spawnPoint.position, facingDirection);

		/// <summary>
		/// 使用此方法临时改变角色状态，并在之后重置。
		/// 你也可以用它来临时禁用重力，并可选择重置力。
		/// </summary>
		/// <param name="newCondition">新状态</param>
		/// <param name="duration">持续时间</param>
		/// <param name="resetControllerForces">是否重置控制器力</param>
		/// <param name="disableGravity">是否禁用重力</param>
		public virtual async UniTaskVoid ChangeCharacterConditionTemporarily(CharacterStates.CharacterConditions newCondition,
			float duration, bool resetControllerForces, bool disableGravity)
		{
			if (mConditionChangeCancellation != null)
			{
				mConditionChangeCancellation.Cancel();
				mConditionChangeCancellation.Dispose();
			}

			mConditionChangeCancellation = new CancellationTokenSource();
			try
			{
				await ChangeCharacterConditionTemporarilyCo(newCondition, duration, resetControllerForces, disableGravity)
					.AttachExternalCancellation(mConditionChangeCancellation.Token);
			}
			catch (OperationCanceledException)
			{
			}
		}

		/// <summary>
		/// 处理由ChangeCharacterConditionTemporarily调用的临时状态改变
		/// </summary>
		protected virtual async UniTask ChangeCharacterConditionTemporarilyCo(
			CharacterStates.CharacterConditions newCondition,
			float duration, bool resetControllerForces, bool disableGravity)
		{
			if (mLastState != newCondition)
				if (mLastState != newCondition && this.ConditionState.CurrentState != newCondition)
				{
					mLastState = this.ConditionState.CurrentState;
				}

			this.ConditionState.ChangeState(newCondition);
			if (resetControllerForces)
			{
				mController?.SetMovement(Vector2.zero);
			}

			await UniTask.Delay((int)(duration * 1000));

			this.ConditionState.ChangeState(mLastState);
		}

		/// <summary>
		/// 存储关联的相机方向
		/// </summary>
		public virtual void SetCameraDirection(Vector2 direction)
		{
			CameraDirection = direction;
		}

		/// <summary>
		/// 冻结此角色
		/// </summary>
		public virtual void Freeze()
		{
			mController.SetMovement(Vector2.zero);
			ConditionState.ChangeState(CharacterStates.CharacterConditions.Frozen);
		}

		/// <summary>
		/// 解冻此角色
		/// </summary>
		public virtual void UnFreeze()
		{
			if (ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen)
			{
				ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
			}
		}

		/// <summary>
		/// 在关卡结束时调用以禁用玩家。
		/// 之后将不会移动和响应输入。
		/// </summary>
		public virtual void Disable()
		{
			this.enabled = false;
			mController.enabled = false;
		}

		public virtual void FlipAllAbilities()
		{
			foreach (CharacterAbility ability in mCharacterAbilities)
			{
				if (ability.enabled)
				{
					ability.Flip();
				}
			}
		}
		
		/// <summary>
		/// 当角色死亡时调用。
		/// 调用每个能力的Reset()方法，以便在需要时将设置恢复为原始值
		/// </summary>
		public virtual void Reset()
		{
			if (mCharacterAbilities == null)
			{
				return;
			}

			if (mCharacterAbilities.Length == 0)
			{
				return;
			}

			foreach (CharacterAbility ability in mCharacterAbilities)
			{
				if (ability.enabled)
				{
					ability.ResetAbility();
				}
			}
		}

		/// <summary>
		/// 在复活时，我们强制设置生成方向
		/// </summary>
		protected virtual void OnRevive()
		{
			if (Pathfinder is not null)
			{
				Pathfinder.SetPath(null);
				Pathfinder.enabled = true;
			}
		}

		protected virtual void OnDeath()
		{
			MovementState.ChangeState(CharacterStates.MovementStates.Idle);
		}

		protected virtual void OnHit()
		{
		}

		protected virtual void OnEnable()
		{
			if (CharacterHealth != null)
			{
				if (!mOnReviveRegistered)
				{
					CharacterHealth.OnRevive += OnRevive;
					mOnReviveRegistered = true;
				}

				CharacterHealth.OnDeath += OnDeath;
				CharacterHealth.OnHit += OnHit;
			}
		}

		protected virtual void OnDisable()
		{
			if (CharacterHealth != null)
			{
				CharacterHealth.OnDeath -= OnDeath;
				CharacterHealth.OnHit -= OnHit;
			}
		}
	}
}