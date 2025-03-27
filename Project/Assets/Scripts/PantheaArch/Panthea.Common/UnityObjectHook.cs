using System;
using System.Collections.Generic;
using UnityEngine;

namespace Panthea.Common
{
    public class UnityObjectHook : MonoBehaviour
    {
        private readonly List<Action<UnityObject>> mDestroyCallback = new();
        private UnityObject mUnityObject;
        public void Init(UnityObject obj)
        {
            mUnityObject = obj;
        }

        public void AddDestroyCallback(Action<UnityObject> uo) => mDestroyCallback.Add(uo);
        public void RemoveDestroyCallback(Action<UnityObject> uo) => mDestroyCallback.Remove(uo);

        private void OnDestroy()
        {
            foreach (var node in mDestroyCallback)
            {
                node(mUnityObject);
            }
        }
    }
}