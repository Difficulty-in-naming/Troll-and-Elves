using System.Collections;
using EdgeStudio.Damage;
using MoreMountains.TopDownEngine;
using Panthea.Common;
using UnityEngine;

namespace EdgeStudio.Weapons
{	
	/// <summary>
	/// 用于投射武器的弹药类
	/// </summary>
	[AddComponentMenu("TopDown Engine/Weapons/Projectile")]
	public class Projectile : BetterMonoBehaviour  
	{
		public enum MovementVectors { Forward, Right, Up}
		
		[Header("Movement")]
		[Tooltip("如果为true，弹药在初始化时将朝向其旋转方向")]
		public bool FaceDirection = true;
		[Tooltip("如果为true，弹药将朝向移动方向")]
		public bool FaceMovement;
		[Tooltip("如果FaceMovement为true，下面指定的弹药向量将与移动向量对齐，通常在3D中使用Forward，在2D中使用Right")]
		[Sirenix.OdinInspector.ShowIf("FaceMovement")]
		public MovementVectors MovementVector = MovementVectors.Forward;

		[Tooltip("物体的速度（相对于关卡的速度）")]
		public float Speed;
		[Tooltip("物体随时间的加速度。在启用时开始加速。")]
		public float Acceleration;
		[Tooltip("物体的当前方向")]
		public Vector3 Direction = Vector3.left;
		[Tooltip("如果设置为true，发射器可以改变物体的方向。如果为false，则使用检查器中设置的方向。")]
		public bool DirectionCanBeChangedBySpawner = true;
		[Tooltip("当弹药被镜像时要应用的翻转系数")]
		public Vector3 FlipValue = new(-1,1,1);
		[Tooltip("如果你的弹药模型（或精灵）朝右，将此设置为true，否则为false")]
		public bool ProjectileIsFacingRight = true;

		[Header("Spawn")]
		// [("<size=11>在这里你可以定义一个初始延迟（以秒为单位），在此期间该物体不会造成或受到伤害。此延迟从物体启用时开始计时。你还可以定义弹药是否应该伤害其所有者（想想火箭之类的）</size>",MMInformationAttribute.InformationType.Info,false)]
		[Tooltip("弹药不能被摧毁的初始延迟时间")]
		public float InitialInvulnerabilityDuration;
		[Tooltip("弹药是否应该伤害其所有者？")]
		public bool DamageOwner;

		/// <summary>
		/// 返回关联的触碰伤害区域
		/// </summary>
		public virtual DamageOnTouch TargetDamageOnTouch { get { return mDamageOnTouch; } }
		/// <summary>
		/// 返回源武器
		/// </summary>
		public virtual Weapon SourceWeapon { get { return mWeapon; } }

		protected Weapon mWeapon;
		protected GameObject mOwner;
		protected Vector3 mMovement;
		protected float mInitialSpeed;
		protected SpriteRenderer mSpriteRenderer;
		protected DamageOnTouch mDamageOnTouch;
		protected WaitForSeconds mInitialInvulnerabilityDurationWfs;
		protected Collider2D mCollider2D;
		protected Rigidbody2D mRigidBody2D;
		protected bool mFacingRightInitially;
		protected bool mInitialFlipX;
		protected Vector3 mInitialLocalScale;
		protected bool mShouldMove = true;
		protected Health mHealth;

		/// <summary>
		/// 初始化时调用
		/// </summary>
		protected virtual void Awake()
		{
			mFacingRightInitially = ProjectileIsFacingRight;
			mInitialSpeed = Speed;
			mHealth = GetComponent<Health>();
			mCollider2D = GetComponent<Collider2D>();
			mSpriteRenderer = GetComponent<SpriteRenderer>();
			mDamageOnTouch = GetComponent<DamageOnTouch>();
			mRigidBody2D = GetComponent<Rigidbody2D>();
			mInitialInvulnerabilityDurationWfs = new WaitForSeconds(InitialInvulnerabilityDuration);
			if (mSpriteRenderer != null) { mInitialFlipX = mSpriteRenderer.flipX; }
			mInitialLocalScale = transform.localScale;
		}

		/// <summary>
		/// 处理弹药的初始无敌状态
		/// </summary>
		protected virtual IEnumerator InitialInvulnerability()
		{
			if (mDamageOnTouch == null) { yield break; }
			if (mWeapon == null) { yield break; }

			mDamageOnTouch.ClearIgnoreList();
			if (mWeapon.Owner != null)
			{
				mDamageOnTouch.IgnoreGameObject(mWeapon.Owner.gameObject);	
			}
			yield return mInitialInvulnerabilityDurationWfs;
			if (DamageOwner)
			{
				mDamageOnTouch.StopIgnoringObject(mWeapon.Owner.gameObject);
			}
		}

		/// <summary>
		/// 初始化弹药
		/// </summary>
		protected virtual void Initialization()
		{
			Speed = mInitialSpeed;
			ProjectileIsFacingRight = mFacingRightInitially;
			if (mSpriteRenderer != null) { mSpriteRenderer.flipX = mInitialFlipX; }
			transform.localScale = mInitialLocalScale;	
			mShouldMove = true;
			mDamageOnTouch?.InitializeFeedbacks();

			if (mCollider2D != null)
			{
				mCollider2D.enabled = true;
			}
		}

		protected void Update()
		{
			if (mShouldMove)
			{
				Movement();
			}
		}

		/// <summary>
		/// 处理弹药的每帧移动
		/// </summary>
		public virtual void Movement()
		{
			mMovement = Direction * (Speed / 10 * Time.deltaTime);
			if (mRigidBody2D != null)
			{
				mRigidBody2D.MovePosition(this.transform.position + mMovement);
			}
			// 应用加速度以增加速度
			Speed += Acceleration * Time.deltaTime;
		}

		/// <summary>
		/// 设置弹药的方向
		/// </summary>
		/// <param name="newDirection">新方向</param>
		/// <param name="newRotation">新旋转</param>
		/// <param name="spawnerIsFacingRight">如果设置为true，发射器朝右</param>
		public virtual void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight = true)
		{
			if (DirectionCanBeChangedBySpawner)
			{
				Direction = newDirection;
			}
			if (ProjectileIsFacingRight != spawnerIsFacingRight)
			{
				Flip();
			}
			if (FaceDirection)
			{
				transform.rotation = newRotation;
			}

			if (mDamageOnTouch != null)
			{
				mDamageOnTouch.SetKnockbackScriptDirection(newDirection);
			}

			if (FaceMovement)
			{
				switch (MovementVector)
				{
					case MovementVectors.Forward:
						transform.forward = newDirection;
						break;
					case MovementVectors.Right:
						transform.right = newDirection;
						break;
					case MovementVectors.Up:
						transform.up = newDirection;
						break;
				}
			}
		}

		/// <summary>
		/// 翻转弹药
		/// </summary>
		protected virtual void Flip()
		{
			if (mSpriteRenderer != null)
			{
				mSpriteRenderer.flipX = !mSpriteRenderer.flipX;
			}	
			else
			{
				this.transform.localScale = Vector3.Scale(this.transform.localScale, FlipValue);
			}
		}
        
		/// <summary>
		/// 翻转弹药到指定状态
		/// </summary>
		protected virtual void Flip(bool state)
		{
			if (mSpriteRenderer != null)
			{
				mSpriteRenderer.flipX = state;
			}
			else
			{
				this.transform.localScale = Vector3.Scale(this.transform.localScale, FlipValue);
			}
		}

		/// <summary>
		/// 设置弹药的父级武器
		/// </summary>
		public virtual void SetWeapon(Weapon newWeapon)
		{
			mWeapon = newWeapon;
		}

		/// <summary>
		/// 设置弹药的DamageOnTouch组件造成的伤害值
		/// </summary>
		public virtual void SetDamage(float minDamage, float maxDamage)
		{
			if (mDamageOnTouch != null)
			{
				mDamageOnTouch.MinDamageCaused = minDamage;
				mDamageOnTouch.MaxDamageCaused = maxDamage;
			}
		}
        
		/// <summary>
		/// 设置弹药的所有者
		/// </summary>
		public virtual void SetOwner(GameObject newOwner)
		{
			mOwner = newOwner;
			DamageOnTouch damageOnTouch = this.gameObject.GetComponent<DamageOnTouch>();
			if (damageOnTouch != null)
			{
				damageOnTouch.Owner = newOwner;
				if (!DamageOwner)
				{
					damageOnTouch.ClearIgnoreList();
					damageOnTouch.IgnoreGameObject(newOwner);
				}
			}
		}

		/// <summary>
		/// 返回弹药的当前所有者
		/// </summary>
		public virtual GameObject GetOwner() => mOwner;

		/// <summary>
		/// 死亡时，禁用碰撞体并阻止移动
		/// </summary>
		public virtual void StopAt()
		{
			if (mCollider2D != null)
			{
				mCollider2D.enabled = false;
			}
			
			mShouldMove = false;
		}

		/// <summary>
		/// 死亡时停止弹药
		/// </summary>
		protected virtual void OnDeath() => StopAt();

		/// <summary>
		/// 启用时，触发短暂的无敌状态
		/// </summary>
		protected void OnEnable()
		{
			Initialization();
			if (InitialInvulnerabilityDuration > 0)
			{
				StartCoroutine(InitialInvulnerability());
			}

			if (mHealth != null)
			{
				mHealth.OnDeath += OnDeath;
			}
		}

		/// <summary>
		/// 禁用时，将OnDeath方法从健康组件中解除
		/// </summary>
		protected void OnDisable()
		{
			if (mHealth != null)
			{
				mHealth.OnDeath -= OnDeath;
			}			
		}
	}	
}