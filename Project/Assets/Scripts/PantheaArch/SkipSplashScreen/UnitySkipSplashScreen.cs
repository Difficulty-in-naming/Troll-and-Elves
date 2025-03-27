// #if !UNITY_EDITOR

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace SkipSplashScreen
{
    public sealed class UnitySkipSplashScreen
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void BeforeSplashScreen()
        {
#if UNITY_WEBGL
            Application.focusChanged += Application_focusChanged;
#else
            AsyncSkip().Forget();
#endif
        }
#if UNITY_WEBGL
        private static void Application_focusChanged(bool obj)
        {
            Application.focusChanged -= Application_focusChanged;
            SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
        }
#else
        private static async UniTask AsyncSkip()
        {
            while (true)
            {
                SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
                await UniTask.NextFrame();
                if (!SplashScreen.isFinished)
                {
                    return;
                }
            }
        }
#endif
    }
}
// #endif