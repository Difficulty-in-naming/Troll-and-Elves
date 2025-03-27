using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Panthea.Utils
{
    [Preserve]
    public class QueuePool<T> : Queue<T>,IDisposable
    {
        public static QueuePool<T> Create()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return new QueuePool<T>();
#endif
            var result = ObjectPool.Fetch(typeof (QueuePool<T>)) as QueuePool<T>;
            result.Clear();
            return result;
        }
    
        public void Dispose()
        {
            Clear();
            ObjectPool.Recycle(this);
        }
    }
}