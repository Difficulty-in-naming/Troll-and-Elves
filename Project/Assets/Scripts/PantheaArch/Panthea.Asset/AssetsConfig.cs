using System.Runtime.CompilerServices;
using UnityEngine;

namespace Panthea.Asset
{
    public class AssetsConfig
    {
        private static string mStreamingAssets;
        public static string StreamingAssets
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => mStreamingAssets ??= Application.streamingAssetsPath + "/";
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => mStreamingAssets = value;
        }

        private static string mPersistentDataPath;

        public static string PersistentDataPath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEVELOPMENT_BUILD
                return mPersistentDataPath ??= Application.persistentDataPath;
#elif UNITY_ANDROID && !UNITY_EDITOR
            if (mPersistentDataPath == null)
            {
                using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        mPersistentDataPath = currentActivity.Call<AndroidJavaObject>("getFilesDir").Call<string>("getCanonicalPath");
                    }
                }
            }
            return mPersistentDataPath;
#else
                return mPersistentDataPath ??= Application.persistentDataPath;
#endif
            }
        }
        
        public static bool UseAssetBundleCache
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool(Application.productName + "." + nameof (UseAssetBundleCache), true);
#else
#if WECHAT
                return false;
#else
                return true;
#endif
#endif
            }
        }

        public const string Suffix = ".bundle";

        public static string AssetBundlePersistentDataPath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => PersistentDataPath + "/resources/";
        }

        /// <summary>
        /// 保证运行时的AB包永远是最新的
        /// </summary>
        public static bool EnsureLatestBundle => true;
    }
}