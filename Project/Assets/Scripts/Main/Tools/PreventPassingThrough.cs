using UnityEngine;
using UnityEngine.Serialization;

namespace EdgeStudio
{
	/// <summary>
	/// 通过在每次移动后向后投射射线来防止快速移动的物体穿过碰撞体
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Movement/Prevent Passing Through 2D"),DefaultExecutionOrder(1000)]
	public class PreventPassingThrough2D : MonoBehaviour 
	{
		public enum Modes { Raycast, BoxCast }
		/// 是否使用射线或盒型射线来检测目标
		public Modes Mode = Modes.Raycast;	
		/// 用于搜索障碍物的层级遮罩
		public LayerMask ObstaclesLayerMask; 
		/// 边界调整变量
		public float SkinWidth = 0.1f;
		/// 是否在碰到触发器碰撞体时重新定位刚体
		public bool RepositionRigidbodyIfHitTrigger = true;
		/// 是否在碰到非触发器碰撞体时重新定位刚体
		[FormerlySerializedAs("RepositionRigidbody")] 
		public bool RepositionRigidbodyIfHitNonTrigger = true;

		public RaycastHit2D Hit { get; set; }

		protected float mSmallestBoundsWidth; // 最小边界宽度
		protected float mAdjustedSmallestBoundsWidth; // 调整后的最小边界宽度
		protected float mSquaredBoundsWidth; // 边界宽度的平方
		protected Vector3 mPositionLastFrame; // 上一帧的位置
		protected Rigidbody2D mRigidbody;
		protected Collider2D mCollider;
		protected Vector2 mLastMovement; // 最后一次移动
		protected float mLastMovementSquared; // 最后一次移动距离的平方
		protected RaycastHit2D mHitInfo;
		protected Vector2 mColliderSize; // 碰撞体尺寸

		/// <summary>
		/// 在Start中初始化对象
		/// </summary>
		protected virtual void Start() 
		{ 
			Initialization ();
		} 

		/// <summary>
		/// 获取刚体组件并计算边界宽度
		/// </summary>
		protected virtual void Initialization()
		{
			mRigidbody = GetComponent<Rigidbody2D>();
			mPositionLastFrame = mRigidbody.position; 

			mCollider = GetComponent<Collider2D>();
			if (mCollider is BoxCollider2D instance)
			{
				mColliderSize = instance.size;
			}
			
			mSmallestBoundsWidth = Mathf.Min(Mathf.Min(mCollider.bounds.extents.x, mCollider.bounds.extents.y), mCollider.bounds.extents.z); 
			mAdjustedSmallestBoundsWidth = mSmallestBoundsWidth * (1.0f - SkinWidth); 
			mSquaredBoundsWidth = mSmallestBoundsWidth * mSmallestBoundsWidth; 
		}

		/// <summary>
		/// 在Enable时初始化上一帧的位置
		/// </summary>
		protected virtual void OnEnable()
		{
			mPositionLastFrame = this.transform.position;
		}

		/// <summary>
		/// 在fixedUpdate中检查最后的移动,如果需要则投射射线来检测障碍物
		/// </summary>
		protected virtual void Update() 
		{ 
			mLastMovement = this.transform.position - mPositionLastFrame; 
			mLastMovementSquared = mLastMovement.sqrMagnitude;

			// 如果我们移动的距离超过了边界,可能错过了一些碰撞
			if (mLastMovementSquared > mSquaredBoundsWidth) 
			{ 
				float movementMagnitude = Mathf.Sqrt(mLastMovementSquared);

				// 我们向后投射射线来检查是否应该碰到什么东西
				if (Mode == Modes.Raycast)
				{
					mHitInfo = Physics2D.Raycast(mPositionLastFrame, mLastMovement.normalized, movementMagnitude, ObstaclesLayerMask);	
				}
				else
				{
					mHitInfo = Physics2D.BoxCast(origin: mPositionLastFrame,
						size: mColliderSize,
						angle: 0,
						layerMask: ObstaclesLayerMask,
						direction: mLastMovement.normalized,
						distance: movementMagnitude);
				}

				if (mHitInfo.collider != null)
				{
					if (mHitInfo.collider.isTrigger) 
					{
						mHitInfo.collider.SendMessage("OnTriggerEnter2D", mCollider, SendMessageOptions.DontRequireReceiver);
						if (RepositionRigidbodyIfHitTrigger)
						{
							this.transform.position = mHitInfo.point - (mLastMovement / movementMagnitude) * mAdjustedSmallestBoundsWidth;
							mRigidbody.position = mHitInfo.point - (mLastMovement / movementMagnitude) * mAdjustedSmallestBoundsWidth;
						}   
					}						

					if (!mHitInfo.collider.isTrigger)
					{
						Hit = mHitInfo;
						this.gameObject.SendMessage("PreventedCollision2D", Hit, SendMessageOptions.DontRequireReceiver);
						if (RepositionRigidbodyIfHitNonTrigger)
						{
							this.transform.position = mHitInfo.point - (mLastMovement / movementMagnitude) * mAdjustedSmallestBoundsWidth;
							mRigidbody.position = mHitInfo.point - (mLastMovement / movementMagnitude) * mAdjustedSmallestBoundsWidth;
						}                        
					}
				}
			} 
			mPositionLastFrame = this.transform.position; 
		}
	}
}