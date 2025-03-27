using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Panthea.Common
{
    /// <summary>
    /// 这是对Unity GameObject 方法的再封装.
    /// 因为Unity再处理GameObject.xxx的时候并不是直接拿的缓存或者指针中的数据
    /// 而是直接重新包装一个新的指针或者数据发送给c#.比如GameObject.transform.实际再底层等同于 GameObject.GetComponent<!--Transform-->()
    /// 而这个类就是为了避免这种情况的发生
    /// 其次.通过使用这个类.我们能够很精确的知道GameObject中的Position和Rotation等方法是从何处调用的.方便调试
    /// SetMaterial方法在UnityObjectMaterialExtensions类中.这是一个扩展类
    /// </summary>
    public sealed partial class UnityObject
    {
        //为了避免GC我们在这里存储不常访问名字的列表
        private List<UnityObject> mAllChildren;

        private Transform mCachedTransform;

        //Unity 不会再一帧内迅速刷新GameObject的状态.所以我们需要加入一个自己的状态
        private bool mDisposed;

        //需要经常访问名字的会存储在这
        private readonly Dictionary<string, UnityObject> mFindChildCache = new();

        private string mName;

        private UnityObject mParent;

        private Dictionary<string, object> mData;

        private UnityObjectHook mHook;
#if HAS_PANTHEA_ASSET
        private object FromBundle;
        private Action<object> ReleaseBundle;
        public UnityObject(GameObject go,object fromBundle,Action<object> releaseBundle)
        {
            GameObject = go;
            FromBundle = fromBundle;
            ReleaseBundle = releaseBundle;
            Init();
            RegisterMap();
        }
#endif
        public event Action<UnityObject> AfterDestroyCallback;
        public UnityObject(GameObject go)
        {
            GameObject = go;
            Init();
            RegisterMap();
        }
        
        public UnityObject(Transform transform)
        {
            GameObject = transform.gameObject;
            Init();
            RegisterMap();
        }

        public UnityObject(string name)
        {
            GameObject = new GameObject(name);
            Init();
            RegisterMap();
        }

        private void Init()
        {
            mHook = GameObject.AddComponent<UnityObjectHook>();
            mHook.Init(this);
            mHook.AddDestroyCallback(o => o.Dispose());
        }

        /// <summary>
        ///     禁止暴露这个变量！！
        /// </summary>
        internal Transform Transform
        {
            get
            {
                if (!mCachedTransform) mCachedTransform = GameObject.transform;

                return mCachedTransform;
            }
        }

        /// <summary>
        ///     禁止暴露这个变量！！
        /// </summary>
        internal GameObject GameObject { get; }

        public UnityObject Parent
        {
            get { return mParent ??= new UnityObject(Transform.parent.gameObject); }
            set
            {
                mParent = value;
                Transform.parent = value.Transform;
            }
        }

        private Dictionary<Type, WrapComponents> Components { get; } = new();

        public Vector3 LocalPosition
        {
            get => Transform.localPosition;
            set => Transform.localPosition = value;
        }

        public string Name
        {
            get
            {
                mName ??= GameObject.name.Replace("(Clone)","");
                return mName;
            }
            set
            {
                mName = value;
                GameObject.name = value;
            }
        }

        public bool Active
        {
            get => GameObject.activeSelf;
            set
            {
                if (Active != value) GameObject.SetActive(value);
            }
        }

        public Vector3 Right
        {
            get => Transform.right;
            set => Transform.right = value;
        }

        public Vector3 Position
        {
            get => Transform.position;
            set => Transform.position = value;
        }

        public float X
        {
            get => Transform.position.x;
            set
            {
                var pos = Transform.position;
                pos.x = value;
                Transform.position = pos;
            }
        }

        public float Y
        {
            get => Transform.position.y;
            set
            {
                var pos = Transform.position;
                pos.y = value;
                Transform.position = pos;
            }
        }

        public float Z
        {
            get => Transform.position.z;
            set
            {
                var pos = Transform.position;
                pos.z = value;
                Transform.position = pos;
            }
        }

        public Quaternion LocalRotation
        {
            get => Transform.localRotation;
            set => Transform.localRotation = value;
        }

        public Quaternion Rotation
        {
            get => Transform.rotation;
            set => Transform.rotation = value;
        }

        public Vector3 EulerAngles
        {
            get => Transform.eulerAngles;
            set => Transform.eulerAngles = value;
        }

        public int Layer
        {
            get => GameObject.layer;
            set => GameObject.layer = value;
        }

        public Vector3 LocalEulerAngles
        {
            get => Transform.localEulerAngles;
            set => Transform.localEulerAngles = value;
        }

        public Vector3 LocalScale
        {
            get => Transform.localScale;
            set => Transform.localScale = value;
        }
        
        public Vector3 LossyScale
        {
            get => Transform.lossyScale;
            set
            {
                var lossyScale = value;

                Transform.localScale = Vector3.one;
                var scale = Transform.lossyScale;
                Transform.localScale = new Vector3(lossyScale.x / scale.x, lossyScale.y / scale.y, lossyScale.z / scale.z);
            }
        }


        public Vector3 Forward => Transform.forward;

        public Vector3 Up => Transform.up;

        public Matrix4x4 WorldMatrix => Transform.localToWorldMatrix;

        public void RemoveComponents<T>() where T : Object
        {
            var t = typeof(T);
            var components = GetComponents<T>();
            foreach (var node in components) Object.Destroy(node);

            Components.Remove(t);
        }

        public Vector3 TransformPoint(Vector3 vec) => Transform.TransformPoint(vec);

        public Vector3 TransformPoint(float x, float y, float z) => Transform.TransformPoint(x, y, z);

        public T GetComponent<T>()
        {
            var type = typeof(T);
            if (Components.TryGetValue(type, out var wc))
            {
                return (T)wc.First();
            }

            var result = GameObject.GetComponent<T>();
            if (result != null)
            {
                wc = new WrapComponents();
                Components.Add(type, wc);
                wc.Components.Add(result);
            }

            return result;
        }

        public List<T> GetComponents<T>()
        {
            var type = typeof(T);
            var result = new List<T>();
            if (Components.TryGetValue(type, out var wc))
            {
                if (wc.HasGetComponents)
                {
                    foreach (var node in wc.Components) result.Add((T)node);

                    return result;
                }

                wc.Components.Clear();
            }

            var com = GameObject.GetComponents<T>();
            if (wc == null)
            {
                wc = new WrapComponents();
                Components.Add(type, wc);
            }

            foreach (var node in com)
            {
                wc.Components.Add(node);
                result.Add(node);
            }

            return result;
        }

        public T AddComponent<T>() where T : Component
        {
            var type = typeof(T);
            if (!Components.TryGetValue(type, out var wc))
            {
                wc = new WrapComponents();
                Components.Add(type, wc);
            }

            var result = GameObject.AddComponent<T>();
            wc.Components.Add(result);
            return result;
        }

        public UnityObject Find(string path)
        {
            mFindChildCache.TryGetValue(path, out var obj);
            if (obj == null || obj.IsDisposed())
            {
                var transform = Transform.Find(path);
                if (transform)
                {
                    obj = new UnityObject(transform.gameObject);
                    mFindChildCache[path] = obj;
                }
            }

            return obj;
        }

        public void SetLayer(int layer, bool includeChildren)
        {
            if (includeChildren)
                foreach (var node in GetChildren())
                    if (node.Layer != layer)
                        node.Layer = layer;
            if (Layer != layer)
                Layer = layer;
        }

        public void AddChild(Transform transform)
        {
            transform.parent = Transform;
            mAllChildren?.Add(new UnityObject(transform.gameObject));
        }
        
        public void AddChild(GameObject gameObject) => AddChild(gameObject.transform);

        public void AddChild(UnityObject uo)
        {
            uo.Parent = this;
            mAllChildren?.Add(uo);
        }

        public UnityObject GetChild(string name) => Find(name);

        public void SetParent(UnityObject uo) => Parent = uo;

        public List<T> GetComponentsInChildren<T>(bool includeInactive = false) where T : Object
        {
            var list = GetChildren();
            list.Insert(0, this);
            var com = new List<T>();
            foreach (var node in list)
            {
                if (!includeInactive)
                    if (!node.Active)
                        continue;
                var t = node.GetComponent<T>();
                if (t != null)
                    com.Add(t);
            }

            return com;
        }

        public List<UnityObject> GetChildren()
        {
            if (mAllChildren == null) InternalGetChildren(this);

            return mAllChildren;
        }

        private List<UnityObject> InternalGetChildren(UnityObject parent)
        {
            mAllChildren ??= new List<UnityObject>();
            foreach (var node in parent.Transform)
            {
                var transform = (Transform)node;
                var gameObject = transform.gameObject;
                if (!gameObject)
                {
                    Debug.LogError("物体丢失,无法找到该GameObject");
                }
                else
                {
                    var uo = new UnityObject(gameObject)
                    {
                        Parent = this
                    };
                    mAllChildren.Add(uo);
                    mAllChildren.AddRange(uo.InternalGetChildren(uo));
                }
            }

            return mAllChildren;
        }

        public void LookAt(UnityObject target, Vector3? worldUp = null)
        {
            if (worldUp.HasValue)
                Transform.LookAt(target.Transform, worldUp.Value);
            else
                Transform.LookAt(target.Transform);
        }

        public void LookAt(Vector3 pos, Vector3? worldUp = null)
        {
            if (worldUp.HasValue)
                Transform.LookAt(pos, worldUp.Value);
            else
                Transform.LookAt(pos);
        }

        public void SetPositionAndRotation(Vector3 pos,Quaternion rotation) => Transform.SetPositionAndRotation(pos, rotation);

        public void Rotate(Vector3 vec) => Transform.Rotate(vec);

        public bool Equals(GameObject obj) => obj == GameObject;

        public bool Equals(Transform obj) => obj == Transform;

        public void Dispose()
        {
            if (!mDisposed && GameObject)
            {
                mParent?.mAllChildren?.Remove(this);
                Object.Destroy(GameObject);
                mDisposed = true;
                AfterDestroyCallback?.Invoke(this);
                RemoveMap();
#if HAS_PANTHEA_ASSET
                ReleaseBundle?.Invoke(FromBundle);
#endif
            }
        }

        public bool IsDisposed() => !GameObject || mDisposed;

        public UnityObject Clone() => new(Object.Instantiate(GameObject));

        public int GetInstanceID() => Transform.GetInstanceID();

        public void DontDestroyOnLoad() => Object.DontDestroyOnLoad(GameObject);

        public void AddData(string key, object data)
        {
            mData ??= new Dictionary<string, object>();
            if (!mData.TryAdd(key, data)) Log.Warning("Data使用了相同的Key,这可能导致一些逻辑问题");
        }

        public T GetData<T>(string key)
        {
            if (mData == null)
            {
                Log.Warning("Data内不存在任何Key");
                return default;
            }

            if (mData.TryGetValue(key, out var value)) return (T)value;
            Log.Warning($"UnityObject找不到Data:{key}");
            return default;
        }

        public bool ContainsData(string key) => mData.ContainsKey(key);

        private class WrapComponents
        {
            public readonly List<object> Components = new();

            /// <summary>
            ///     是否调用过GetComponents
            ///     通过添加这个接口我们避免AddComponent和GetComponent的时候也需要调用GetComponents生成列表.可以在一定程度提升性能
            /// </summary>
            public readonly bool HasGetComponents = false;

            public object First()
            {
                if (Components.Count == 0) return null;

                return Components[0];
            }
        }
    }
}