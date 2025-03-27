using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Panthea.Utils
{
    [Preserve]
    public class DictionaryPool<T1,T2> : Dictionary<T1,T2>,IDisposable
    {
        public static DictionaryPool<T1,T2> Create()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return new DictionaryPool<T1,T2>();
#endif
            var result = ObjectPool.Fetch(typeof (DictionaryPool<T1,T2>)) as DictionaryPool<T1,T2>;
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
