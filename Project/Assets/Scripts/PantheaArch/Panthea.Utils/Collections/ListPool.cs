using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Panthea.Utils
{
    [Preserve]
    public class ListPool<T> : List<T>,IDisposable
    {
        public static ListPool<T> Create()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return new ListPool<T>();
#endif
            var result = ObjectPool.Fetch(typeof (ListPool<T>)) as ListPool<T>;
            result.Clear();
            return result;
        }

        public static ListPool<T> Create(IEnumerable<T> collection)
        {
            var pool = Create();
            pool.AddRange(collection);
            return pool;
        }
    
        public void Dispose()
        {
            Clear();
            ObjectPool.Recycle(this);
        }
    }
}