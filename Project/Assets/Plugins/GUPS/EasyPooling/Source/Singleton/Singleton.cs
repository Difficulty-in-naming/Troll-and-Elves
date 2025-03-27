// Unity
using UnityEngine;

namespace GUPS.EasyPooling.Singleton
{
    /// <summary>
    /// A thread-safe singleton either active throughout the whole application or scene only.
    /// </summary>
    /// <typeparam name="T">The type of the singleton.</typeparam>
    public abstract class Singleton<T> : MonoBehaviour
        where T : Singleton<T>
    {
        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static T instance;

        /// <summary>
        /// Lock for thread safety.
        /// </summary>
        private static object lockHandle = new object();

        /// <summary>
        /// Returns true if the singleton is persistent.
        /// </summary>
        public abstract bool IsPersistent { get; }

        /// <summary>
        /// Returns an active singleton of this instance or creates a new one.
        /// </summary>
        public static T Instance
        {
            get
            {
                lock (lockHandle)
                {
                    // The GameObject got destroyed, so the singleton is null.
                    if (instance != null && instance.gameObject == null)
                    {
                        instance = null;
                    }

                    // If there is no singleton, create a new one.
                    if (instance == null)
                    {
#if UNITY_6000_0_OR_NEWER
                        instance = (T)FindFirstObjectByType(typeof(T));

                        if (FindObjectsByType(typeof(T), FindObjectsSortMode.None).Length > 1)
                        {
                            return instance;
                        }
#else
                        instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            return instance;
                        }
#endif

                        if (instance == null)
                        {
                            Create<T>();
                        }
                    }

                    return instance;
                }
            }
        }

        /// <summary>
        /// Returns true if a singleton exists.
        /// </summary>
        public static bool Exists
        {
            get
            {
                return instance != null;
            }
        }

        /// <summary>
        /// On awake, check if there is already a singleton.
        /// If there is one and it is not this, destroy the GameObject.
        /// </summary>
        protected virtual void Awake()
        {
            // If a singleton already exists and this is not the singleton, destroy it immediately. Else keep it.
            if (Exists)
            {
                if (this != instance && this.gameObject != null)
                {
                    DestroyImmediate(this.gameObject);
                }
            }
            else
            {
                // Set the singleton.
                instance = this as T;

                // Rename the GameObject and mark it as 'do not destroy' if it is persistent.
                if (instance.IsPersistent)
                {
                    // Rename the GameObject.
                    instance.gameObject.name = "(PersistentSingleton) " + typeof(T).ToString();

                    // Mark the GameObject as 'do not destroy'.
                    DontDestroyOnLoad(instance.gameObject);
                }
                else
                {
                    // Rename the GameObject.
                    instance.gameObject.name = "(Singleton) " + typeof(T).ToString();
                }
            }
        }

        /// <summary>
        /// Create a GameObject adding T1 and set the singleton to the value T1.
        /// </summary>
        /// <typeparam name="T1">The type of the singleton.</typeparam>
        private static void Create<T1>() where T1 : T
        {
            // Already exists, just return.
            if (Exists)
            {
                return;
            }

            // Create a GameObject.
            GameObject var_Singleton = new GameObject();

            // Add the singleton component to it.
            instance = var_Singleton.AddComponent<T1>();

            // Rename the GameObject and mark it as 'do not destroy' if it is persistent.
            if (instance.IsPersistent)
            {
                // Rename the GameObject.
                instance.gameObject.name = "(PersistentSingleton) " + typeof(T).ToString();

                // Mark the GameObject as 'do not destroy'.
                DontDestroyOnLoad(instance.gameObject);
            }
            else
            {
                // Rename the GameObject.
                instance.gameObject.name = "(Singleton) " + typeof(T).ToString();
            }
        }
    }
}
