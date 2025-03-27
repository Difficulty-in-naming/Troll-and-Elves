using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Panthea.Utils
{
    /// <summary>
    /// 不可以直接使用，应该使用ListPool
    /// </summary>
    [Preserve]
    public static class ObjectPool
    {
        private static readonly Dictionary<Type, Queue<object>> mPool = new Dictionary<Type, Queue<object>>();

        private static int mMaxCount = 1000;

        public static T Fetch<T>() where T : class
        {
            return Fetch(typeof(T)) as T;
        }
    
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Purge()
        {
            mPool.Clear();
        }
#endif

        public static object Fetch(Type type)
        {

            if (!mPool.TryGetValue(type, out var queue))
            {
                return Activator.CreateInstance(type);
            }



            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                var x = queue.Dequeue();
                return x;
            }
        }

        public static void Recycle(object obj)
        {
            var type = obj.GetType();
            if (!mPool.TryGetValue(type, out var queue))
            {
                queue = new Queue<object>();
                mPool.Add(type, queue);
            }

            if (queue.Count > mMaxCount)
            {
                return;
            }

            foreach (var node in queue)
            {
                if (node == obj)
                {
                    return;
                }
            }
            queue.Enqueue(obj);
        }

        public static void SetMax(int max)
        {
            mMaxCount = max;
        }
    }
}