using EdgeStudio.Odin;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EdgeStudio
{	
	/// <summary> 用于在俯视图中移动Rigidbody2D和Collider2D的控制器 </summary>
	public class TopDownController : TopDownMonoBehaviour 
	{
		[ColorFoldout("运动状态"), ReadOnly, Tooltip("角色当前的移动")] public Vector2 CurrentMovement;

		[ColorFoldout("运动状态"), ReadOnly, Tooltip("角色前进的方向")] public Vector2 CurrentDirection;

		public Vector2 ColliderSize
		{
			get => mBoxCollider.size;
			set => mBoxCollider.size = value;
		}

		public Vector2 ColliderOffset
		{
			get => mBoxCollider.offset;
			set => mBoxCollider.offset = value;
		}

		protected Rigidbody2D mRigidBody;
		protected BoxCollider2D mBoxCollider;
		protected Vector2 mOriginalColliderSize;
		protected Vector2 mOriginalColliderCenter;

		protected void Awake()
		{			
			mRigidBody = GetComponent<Rigidbody2D>();
			mBoxCollider = GetComponent<BoxCollider2D>();
			mOriginalColliderSize = ColliderSize;
			mOriginalColliderCenter = ColliderOffset;
			CurrentDirection = CachedTransform.forward;
		}

		private void Update() => DetermineDirection();

		private void FixedUpdate() => mRigidBody.MovePosition(mRigidBody.position + CurrentMovement * Time.fixedDeltaTime);

		/// <summary> 确定控制器当前方向 </summary>
		private void DetermineDirection()
		{
			if (CurrentMovement != Vector2.zero)
				CurrentDirection = CurrentMovement.normalized;
		}
        
		/// <summary> 将控制器的当前移动设置为指定的Vector2 </summary>
		public void SetMovement(Vector2 movement) => CurrentMovement = movement;

		/// <summary> 将控制器移动到指定位置（世界坐标系） </summary>
		public void MovePosition(Vector2 newPosition, bool targetTransform = false)
		{
			if (targetTransform)
				CachedTransform.position = newPosition;
			else
				mRigidBody.MovePosition(newPosition);
		}
		
		/// <summary> 重置控制器的碰撞体尺寸 </summary>
		public void ResetColliderSize()
		{
			ColliderSize = mOriginalColliderSize;
			ColliderOffset = mOriginalColliderCenter;
		}

		/// <summary> 启用碰撞体 </summary>
		public void CollisionsOn() => mBoxCollider.enabled = true;

		/// <summary> 禁用碰撞体 </summary>
		public void CollisionsOff() => mBoxCollider.enabled = false;

		/// <summary> 重置控制器的所有值 </summary>
		public void Reset()
		{
			CurrentMovement = Vector2.zero;
			if (mRigidBody != null) mRigidBody.linearVelocity = Vector2.zero;
		}
	}
}
