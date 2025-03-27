using System.Collections.Generic;
using EdgeStudio.Damage;
using EdgeStudio.Event;
using UnityEngine;
namespace EdgeStudio
{
    // 生命周期事件
    [GameEvent]
    public partial struct LifeCycleEvent
    {
        public Health AffectedHealth; // 受影响的健康组件
        public LifeCycleEventTypes LifeCycleEventType; // 生命周期事件类型
    }
    
    /// <summary>
    /// 当某物受到伤害时触发的事件
    /// </summary>
    [GameEvent]
    public partial struct DamageTakenEvent
    {
        public Health AffectedHealth; // 受影响的健康组件
        public GameObject Instigator; // 伤害发起者
        public float CurrentHealth; // 当前生命值
        public float DamageCaused; // 造成的伤害
        public float PreviousHealth; // 之前的生命值
        public List<TypedDamage> TypedDamages; // 类型化伤害列表
    }
    
    [GameEvent]
    public partial struct TopDownEngineEvent
    {
        public TopDownEngineEventTypes EventType;
        public Character OriginCharacter;
    }

    [GameEvent]
    public partial struct HealthChangeEvent
    {
        public Health AffectedHealth;
        public float NewHealth;
    }

    [GameEvent]
    public partial struct CameraEvent
    {
        public CameraEventTypes EventType;
        public Character TargetCharacter;
    }
    
    [GameEvent]
    public partial struct EnemyKilledEvent
    {
        public Character Killer;
        public Character Target;
    }
}