using EdgeStudio.Odin;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EdgeStudio
{
    /// <summary>
    /// 一个处理冷却相关属性及其随时间消耗资源的类
    /// 记得从另一个类中初始化它(一次)并每帧更新它
    /// </summary>
    [System.Serializable]
    [InlineProperty,HideLabel]
    public class Cooldown 
    {
        /// 对象的所有可能状态
        public enum CooldownStates { Idle, Consuming, Stopped, Refilling }
        /// 如果为true，冷却将不会执行任何操作
        [ColorFoldout("冷却")] public bool Unlimited = false;
        /// 消耗对象所需的时间，以秒为单位
        [ColorFoldout("冷却")] public float ConsumptionDuration = 2f;
        /// 对象耗尽后，在重新填充前应用的暂停时间
        [ColorFoldout("冷却")] public float PauseOnEmptyDuration = 1f;
        /// 如果不中断，重新填充的持续时间，以秒为单位
        [ColorFoldout("冷却")] public float RefillDuration = 1f;
        /// 重新填充是否可以被新的Start指令中断
        [ColorFoldout("冷却")] public bool CanInterruptRefill = true;

        /// 对象的当前状态
        [ColorFoldout("冷却"), ReadOnly] public CooldownStates CooldownState = CooldownStates.Idle;
        /// 对象在任何给定时间剩余的持续时间量
        [ColorFoldout("冷却"), ReadOnly] public float CurrentDurationLeft;
        
        public ReactiveProperty<CooldownStates> OnStateChange = new();

        protected float _emptyReachedTimestamp = 0f;

        /// <summary>
        /// 确保对象被重置的初始化方法
        /// </summary>
        public virtual void Initialization()
        {
            CurrentDurationLeft = ConsumptionDuration;
            ChangeState(CooldownStates.Idle);
            _emptyReachedTimestamp = 0f;
        }

        /// <summary>
        /// 如果可能，开始消耗冷却对象
        /// </summary>
        public virtual void Start()
        {
            if (Ready())
            {
                ChangeState(CooldownStates.Consuming);
            }
        }

        /// <summary>
        /// 如果冷却准备好被消耗则返回true，否则返回false
        /// </summary>
        /// <returns></returns>
        public virtual bool Ready()
        {
            if (Unlimited)
            {
                return true;
            }
            if (CooldownState == CooldownStates.Idle)
            {
                return true;
            }
            if ((CooldownState == CooldownStates.Refilling) && (CanInterruptRefill))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 停止消耗对象
        /// </summary>
        public virtual void Stop()
        {
            if (CooldownState == CooldownStates.Consuming)
            {
                ChangeState(CooldownStates.Stopped);
            }
        }
        
        public float Progress 
        {
            get
            {
                if (Unlimited)
                {
                    return 1f;
                }
                
                if (CooldownState == CooldownStates.Consuming || CooldownState == CooldownStates.Stopped)
                {
                    return 0f;
                }

                if (CooldownState == CooldownStates.Refilling)
                {
                    return CurrentDurationLeft / RefillDuration;
                }
                
                return 1f;
            }
        }

        /// <summary>
        /// 处理对象的状态机
        /// </summary>
        public virtual void Update()
        {
            if (Unlimited)
            {
                return;
            }

            switch (CooldownState)
            {
                case CooldownStates.Idle:
                    break;

                case CooldownStates.Consuming:
                    CurrentDurationLeft = CurrentDurationLeft - Time.deltaTime;
                    if (CurrentDurationLeft <= 0f)
                    {
                        CurrentDurationLeft = 0f;
                        _emptyReachedTimestamp = Time.time;
                        ChangeState(CooldownStates.Stopped);
                    }
                    break;

                case CooldownStates.Stopped:
                    if (Time.time - _emptyReachedTimestamp >= PauseOnEmptyDuration)
                    {
                        ChangeState(CooldownStates.Refilling);
                    }
                    break;

                case CooldownStates.Refilling:
                    CurrentDurationLeft += (RefillDuration * Time.deltaTime) / RefillDuration;
                    if (CurrentDurationLeft >= RefillDuration)
                    {
                        CurrentDurationLeft = ConsumptionDuration;
                        ChangeState(CooldownStates.Idle);
                    }
                    break;
            }
        }

        /// <summary>
        /// 更改冷却的当前状态并在需要时调用观察者
        /// </summary>
        /// <param name="newState"></param>
        protected virtual void ChangeState(CooldownStates newState)
        {
            CooldownState = newState;
            OnStateChange.Value = newState;
        }
    }
}
