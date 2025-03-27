using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Panthea.Common
{
    [Preserve]
    public class MonoSingleton : BetterMonoBehaviour
    {
        [Preserve]
        public virtual void OnRelease()
        {
        }

        [Preserve]
        public virtual void OnCreate()
        {
        }
    }

    /// <summary>
    /// 用以标记类会单例类
    /// 便于后期管理和搜查问题
    /// 如果热更新需要卸载可以通过方法中的Release卸载资源
    /// 加载单例结束以后会返回加载完成的OnCreate的方法
    /// 请用OnCreate代替构造函数
    /// 以代替Unity中的Awake
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Preserve]
    public abstract class MonoSingleton<T>: MonoSingleton where T : MonoSingleton
    {
        [Preserve]
        private static T mInst;
        [Preserve]
        private static object mLock = new object();

        [Preserve]
        public static T Inst
        {
            get
            {
                if (mInst == null)
                {
                    CreateInstance();
#if UNITY_EDITOR
                    EDITOR_PurgeManager.Add(() => mInst = null);
#endif
                }

                return mInst;
            }
            //这个做法是不对的但是因为有的存档数据是在Manager里面的.所以不得已设置了可以被set
            set => mInst = value;
        }

        [Preserve]
        private static void CreateInstance()
        {
            mInst = FindFirstObjectByType<T>();
            mInst.OnCreate();
        }
        
        [Preserve]
        public void Dispose()
        {
            OnRelease();
            Destroy(mInst.gameObject);
            mInst = null;
        }
    }
}