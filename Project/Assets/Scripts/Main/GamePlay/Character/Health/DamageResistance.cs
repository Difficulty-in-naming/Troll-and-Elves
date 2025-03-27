using System;
using UnityEngine;

namespace EdgeStudio
{
	/// <summary>
	/// 由DamageResistanceProcessor使用，此类定义了对特定类型伤害的抗性。
	/// </summary>
	[AddComponentMenu("Edge Studio/Character/Health/Damage Resistance")]
	public class DamageResistance : TopDownMonoBehaviour
	{
		public enum DamageModifierModes { Multiplier, Flat }
		public enum KnockbackModifierModes { Multiplier, Flat }

		[Header("通用")]
		/// 此伤害抗性的优先级。这将用于确定评估伤害抗性的顺序。最低优先级意味着首先评估。
		[Tooltip("此伤害抗性的优先级。这将用于确定评估伤害抗性的顺序。最低优先级意味着首先评估。")]
		public float Priority = 0;
		/// 此伤害抗性的标签。用于组织，并通过其标签激活/禁用抗性。
		[Tooltip("此伤害抗性的标签。用于组织，并通过其标签激活/禁用抗性。")]
		public string Label = "";
		
		[Header("伤害抗性设置")]
		/// 此抗性是影响基础伤害还是类型伤害
		[Tooltip("此抗性是影响基础伤害还是类型伤害")]
		public DamageTypeModes DamageTypeMode = DamageTypeModes.BaseDamage;
		/// 在TypedDamage模式下，此抗性将与之交互的伤害类型
		[Tooltip("在TypedDamage模式下，此抗性将与之交互的伤害类型")]
		[Sirenix.OdinInspector.ShowIf("DamageTypeMode", (int)DamageTypeModes.TypedDamage)]
		public DamageType TypeResistance;
		/// 减少（或增加）接收伤害的方式。乘数将通过乘数乘以传入伤害，固定值将从传入伤害中减去一个常数值。
		[Tooltip("减少（或增加）接收伤害的方式。乘数将通过乘数乘以传入伤害，固定值将从传入伤害中减去一个常数值。")]
		public DamageModifierModes DamageModifierMode = DamageModifierModes.Multiplier;

		[Header("伤害修饰符")]
		/// 在乘数模式下，应用于传入伤害的乘数。0.5将使其减半，而值为2将创建对指定伤害类型的弱点，伤害将加倍。
		[Tooltip("在乘数模式下，应用于传入伤害的乘数。0.5将使其减半，而值为2将创建对指定伤害类型的弱点，伤害将加倍。")]
		[Sirenix.OdinInspector.ShowIf("DamageModifierMode", (int)DamageModifierModes.Multiplier)]
		public float DamageMultiplier = 0.25f;
		/// 在固定模式下，每次接收该类型伤害时要减去的伤害量
		[Tooltip("在固定模式下，每次接收该类型伤害时要减去的伤害量")]
		[Sirenix.OdinInspector.ShowIf("DamageModifierMode", (int)DamageModifierModes.Flat)]
		public float FlatDamageReduction = 10f;
		/// 指定类型的传入伤害是否应该在最小值和最大值之间限制
		[Tooltip("指定类型的传入伤害是否应该在最小值和最大值之间限制")] 
		public bool ClampDamage = false;
		/// 限制传入伤害的值范围
		[Tooltip("限制传入伤害的值范围")]
		public Vector2 DamageModifierClamps = new Vector2(0f,10f);

		[Header("状态变化")]
		/// 是否允许针对该类型伤害的状态变化
		[Tooltip("是否允许针对该类型伤害的状态变化")]
		public bool PreventCharacterConditionChange = false;
		/// 是否允许针对该类型伤害的移动修饰符
		[Tooltip("是否允许针对该类型伤害的移动修饰符")]
		public bool PreventMovementModifier = false;
		
		[Header("击退")] 
		/// 如果为true，击退力将被忽略，不会被应用
		[Tooltip("如果为true，击退力将被忽略，不会被应用")]
		public bool ImmuneToKnockback = false;
		/// 减少（或增加）接收击退的方式。乘数将通过乘数乘以传入击退强度，固定值将从传入击退强度中减去一个常数值。
		[Tooltip("减少（或增加）接收击退的方式。乘数将通过乘数乘以传入击退强度，固定值将从传入击退强度中减去一个常数值。")]
		public KnockbackModifierModes KnockbackModifierMode = KnockbackModifierModes.Multiplier;
		/// 在乘数模式下，应用于传入击退的乘数。0.5将使其减半，而值为2将创建对指定伤害类型的弱点，击退强度将加倍。
		[Tooltip("在乘数模式下，应用于传入击退的乘数。0.5将使其减半，而值为2将创建对指定伤害类型的弱点，击退强度将加倍。")]
		[Sirenix.OdinInspector.ShowIf("KnockbackModifierMode", (int)DamageModifierModes.Multiplier)]
		public float KnockbackMultiplier = 1f;
		/// 在固定模式下，每次接收该类型伤害时要减去的击退量
		[Tooltip("在固定模式下，每次接收该类型伤害时要减去的击退量")]
		[Sirenix.OdinInspector.ShowIf("KnockbackModifierMode", (int)DamageModifierModes.Flat)]
		public float FlatKnockbackMagnitudeReduction = 10f;
		/// 指定类型的传入击退是否应该在最小值和最大值之间限制
		[Tooltip("指定类型的传入击退是否应该在最小值和最大值之间限制")] 
		public bool ClampKnockback = false;
		/// 限制传入击退幅度的值范围
		[Tooltip("限制传入击退幅度的值范围")]
		[Sirenix.OdinInspector.ShowIf("ClampKnockback", true)]
		public float KnockbackMaxMagnitude = 10f;

		[Header("反馈")]
		/// 当该类型的伤害被中断时，该反馈是否可以被中断（停止）
		[Tooltip("当该类型的伤害被中断时，该反馈是否可以被中断（停止）")]
		public bool InterruptibleFeedback = false;
		/// 如果为true，反馈将始终在播放前预防性地停止
		[Tooltip("如果为true，反馈将始终在播放前预防性地停止")]
		public bool AlwaysInterruptFeedbackBeforePlay = false;
		/// 如果接收到的伤害为零，此反馈是否应该播放
		[Tooltip("如果接收到的伤害为零，此反馈是否应该播放")]
		public bool TriggerFeedbackIfDamageIsZero = false;
		
		/// <summary>
		/// 当受到伤害时，通过伤害减免并输出结果伤害
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="type"></param>
		/// <param name="damageApplied"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public virtual float ProcessDamage(float damage, DamageType type, bool damageApplied)
		{
			if (!this.gameObject.activeInHierarchy)
			{
				return damage;
			}
			
			if ((type == null) && (DamageTypeMode != DamageTypeModes.BaseDamage))
			{
				return damage;
			}

			if ((type != null) && (DamageTypeMode == DamageTypeModes.BaseDamage))
			{
				return damage;
			}

			if ((type != null) && (type != TypeResistance))
			{
				return damage;
			}
			
			// 应用伤害修饰符或减免
			switch (DamageModifierMode)
			{
				case DamageModifierModes.Multiplier:
					damage = damage * DamageMultiplier;
					break;
				case DamageModifierModes.Flat:
					damage = damage - FlatDamageReduction;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			// 限制伤害
			damage = ClampDamage ? Mathf.Clamp(damage, DamageModifierClamps.x, DamageModifierClamps.y) : damage;

			if (damageApplied)
			{
			}

			return damage;
		}
		
		/// <summary>
		/// 处理击退输入值，并返回可能被伤害抗性修改后的结果
		/// </summary>
		/// <param name="knockback"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public virtual Vector2 ProcessKnockback(Vector2 knockback, DamageType type)
		{
			if (!this.gameObject.activeInHierarchy)
			{
				return knockback;
			}

			if ((type == null) && (DamageTypeMode != DamageTypeModes.BaseDamage))
			{
				return knockback;
			}

			if ((type != null) && (DamageTypeMode == DamageTypeModes.BaseDamage))
			{
				return knockback;
			}

			if ((type != null) && (type != TypeResistance))
			{
				return knockback;
			}

			// 应用伤害修饰符或减免
			switch (KnockbackModifierMode)
			{
				case KnockbackModifierModes.Multiplier:
					knockback = knockback * KnockbackMultiplier;
					break;
				case KnockbackModifierModes.Flat:
					float magnitudeReduction = Mathf.Clamp(Mathf.Abs(knockback.magnitude) - FlatKnockbackMagnitudeReduction, 0f, Single.MaxValue);
					knockback = knockback.normalized * magnitudeReduction * Mathf.Sign(knockback.magnitude);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			// 限制伤害
			knockback = ClampKnockback ? Vector2.ClampMagnitude(knockback, KnockbackMaxMagnitude) : knockback;

			return knockback;
		}
	}
}