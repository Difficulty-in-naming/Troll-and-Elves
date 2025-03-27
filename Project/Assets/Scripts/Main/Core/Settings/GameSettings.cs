using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Panthea.Asset;
using Panthea.Common;
using UnityEngine;

namespace EdgeStudio
{
    public class GameSettings
    {
        public static char[] StringSplit = { ',' };
        public static char[] ItemConfigSplit = { ':' };
        public static string PersistentDataPath { get; } = AssetsConfig.PersistentDataPath;
        public static string Version { get; set; }
        
        public static int IntVersion
        {
            get
            {
                var match = Regex.Match(Version, @"\d+(\.\d+)*");
                if (!match.Success) return 0;
                if (int.TryParse(match.Value.Replace(".", ""), out var result)) return result;
                return 0;
            }
        }
        public static bool MobileRuntime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool(Application.productName + "." + nameof (MobileRuntime), false);
#else
                return true;
#endif
            }
        }
        
        public static bool EnableNetwork
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool(Application.productName + "." + nameof (EnableNetwork), true);
#else
                return true;
#endif
            }
        }

        private static UnityObject mMainCamera;
        public static UnityObject MainCamera
        {
            get
            {
                return mMainCamera ??= new UnityObject(Camera.main.gameObject);
            }
        }

        public static bool SkipGuide
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool(Application.productName + "." + nameof (SkipGuide), false);
#else
                return false;
#endif
            }
        }
        
        /// <summary>
        /// 忽略功能的等级限制
        /// </summary>
        public static bool IgnoreLevelUnlock
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if UNITY_EDITOR
                Debug.Log(UnityEditor.EditorPrefs.GetBool(Application.productName + "." + nameof (IgnoreLevelUnlock), false));
                return UnityEditor.EditorPrefs.GetBool(Application.productName + "." + nameof (IgnoreLevelUnlock), false);
#else
                return false;
#endif
            }
        }


        /// <summary>
        /// 是否可以打开超链接
        /// </summary>
        public static bool CanOpenURL
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if WECHAT || KUAISHOU || DOUYIN
                return false;
#endif
                return true;
            }
        }

        public static int PlayerId { get; set; } = 0;
        public static string AuthCode { get; set; }
        public static string OpenId { get; set; }
        public static bool IsLoadingComplete { get; set; }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Cleanup()
        {
            PlayerId = 0;
            AuthCode = null;
            OpenId = null;
            IsLoadingComplete = false;
        }
#endif
    }
}