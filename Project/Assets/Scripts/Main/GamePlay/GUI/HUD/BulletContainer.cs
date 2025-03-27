using System;
using Panthea.Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EdgeStudio.GUI
{
    public class BulletContainer : BetterMonoBehaviour
    {
        [SerializeField] private ParabolicCurve Curve;

        [ShowInInspector, ReadOnly, NonSerialized] public bool IsDrop;

        public void SetEmpty()
        {
            IsDrop = true;
            Curve.SetToEnd();
        }

        public void SetHasAmmo()
        {
            IsDrop = false;
            Curve.SetToStart();
        }
        
        public void Drop()
        {
            if (IsDrop)
                return;
            IsDrop = true;
            Curve.Drop();
        }
    }
}