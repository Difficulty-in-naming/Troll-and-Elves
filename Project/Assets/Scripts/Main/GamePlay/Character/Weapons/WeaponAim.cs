using EdgeStudio.Odin;
using EdgeStudio.R3;
using MoreMountains.TopDownEngine;
using R3;
using UnityEngine;

namespace EdgeStudio.Weapons
{
	[RequireComponent(typeof(Weapon))]
	public class WeaponAim : TopDownMonoBehaviour
	{
		[ColorFoldout("Weapon Rotation"), Tooltip("武器达到新位置的速度。如果您希望移动直接跟随输入，请将其设置为零")]
		public float WeaponRotationSpeed = 1f;

		[ColorFoldout("Weapon Rotation"), Range(-180, 180), Tooltip("武器旋转将被限制的最小角度")] 
		public float MinimumAngle = -180f;

		[ColorFoldout("Weapon Rotation"), Range(-180, 180), Tooltip("武器旋转将被限制的最大角度")] 
		public float MaximumAngle = 180f;

		[ColorFoldout("Weapon Rotation"), Tooltip("武器旋转幅度将被考虑的最小阈值")] public float MinimumMagnitude = 0.2f;

		protected Vector2 mInputMovement;
		public virtual float CurrentAngleAbsolute { get; protected set; }

		public Vector3 CurrentAim;

		public virtual float CurrentAngle { get; protected set; }


		protected Camera mMainCamera;
		protected Vector2 mLastNonNullMovement;
		protected Weapon mWeapon;


		protected Vector3 mCurrentAimAbsolute = Vector3.zero;
		protected Quaternion mLookRotation;
		protected Vector3 mDirection;
		protected float[] mPossibleAngleValues;
		protected Vector2 mMousePosition;
		protected float mAdditionalAngle;
		protected Quaternion mInitialRotation;
		protected bool mInitialized;

		protected virtual void Start() => Initialization();

		/// <summary>
		/// 获取武器组件，初始化角度值
		/// </summary>
		protected virtual void Initialization()
		{
			if (mInitialized)
			{
				return;
			}

			mWeapon = GetComponent<Weapon>();
			mMainCamera = Camera.main;
			mInitialRotation = CachedTransform.rotation;
			mInitialized = true;
		}

		public virtual void ApplyAim()
		{
			Initialization();
		}

		/// <summary>
		/// 在LateUpdate时，重置任何额外的角度
		/// </summary>
		protected virtual void LateUpdate()
		{
			ResetAdditionalAngle();
		}

		/// <summary>
		/// 旋转武器，可选地应用插值。
		/// </summary>
		/// <param name="newRotation">新的旋转</param>
		/// <param name="forceInstant"></param>
		protected virtual void RotateWeapon(Quaternion newRotation, bool forceInstant = false)
		{
			// 如果旋转速度 == 0，则立即旋转
			if (WeaponRotationSpeed == 0f || forceInstant)
			{
				CachedTransform.rotation = newRotation;
			}
			// 否则我们插值旋转
			else
			{
				CachedTransform.rotation = Quaternion.Slerp(CachedTransform.rotation, newRotation, WeaponRotationSpeed * Time.deltaTime);
			}
		}

		public virtual void AimAt(Vector3 target)
		{
			var aimAtDirection = target - CachedTransform.position;
			aimAtDirection.Normalize();
			mCurrentAimAbsolute = aimAtDirection;
			CurrentAim = mCurrentAimAbsolute;
		}
		
		/// <summary>
		/// 向武器的旋转添加额外的角度
		/// </summary>
		/// <param name="addedAngle"></param>
		public virtual void AddAdditionalAngle(float addedAngle) => mAdditionalAngle += addedAngle;

		/// <summary>
		/// 重置额外的角度
		/// </summary>
		protected virtual void ResetAdditionalAngle() => mAdditionalAngle = 0;

		public void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.LevelStart:
					mInitialized = false;
					Initialization();
					break;
			}
		}

		protected virtual void OnEnable() => TopDownEngineEvent.Event.Subscribe(OnMMEvent).AddToDisable(CachedGameObject);

		void Update()
		{
		}
	}
}