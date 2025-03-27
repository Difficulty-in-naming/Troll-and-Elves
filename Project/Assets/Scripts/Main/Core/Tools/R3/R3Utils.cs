using System;
using UnityEngine;

namespace EdgeStudio.R3
{
    public static class R3Utils
    {
        public static T AddToDisable<T>(this T disposable, GameObject gameObject) where T : IDisposable
        {
            if (gameObject == null)
            {
                disposable.Dispose();
                return disposable;
            }

            var trigger = gameObject.GetComponent<ObservableDisableTrigger>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<ObservableDisableTrigger>();
            }

            if (trigger.IsActivated && !trigger.gameObject.activeInHierarchy)
            {
                trigger.TryStartActivateMonitoring();
            }

            trigger.AddDisposableOnDisable(disposable);
            return disposable;
        }
    }
}