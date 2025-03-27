using System;
using System.Collections.Generic;
using UnityEngine;

namespace EdgeStudio.R3
{
    public class ObservableDisableTrigger : MonoBehaviour
    {
        private readonly List<IDisposable> mDisposables = new();

        public bool IsActivated { get; private set; } = false;

        private void OnEnable() => IsActivated = true;

        private void OnDisable()
        {
            IsActivated = false;
            DisposeAll();
        }

        public void AddDisposableOnDisable(IDisposable disposable)
        {
            if (disposable != null && !mDisposables.Contains(disposable))
            {
                mDisposables.Add(disposable);
            }
        }

        private void DisposeAll()
        {
            foreach (var disposable in mDisposables)
            {
                disposable.Dispose();
            }
            mDisposables.Clear();
        }

        public void TryStartActivateMonitoring() => DisposeAll();
    }
}