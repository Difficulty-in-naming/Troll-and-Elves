using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EdgeStudio.Damage
{
   /// <summary>
   /// 用于存储和定义类型化伤害影响的类：造成的伤害、状态或移动速度变化等
   /// </summary>
   [Serializable]
   public class TypedDamage
   {
      /// 与此定义相关联的伤害类型
      [Tooltip("与此定义相关联的伤害类型")]
      public DamageType AssociatedDamageType;

      /// 从玩家健康值中移除的最小伤害量
      [Tooltip("从玩家健康值中移除的最小伤害量")]
      public float MinDamageCaused = 10f;

      /// 从玩家健康值中移除的最大伤害量
      [Tooltip("从玩家健康值中移除的最大伤害量")]
      public float MaxDamageCaused = 10f;

      /// 此伤害在应用时是否应强制角色进入指定状态
      [Tooltip("此伤害在应用时是否应强制角色进入指定状态")]
      public bool ForceCharacterCondition = false;

      /// 在强制角色状态模式下，要切换到的状态
      [Tooltip("在强制角色状态模式下，要切换到的状态")] [Sirenix.OdinInspector.ShowIf("ForceCharacterCondition", true)]
      public CharacterStates.CharacterConditions ForcedCondition;

      /// 在强制角色状态模式下，是否禁用重力
      [Tooltip("在强制角色状态模式下，是否禁用重力")] [Sirenix.OdinInspector.ShowIf("ForceCharacterCondition", true)]
      public bool DisableGravity = false;

      /// 在强制角色状态模式下，是否重置控制器力量
      [Tooltip("在强制角色状态模式下，是否重置控制器力量")] [Sirenix.OdinInspector.ShowIf("ForceCharacterCondition", true)]
      public bool ResetControllerForces = false;

      /// 在强制角色状态模式下，效果的持续时间，之后状态将被恢复
      [Tooltip("在强制角色状态模式下，效果的持续时间，之后状态将被恢复")]
      [Sirenix.OdinInspector.ShowIf("ForceCharacterCondition", true)]
      public float ForcedConditionDuration = 3f;

      /// 是否对受伤角色应用移动乘数
      [Tooltip("是否对受伤角色应用移动乘数")]
      public bool ApplyMovementMultiplier = false;

      /// 当ApplyMovementMultiplier为true时应用的移动乘数
      [Tooltip("当ApplyMovementMultiplier为true时应用的移动乘数")] [Sirenix.OdinInspector.ShowIf("ApplyMovementMultiplier", true)]
      public float MovementMultiplier = 0.5f;

      /// 如果ApplyMovementMultiplier为true，移动乘数的持续时间
      [Tooltip("如果ApplyMovementMultiplier为true，移动乘数的持续时间")] [Sirenix.OdinInspector.ShowIf("ApplyMovementMultiplier", true)]
      public float MovementMultiplierDuration = 2f;



      protected int _lastRandomFrame = -1000;
      protected float _lastRandomValue = 0f;

      public virtual float DamageCaused
      {
         get
         {
            if (Time.frameCount != _lastRandomFrame)
            {
               _lastRandomValue = Random.Range(MinDamageCaused, MaxDamageCaused);
               _lastRandomFrame = Time.frameCount;
            }

            return _lastRandomValue;
         }
      }
   }
}