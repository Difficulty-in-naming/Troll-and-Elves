using EdgeStudio.Environments;
using EdgeStudio.Odin;
using EdgeStudio.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EdgeStudio.Abilities
{
	/// <summary>
	/// 将此组件添加到角色上，使其能够激活按钮区域
	/// </summary>
	[HideProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Button Activation")] 
	[InfoBox("此组件允许您的角色与按钮驱动的对象（对话区域、开关等）进行交互。")]
	public class CharacterButtonActivation : CharacterAbility 
	{
		public bool InButtonActivatedZone {get;set;}
		public virtual bool InButtonAutoActivatedZone { get; set; }

		[ColorFoldout("按钮激活"), Tooltip("当前角色所在的按钮激活区域"), ReadOnly]
		public ButtonActivated ButtonActivatedZone;

		/// <summary>
		/// 获取并存储组件以供后续使用
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			InButtonActivatedZone = false;
			ButtonActivatedZone = null;
			InButtonAutoActivatedZone = false;
		}

		/// <summary>
		/// 每帧检查输入以确定是否需要暂停/恢复游戏
		/// </summary>
		protected override void HandleInput()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			if (InButtonActivatedZone && ButtonActivatedZone != null)
			{
				if (ButtonActivatedZone.InputActionPerformed)
				{
					ButtonActivation();
				}
			}
		}
        
		/// <summary>
		/// 尝试激活按钮激活区域
		/// </summary>
		protected virtual void ButtonActivation()
		{
			// 如果玩家在按钮激活区域内，我们处理它
			if (InButtonActivatedZone && ButtonActivatedZone != null
			                          && mCondition.CurrentState is CharacterStates.CharacterConditions.Normal or CharacterStates.CharacterConditions.Frozen
			                          && mMovement.CurrentState != CharacterStates.MovementStates.Dashing)
			{
				// 如果是自动激活区域，则不执行任何操作
				if (ButtonActivatedZone.AutoActivation) return;

				ButtonActivatedZone.TriggerButtonAction();
			}
		}

		/// <summary>	
		/// 死亡时失去与任何按钮激活区域的连接	
		/// </summary>	
		protected override void OnDeath()
		{
			base.OnDeath();
			ResetFlags();
		}

		/// <summary>
		/// 启用时确保重置标志
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			ResetFlags();
		}

		/// <summary>
		/// 重置区域标志
		/// </summary>
		protected virtual void ResetFlags()
		{
			InButtonActivatedZone = false;
			ButtonActivatedZone = null;
			InButtonAutoActivatedZone = false;
		}
	}
}