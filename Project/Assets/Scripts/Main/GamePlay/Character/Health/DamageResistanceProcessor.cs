using System.Collections.Generic;
using System.Linq;
using EdgeStudio.Damage;
using UnityEngine;
using UnityEngine.Serialization;

namespace EdgeStudio
{
	/// <summary>
	/// 将此组件链接到Health组件，它将能够通过抗性处理传入的伤害，处理伤害减少/增加、状态变化、移动乘数、反馈等。
	/// </summary>
	[AddComponentMenu("Edge Studio/Character/Health/Damage Resistance Processor")]
	public class DamageResistanceProcessor : TopDownMonoBehaviour
	{
		[Header("伤害抗性列表")]
		
		/// 如果为true，此组件将尝试从其子对象中找到的伤害抗性自动填充其伤害抗性列表
		[Tooltip("如果为true，此组件将尝试从其子对象中找到的伤害抗性自动填充其伤害抗性列表")]
		public bool AutoFillDamageResistanceList = true;
		/// 如果为true，自动填充将忽略禁用的抗性
		[Tooltip("如果为true，自动填充将忽略禁用的抗性")]
		public bool IgnoreDisabledResistances = true;
		/// 如果为true，将忽略此处理器没有抗性的伤害类型造成的伤害
		[Tooltip("如果为true，将忽略此处理器没有抗性的伤害类型造成的伤害")]
		public bool IgnoreUnknownDamageTypes = false;
		
		/// 此处理器将处理的伤害抗性列表。如果AutoFillDamageResistanceList为true，则自动填充
		[FormerlySerializedAs("DamageResitanceList")] 
		[Tooltip("此处理器将处理的伤害抗性列表。如果AutoFillDamageResistanceList为true，则自动填充")]
		public List<DamageResistance> DamageResistanceList;

		/// <summary>
		/// 在唤醒时初始化我们的处理器
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// 如果需要，自动查找抗性并对它们进行排序
		/// </summary>
		protected virtual void Initialization()
		{
			if (AutoFillDamageResistanceList)
			{
				DamageResistance[] foundResistances =
					this.gameObject.GetComponentsInChildren<DamageResistance>(
						includeInactive: !IgnoreDisabledResistances);
				if (foundResistances.Length > 0)
				{
					DamageResistanceList = foundResistances.ToList();	
				}
			}
			SortDamageResistanceList();
		}

		/// <summary>
		/// 用于重新排序抗性列表的方法，默认基于优先级。
		/// 如果您希望以不同的顺序处理抗性，请随时重写此方法
		/// </summary>
		public virtual void SortDamageResistanceList()
		{
			// 我们按优先级对列表进行排序
			DamageResistanceList.Sort((p1,p2)=>p1.Priority.CompareTo(p2.Priority));
		}
		
		/// <summary>
		/// 通过抗性列表处理传入的伤害，并输出最终伤害值
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="typedDamages"></param>
		/// <param name="damageApplied"></param>
		/// <returns></returns>
		public virtual float ProcessDamage(float damage, List<TypedDamage> typedDamages, bool damageApplied)
		{
			float totalDamage = 0f;
			if (DamageResistanceList.Count == 0) // 如果我们没有抗性，我们输出原始伤害
			{
				totalDamage = damage;
				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						totalDamage += typedDamage.DamageCaused;
					}
				}
				if (IgnoreUnknownDamageTypes)
				{
					totalDamage = damage;
				}
				return totalDamage;
			}
			else // 如果我们有抗性
			{
				totalDamage = damage;
				
				foreach (DamageResistance resistance in DamageResistanceList)
				{
					totalDamage = resistance.ProcessDamage(totalDamage, null, damageApplied);
				}

				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						float currentDamage = typedDamage.DamageCaused;
						
						bool atLeastOneResistanceFound = false;
						foreach (DamageResistance resistance in DamageResistanceList)
						{
							if (resistance.TypeResistance == typedDamage.AssociatedDamageType)
							{
								atLeastOneResistanceFound = true;
							}
							currentDamage = resistance.ProcessDamage(currentDamage, typedDamage.AssociatedDamageType, damageApplied);
						}
						if (IgnoreUnknownDamageTypes && !atLeastOneResistanceFound)
						{
							// 我们不添加到总数
						}
						else
						{
							totalDamage += currentDamage;	
						}
						
					}
				}
				
				return totalDamage;
			}
		}

		public virtual void SetResistanceByLabel(string searchedLabel, bool active)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if (resistance.Label == searchedLabel)
				{
					resistance.gameObject.SetActive(active);
				}
			}
		}

		/// <summary>
		/// 当中断指定类型的所有持续伤害时，如果需要，停止它们相关的反馈
		/// </summary>
		/// <param name="damageType"></param>
		public virtual void InterruptDamageOverTime(DamageType damageType)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if ( resistance.gameObject.activeInHierarchy &&
					((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) ||
				        (resistance.TypeResistance == damageType))
				    && resistance.InterruptibleFeedback)
				{
				}
			}
		}

		/// <summary>
		/// 检查是否有任何抗性阻止角色改变状态，如果是这种情况，则返回true，否则返回false
		/// </summary>
		/// <param name="typedDamage"></param>
		/// <returns></returns>
		public virtual bool CheckPreventCharacterConditionChange(DamageType typedDamage)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if (!resistance.gameObject.activeInHierarchy)
				{
					continue;
				}
				
				if (typedDamage == null)
				{
					if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
					    (resistance.PreventCharacterConditionChange))
					{
						return true;	
					}
				}
				else
				{
					if ((resistance.TypeResistance == typedDamage) &&
					    (resistance.PreventCharacterConditionChange))
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// 检查是否有任何抗性阻止角色改变状态，如果是这种情况，则返回true，否则返回false
		/// </summary>
		/// <param name="typedDamage"></param>
		/// <returns></returns>
		public virtual bool CheckPreventMovementModifier(DamageType typedDamage)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if (!resistance.gameObject.activeInHierarchy)
				{
					continue;
				}
				if (typedDamage == null)
				{
					if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
					    (resistance.PreventMovementModifier))
					{
						return true;	
					}
				}
				else
				{
					if ((resistance.TypeResistance == typedDamage) &&
					    (resistance.PreventMovementModifier))
					{
						return true;
					}
				}
			}
			return false;
		}
		
		/// <summary>
		/// 如果此处理器上的抗性使其免疫击退，则返回true，否则返回false
		/// </summary>
		/// <param name="typedDamage"></param>
		/// <returns></returns>
		public virtual bool CheckPreventKnockback(List<TypedDamage> typedDamages)
		{
			if ((typedDamages == null) || (typedDamages.Count == 0))
			{
				foreach (DamageResistance resistance in DamageResistanceList)
				{
					if (!resistance.gameObject.activeInHierarchy)
					{
						continue;
					}

					if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
					    (resistance.ImmuneToKnockback))
					{
						return true;	
					}
				}
			}
			else
			{
				foreach (TypedDamage typedDamage in typedDamages)
				{
					foreach (DamageResistance resistance in DamageResistanceList)
					{
						if (!resistance.gameObject.activeInHierarchy)
						{
							continue;
						}

						if (typedDamage == null)
						{
							if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
							    (resistance.ImmuneToKnockback))
							{
								return true;	
							}
						}
						else
						{
							if ((resistance.TypeResistance == typedDamage.AssociatedDamageType) &&
							    (resistance.ImmuneToKnockback))
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		/// 通过各种抗性处理输入的击退力
		/// </summary>
		/// <param name="knockback"></param>
		/// <param name="typedDamages"></param>
		/// <returns></returns>
		public virtual Vector2 ProcessKnockbackForce(Vector2 knockback, List<TypedDamage> typedDamages)
		{
			if (DamageResistanceList.Count == 0) // 如果我们没有抗性，我们输出原始击退值
			{
				return knockback;
			}
			else // 如果我们有抗性
			{
				foreach (DamageResistance resistance in DamageResistanceList)
				{
					knockback = resistance.ProcessKnockback(knockback, null);
				}

				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						foreach (DamageResistance resistance in DamageResistanceList)
						{
							if (IgnoreDisabledResistances && !resistance.isActiveAndEnabled)
							{
								continue;
							}
							knockback = resistance.ProcessKnockback(knockback, typedDamage.AssociatedDamageType);
						}
					}
				}

				return knockback;
			}
		}
	}
}