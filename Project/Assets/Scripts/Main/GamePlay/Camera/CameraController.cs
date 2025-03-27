using Com.LuisPedroFonseca.ProCamera2D;
using Cysharp.Threading.Tasks;
using EdgeStudio.R3;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EdgeStudio
{
	/// <summary>
	/// 处理Cinemachine驱动的摄像机跟随逻辑的类
	/// </summary>
	public sealed class CameraController : TopDownMonoBehaviour
	{
		/// <summary>
		/// 获取或设置是否跟随玩家
		/// </summary>
		public bool FollowsPlayer { get; set; }
    
		[Tooltip("摄像机是否应该跟随玩家")]
		public bool FollowsAPlayer = true;
    
		[Tooltip("是否将摄像机限制在关卡边界内（由LevelManager定义）")]
		public bool ConfineCameraToLevelBounds = true;
    
		[Tooltip("如果启用，该限制器将监听设置边界事件")]
		public bool ListenToSetConfinerEvents = true;
    
		[ReadOnly]
		[Tooltip("摄像机需要跟随的目标角色")]
		public Character TargetCharacter;

		private ProCamera2D mCamera;
		private ProCamera2DShake mShake;

		private int mLastStopFollow = -1;

		private void Awake()
		{
			mCamera = GetComponent<ProCamera2D>();
			mShake = GetComponent<ProCamera2DShake>();
		}

		public void SetTarget(Character character) => TargetCharacter = character;

		public void StartFollowing() => StartFollowingAsync().Forget();

		private async UniTask StartFollowingAsync()
		{
			if (mLastStopFollow > 0 && mLastStopFollow == Time.frameCount)
			{
				await UniTask.NextFrame();
			}
			if (!FollowsAPlayer) { return; }
			FollowsPlayer = true;
			mCamera.AddCameraTarget(TargetCharacter.CameraTarget.transform);
			mCamera.enabled = true;
		}
		
		public void StopFollowing()
		{
			if (!FollowsAPlayer) { return; }
			FollowsPlayer = false;
			mCamera.RemoveAllCameraTargets();
			mCamera.enabled = false;
			mLastStopFollow = Time.frameCount;
		}

		public void OnMMEvent(CameraEvent cameraEvent)
		{
			switch (cameraEvent.EventType)
			{
				case CameraEventTypes.SetTargetCharacter:
					SetTarget(cameraEvent.TargetCharacter);
					break;
				case CameraEventTypes.StartFollowing:
					if (cameraEvent.TargetCharacter != null)
					{
						if (cameraEvent.TargetCharacter != TargetCharacter)
						{
							return;
						}
					}
					StartFollowing();
					break;

				case CameraEventTypes.StopFollowing:
					if (cameraEvent.TargetCharacter != null)
					{
						if (cameraEvent.TargetCharacter != TargetCharacter)
						{
							return;
						}
					}
					StopFollowing();
					break;

				case CameraEventTypes.RefreshPosition:
					RefreshPosition().Forget();
					break;
			}
		}

		private async UniTask RefreshPosition()
		{
			mCamera.enabled = false;
			await UniTask.NextFrame();
			StartFollowing();
		}

		public void OnMMEvent(TopDownEngineEvent topdownEngineEvent)
		{
			if (topdownEngineEvent.EventType == TopDownEngineEventTypes.CharacterSwitch)
			{
				SetTarget(GameObject.FindWithTag("Player").GetComponent<Character>());
				StartFollowing();
			}

			if (topdownEngineEvent.EventType == TopDownEngineEventTypes.CharacterSwap)
			{
				SetTarget(GameObject.FindWithTag("Player").GetComponent<Character>());
				CameraEvent.Trigger(CameraEventTypes.RefreshPosition);
			}
		}
		

		private void OnEnable()
		{
			CameraEvent.Event.Subscribe(OnMMEvent).AddToDisable(CachedGameObject);
			TopDownEngineEvent.Event.Subscribe(OnMMEvent).AddToDisable(CachedGameObject);
		}
	}
}