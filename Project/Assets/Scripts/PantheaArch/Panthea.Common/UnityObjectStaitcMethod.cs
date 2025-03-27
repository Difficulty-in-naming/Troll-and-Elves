using System.Collections.Generic;
using UnityEngine;

namespace Panthea.Common
{
    public partial class UnityObject
    {
        internal string Tag { get; set; }
        private static Dictionary<GameObject, UnityObject> GameObjectMap = new();
        private static Dictionary<Transform, UnityObject> TransformMap = new();
        private static Dictionary<string, UnityObject> TagMap = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Purge()
        {
            GameObjectMap.Clear();
            TransformMap.Clear();
            TagMap.Clear();
        }
        
        public static UnityObject Instantiate(UnityObject uo)
        {
            var obj = Object.Instantiate(uo.GameObject);
            var result = new UnityObject(obj);
#if HAS_PANTHEA_ASSET
            result.FromBundle = uo.FromBundle;
            result.ReleaseBundle = uo.ReleaseBundle;
#endif
            return result;
        }
        
        public static UnityObject FindWithTag(string tag)
        {
            if (TagMap.TryGetValue(tag, out var value))
                return value;
            return null;
        }
        
        public static UnityObject FindWithObject(GameObject obj)
        {
            if (GameObjectMap.TryGetValue(obj, out var value))
                return value;
            return null;
        }

        public static UnityObject FindWithObject(Transform obj)
        {
            if (TransformMap.TryGetValue(obj, out var value))
                return value;
            return null;
        }
        
        public UnityObject WithTag(string tag)
        {
            TagMap.Remove(tag);
            TagMap.Add(tag,this);
            Tag = tag;
            return this;
        }

        internal void RemoveMap()
        {
            if (Tag != null) 
                TagMap.Remove(Tag);
            GameObjectMap.Remove(GameObject);
            TransformMap.Remove(Transform);
        }
        
        internal void RegisterMap()
        {
            GameObjectMap.Add(GameObject,this);
            TransformMap.Add(Transform,this);
        }
    }
}