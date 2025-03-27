using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Panthea.Utils
{
    public static class GameObjectUtils
    {
        private static readonly List<Component> ComponentCache = new List<Component>();

        public static string GetObjPath(Transform root, Transform me)
        {
            Transform parent = me;
            StringBuilder path = new StringBuilder(me.name);
            while (true)
            {
                parent = parent.parent;
                if (parent == root)
                {
                    return path.ToString();
                }

                if (parent == null)
                {
                    return path.ToString();
                }
                else
                {
                    path.Insert(0, parent.name + "/");
                }
            }
        }

        /// <summary>
        /// 获取一个组件而不浪费内存
        /// </summary>
        /// <param name="this"></param>
        /// <param name="componentType"></param>
        /// <returns></returns>
        public static Component GetComponentNoAlloc(this GameObject @this, System.Type componentType)
        {
            @this.GetComponents(componentType, ComponentCache);
            Component component = ComponentCache.Count > 0 ? ComponentCache[0] : null;
            ComponentCache.Clear();
            return component;
        }

        /// <summary>
        /// 获取一个组件而不浪费内存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T GetComponentNoAlloc<T>(this GameObject @this) where T : Component
        {
            @this.GetComponents(typeof(T), ComponentCache);
            Component component = ComponentCache.Count > 0 ? ComponentCache[0] : null;
            ComponentCache.Clear();
            return component as T;
        }

        /// <summary>
        /// 在对象上获取组件，或者在其子对象上，或者在父对象上，如果没有找到则将其添加到对象上
        /// </summary>
        /// <param name="this"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetComponentAroundOrAdd<T>(this GameObject @this) where T : Component
        {
            T component = @this.GetComponentInChildren<T>(true);
            if (component == null)
            {
                component = @this.GetComponentInParent<T>();
            }

            if (component == null)
            {
                component = @this.AddComponent<T>();
            }

            return component;
        }

        /// <summary>
        /// 获取指定的组件，如果没有则添加并返回它
        /// </summary>
        /// <param name="gameObject"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this GameObject @this) where T : Component
        {
            T component = @this.GetComponent<T>();
            if (component == null)
            {
                component = @this.AddComponent<T>();
            }

            return component;
        }

        /// <summary>
        /// 获取指定的组件，如果没有则添加并返回它
        /// </summary>
        /// <param name="gameObject"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static (T newComponent, bool createdNew) FindOrCreateObjectOfType<T>(this GameObject @this, string newObjectName, Transform parent,
            bool forceNewCreation = false) where T : Component
        {
            T searchedObject = (T)Object.FindAnyObjectByType(typeof(T));
            if (searchedObject == null || forceNewCreation)
            {
                GameObject newGo = new GameObject(newObjectName);
                newGo.transform.SetParent(parent);
                return (newGo.AddComponent<T>(), true);
            }

            return (searchedObject, false);
        }
    }
}