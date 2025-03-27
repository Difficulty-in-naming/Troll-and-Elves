using System;
using System.Collections;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace EdgeStudio
{
    public class ObservableMono : MonoBehaviour
    {
        Subject<bool> onApplicationFocus;
        Subject<bool> onApplicationPause;
        Subject<Unit> onApplicationQuit;
        private static ObservableMono instance;
        static ObservableMono Instance => instance;

        void Awake()
        {
            instance = this;
        }
        
        void OnApplicationFocus(bool focus)
        {
            if (onApplicationFocus != null) onApplicationFocus.OnNext(focus);
        }

        public static Observable<bool> OnApplicationFocusAsObservable()
        {
            return Instance.onApplicationFocus ?? (Instance.onApplicationFocus = new Subject<bool>());
        }

        void OnApplicationPause(bool pause)
        {
            if (onApplicationPause != null) onApplicationPause.OnNext(pause);
        }

        public static Observable<bool> OnApplicationPauseAsObservable()
        {
            return Instance.onApplicationPause ?? (Instance.onApplicationPause = new Subject<bool>());
        }

        void OnApplicationQuit()
        {
            if (onApplicationQuit != null) onApplicationQuit.OnNext(Unit.Default);
        }

        public static Observable<Unit> OnApplicationQuitAsObservable()
        {
            return Instance.onApplicationQuit ?? (Instance.onApplicationQuit = new Subject<Unit>());
        }
    }
}
