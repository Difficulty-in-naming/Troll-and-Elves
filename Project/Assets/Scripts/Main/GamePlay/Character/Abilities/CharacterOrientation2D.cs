using EdgeStudio.Odin;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EdgeStudio.Abilities
{
	/// <summary>
	/// Add this ability to a character and it'll rotate or flip to face the direction of movement or the weapon's, or both, or none
	/// Only add this ability to a 2D character
	/// </summary>
	[HideProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Orientation 2D")]
	public class CharacterOrientation2D : CharacterAbility
	{
		public enum FacingModes { None, MovementDirection, Both }

		public FacingModes FacingMode = FacingModes.None;

		[InfoBox("You can also decide if the character must automatically flip when going backwards or not. Additionnally, if you're not using sprites, you can define here how the character's model's localscale will be affected by flipping. By default it flips on the x axis, but you can change that to fit your model.")]
		[Header("Horizontal Flip")]

		[Tooltip("whether we should flip the model's scale when the character changes direction or not	")]
		public bool ModelShouldFlip;
		[ShowIf("ModelShouldFlip")]
		[Tooltip("the scale value to apply to the model when facing left")]
		public Vector2 ModelFlipValueLeft = new(-1, 1);
		[ShowIf("ModelShouldFlip")]
		[Tooltip("the scale value to apply to the model when facing east")]
		public Vector2 ModelFlipValueRight = new(1, 1);
		[Tooltip("whether we should rotate the model on direction change or not")]
		public bool ModelShouldRotate;
		[ShowIf("ModelShouldRotate")]
		[Tooltip("the rotation to apply to the model when it changes direction")]
		public Vector2 ModelRotationValueLeft = new(0f, 180f);
		[ShowIf("ModelShouldRotate")]
		[Tooltip("the rotation to apply to the model when it changes direction")]
		public Vector2 ModelRotationValueRight = new(0f, 0f);
		[ShowIf("ModelShouldRotate")]
		[Tooltip("the speed at which to rotate the model when changing direction, 0f means instant rotation	")]
		public float ModelRotationSpeed;
        
		[Header("Direction")]

		[InfoBox("It's usually good practice to build all your characters facing right. If that's not the case of this character, select West instead.")]
		[Tooltip("true if the player is facing right")]
		public FacingDirections InitialFacingDirection = FacingDirections.Right;
		[Tooltip("the threshold at which movement is considered")]
		public float AbsoluteThresholdMovement = 0.5f;

		[ReadOnly]
		[Tooltip("the direction this character is currently facing")]
		public FacingDirections CurrentFacingDirection = FacingDirections.Right;

		protected Vector2 mTargetModelRotation;
		protected float mLastNonNullXMovement;        
		protected float mLastNonNullXInput;
		protected int mDirection;
		protected int mDirectionLastFrame;
		protected float mHorizontalDirection;
		protected float mVerticalDirection;
		
		protected float mLastDirectionX;
		protected float mLastDirectionY;
		protected bool mInitialized;

		/// <summary>
		/// On awake we init our facing direction and grab components
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			this.gameObject.GetComponentInParent<CharacterHandleWeapon>();
		}

		/// <summary>
		/// On Start we reset our CurrentDirection
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			mController.CurrentDirection = Vector2.zero;
			mInitialized = true;
			if (InitialFacingDirection == FacingDirections.Left)
			{
				mDirection = -1;
			}
			else
			{
				mDirection = 1;
			}
			Face(InitialFacingDirection);
			mDirectionLastFrame = 0;
			CurrentFacingDirection = InitialFacingDirection;
			switch(InitialFacingDirection)
			{
				case FacingDirections.Right:
					mLastDirectionX = 1f;
					mLastDirectionY = 0f;
					break;
				case FacingDirections.Left:
					mLastDirectionX = -1f;
					mLastDirectionY = 0f;
					break;
			}
		}

		/// <summary>
		/// On process ability, we flip to face the direction set in settings
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();

			if (mCondition.CurrentState != CharacterStates.CharacterConditions.Normal)
			{
				return;
			}

			if (!AbilityAuthorized)
			{
				return;
			}

			DetermineFacingDirection();
			FlipToFaceMovementDirection();
			ApplyModelRotation();
			FlipAbilities();

			mDirectionLastFrame = mDirection;
			mLastNonNullXMovement = (Mathf.Abs(mController.CurrentDirection.x) > 0) ? mController.CurrentDirection.x : mLastNonNullXMovement;
			mLastNonNullXInput = (Mathf.Abs(InputManager.Inst.PrimaryMovementInput.x) > InputManager.Inst.Threshold.x) ? InputManager.Inst.PrimaryMovementInput.x : mLastNonNullXInput;
		}

		protected virtual void DetermineFacingDirection()
		{
			if (mController.CurrentDirection == Vector2.zero)
			{
				ApplyCurrentDirection();
			}

			if (mController.CurrentDirection.normalized.magnitude >= AbsoluteThresholdMovement)
			{
				if (!(Mathf.Abs(mController.CurrentDirection.y) > Mathf.Abs(mController.CurrentDirection.x)))
				{
					CurrentFacingDirection = (mController.CurrentDirection.x > 0) ? FacingDirections.Right : FacingDirections.Left;
				}

				mHorizontalDirection = Mathf.Abs(mController.CurrentDirection.x) >= AbsoluteThresholdMovement ? mController.CurrentDirection.x : 0f;
				mVerticalDirection = Mathf.Abs(mController.CurrentDirection.y) >= AbsoluteThresholdMovement ? mController.CurrentDirection.y : 0f;	
			}
			else
			{
				mHorizontalDirection = mLastDirectionX;
				mVerticalDirection = mLastDirectionY;
			}
            
			switch (CurrentFacingDirection)
			{
				case FacingDirections.Left:
					break;
				case FacingDirections.Right:
					break;
			}
            
			mLastDirectionX = mHorizontalDirection;
			mLastDirectionY = mVerticalDirection;
		}

		/// <summary>
		/// Applies the current direction to the controller
		/// </summary>
		protected virtual void ApplyCurrentDirection()
		{
			if (!mInitialized)
			{
				Initialization();
			}

			mController.CurrentDirection = CurrentFacingDirection switch
			{
				FacingDirections.Right => Vector2.right,
				FacingDirections.Left => Vector2.left,
				_ => mController.CurrentDirection
			};
		}
        
		/// <summary>
		/// If the model should rotate, we modify its rotation 
		/// </summary>
		protected virtual void ApplyModelRotation()
		{
			if (!ModelShouldRotate) return;
			Character.CharacterModel.transform.localEulerAngles = ModelRotationSpeed > 0f ? Vector2.Lerp(Character.CharacterModel.transform.localEulerAngles, mTargetModelRotation, Time.deltaTime * ModelRotationSpeed) : mTargetModelRotation;
		}

		/// <summary>
		/// Flips the object to face direction
		/// </summary>
		protected virtual void FlipToFaceMovementDirection()
		{
			// if we're not supposed to face our direction, we do nothing and exit
			if (FacingMode != FacingModes.MovementDirection && FacingMode != FacingModes.Both) { return; }
            
			if (mController.CurrentDirection.normalized.magnitude >= AbsoluteThresholdMovement)
			{
				float xThreshold = 0.1f; // 可以根据需要调整这个值
        
				// 只有当x分量足够大时才考虑它
				if (Mathf.Abs(mController.CurrentDirection.normalized.x) > xThreshold)
				{
					if (mController.CurrentDirection.normalized.x > 0)
					{
						FaceDirection(1);
					}
					else
					{
						FaceDirection(-1);
					}
				}
			}                
		}

		/// <summary>
		/// Defines the CurrentFacingDirection
		/// </summary>
		/// <param name="direction"></param>
		public virtual void Face(FacingDirections direction)
		{
			CurrentFacingDirection = direction;
			ApplyCurrentDirection();
			if (direction == FacingDirections.Left)
			{
				FaceDirection(-1);
			}
			if (direction == FacingDirections.Right)
			{
				FaceDirection(1);
			}
		}

		/// <summary>
		/// Flips the character and its dependencies horizontally
		/// </summary>
		public virtual void FaceDirection(int direction)
		{
			if (ModelShouldFlip)
			{
				FlipModel(direction);
			}

			if (ModelShouldRotate)
			{
				RotateModel(direction);
			}

			mDirection = direction;
		}

		/// <summary>
		/// Rotates the model in the specified direction
		/// </summary>
		/// <param name="direction"></param>
		protected virtual void RotateModel(int direction)
		{
			if (Character.CharacterModel != null)
			{
				mTargetModelRotation = direction == 1 ? ModelRotationValueRight : ModelRotationValueLeft;
				mTargetModelRotation.x %= 360;
				mTargetModelRotation.y %= 360;
			}
		}
        
		/// <summary>
		/// Flips the model only, no impact on weapons or attachments
		/// </summary>
		public virtual void FlipModel(int direction)
		{
			if (Character.CharacterModel != null)
			{
				Character.CharacterModel.transform.localScale = (direction == 1) ? ModelFlipValueRight : ModelFlipValueLeft;
			}
		}

		/// <summary>
		/// Sends a flip event on all other abilities
		/// </summary>
		protected virtual void FlipAbilities()
		{
			if ((mDirectionLastFrame != 0) && (mDirectionLastFrame != mDirection))
			{
				Character.FlipAllAbilities();
			}
		}
	}
}