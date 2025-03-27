using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Panthea.Utils
{
    [Preserve]
    public class SortedDictionaryPool<T1,T2> : SortedDictionary<T1,T2>,IDisposable
    {
        public static SortedDictionaryPool<T1,T2> Create()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return new SortedDictionaryPool<T1,T2>();
#endif
            var result = ObjectPool.Fetch(typeof (SortedDictionaryPool<T1,T2>)) as SortedDictionaryPool<T1,T2>;
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