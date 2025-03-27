using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Panthea.Utils
{
    [Preserve]
    public class HashSetPool<T> : HashSet<T>,IDisposable
    {
        public static HashSetPool<T> Create()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return new HashSetPool<T>();
#endif
            var result = ObjectPool.Fetch(typeof (HashSetPool<T>)) as HashSetPool<T>;
            result.Clear();
            return result;
        }

        public static HashSetPool<T> Create(IEnumerable<T> collection)
        {
            var pool = Create();
            foreach (var node in collection)
            {
                pool.Add(node);
            }
            return pool;
        }
    
        public void Dispose()
        {
            Clear();
            ObjectPool.Recycle(this);
        }
    }
}