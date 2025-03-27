using System;
using System.Collections.Generic;

namespace Panthea.Utils
{
    public abstract class UnityObjectPool<T> : IDisposable
        where T : UnityEngine.Component
    {
        private bool isDisposed = false;
        protected Queue<T> Queue { get; private set; }

        /// <summary> 最大实例数量 </summary>
        protected virtual int MaxPoolCount => int.MaxValue;

        /// <summary> 必须实现创建实例的方法 </summary>
        protected abstract T CreateInstance();

        /// <summary> 在返回池之前调用，用于设置活动对象（这是默认行为）</summary>
        protected virtual void OnBeforeRent(T instance)
        {
            instance.gameObject.SetActive(true);
        }

        /// <summary> 在返回到池之前调用，用于设置非活动对象（这是默认行为） </summary>
        protected virtual void OnBeforeReturn(T instance)
        {
            instance.gameObject.SetActive(false);
        }

        /// <summary> 当清除或销毁时调用，用于销毁实例或执行其他终结方法 </summary>
        protected virtual void OnClear(T instance)
        {
            if (instance == null)
            {
                return;
            }

            var go = instance.gameObject;
            if (go == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(go);
        }

        /// <summary> 池子中的物体数量 </summary>
        public int Count => Queue?.Count ?? 0;

        /// <summary> 从池子中获得实例 </summary>
        public T Rent()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException("ObjectPool was already disposed.");
            }

            Queue ??= new Queue<T>();

            var instance = Queue.Count > 0 ? Queue.Dequeue() : CreateInstance();

            OnBeforeRent(instance);
            return instance;
        }

        /// <summary> 将实例放回池子内部 </summary>
        public void Return(T instance)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException("ObjectPool was already disposed.");
            }

            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            Queue ??= new Queue<T>();

            if (Queue.Count + 1 == MaxPoolCount)
            {
                throw new InvalidOperationException("Reached Max PoolSize");
            }

            OnBeforeReturn(instance);
            Queue.Enqueue(instance);
        }

        /// <summary> 清空池子 </summary>
        public void Clear(bool callOnBeforeRent = false)
        {
            if (Queue == null)
            {
                return;
            }

            while (Queue.Count != 0)
            {
                var instance = Queue.Dequeue();
                if (callOnBeforeRent)
                {
                    OnBeforeRent(instance);
                }

                OnClear(instance);
            }
        }
    
        /// <summary>
        /// 调整池中的实例。
        /// </summary>
        /// <param name="instanceCountRatio">0.0f = 清除所有 ~ 1.0f = 保留所有。</param>
        /// <param name="minSize">最小池计数。</param>
        /// <param name="callOnBeforeRent">如果为true，在OnClear之前调用OnBeforeRent。</param>
        public void Shrink(float instanceCountRatio, int minSize, bool callOnBeforeRent = false)
        {
            if (Queue == null)
            {
                return;
            }

            if (instanceCountRatio <= 0)
            {
                instanceCountRatio = 0;
            }

            if (instanceCountRatio >= 1.0f)
            {
                instanceCountRatio = 1.0f;
            }

            var size = (int)(Queue.Count * instanceCountRatio);
            size = System.Math.Max(minSize, size);

            while (Queue.Count > size)
            {
                var instance = Queue.Dequeue();
                if (callOnBeforeRent)
                {
                    OnBeforeRent(instance);
                }

                OnClear(instance);
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    Clear(false);
                }

                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
