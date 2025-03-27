using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace EdgeStudio
{
    public struct StateChangeEvent<T> where T : struct, IComparable, IConvertible, IFormattable
    {
        public GameObject Target;
        public StateMachine<T> TargetStateMachine;
        public T NewState;
        public T PreviousState;

        public StateChangeEvent(StateMachine<T> stateMachine)
        {
            Target = stateMachine.Target;
            TargetStateMachine = stateMachine;
            NewState = stateMachine.CurrentState;
            PreviousState = stateMachine.PreviousState;
        }
    }
    
    public sealed class StateMachine<T> where T : struct, IComparable, IConvertible, IFormattable
    {
        public bool TriggerEvents { get; set; }
        public readonly GameObject Target;
        public T CurrentState { get; private set; }
        public T PreviousState { get; private set; }

        // 状态变化的Subject
        private static readonly Subject<StateChangeEvent<T>> mStateChangeSubject = new Subject<StateChangeEvent<T>>();
        
        // 状态变化的公共Observable
        public static Observable<StateChangeEvent<T>> Event => mStateChangeSubject;

        /// <summary>
        /// 创建一个新的状态机，指定目标GameObject以及是否使用事件
        /// </summary>
        /// <param name="target">目标GameObject。</param>
        /// <param name="triggerEvents">如果设置为<c>true</c>则触发事件。</param>
        public StateMachine(GameObject target, bool triggerEvents)
        {
            this.Target = target;
            this.TriggerEvents = triggerEvents;
        }

        /// <summary>
        /// 将当前移动状态更改为参数中指定的状态
        /// </summary>
        /// <param name="newState">新状态。</param>
        public void ChangeState(T newState)
        {
            // 如果"新状态"与当前状态相同，我们不做任何事情并退出
            if (EqualityComparer<T>.Default.Equals(newState, CurrentState))
            {
                return;
            }

            // 我们存储之前的角色移动状态
            PreviousState = CurrentState;
            CurrentState = newState;

            if (TriggerEvents)
            {
                mStateChangeSubject.OnNext(new StateChangeEvent<T>(this));
            }
        }

        /// <summary>
        /// 将角色恢复到当前状态之前的状态
        /// </summary>
        public void RestorePreviousState()
        {
            // 我们恢复之前的状态
            CurrentState = PreviousState;

            if (TriggerEvents)
            {
                mStateChangeSubject.OnNext(new StateChangeEvent<T>(this));
            }
        }

        /// <summary>
        /// 清理方法，确保正确释放可观察对象
        /// </summary>
        public void Dispose() => mStateChangeSubject?.Dispose();
    }
}