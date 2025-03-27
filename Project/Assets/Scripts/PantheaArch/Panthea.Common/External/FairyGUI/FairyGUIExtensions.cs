#if PANTHEA_COMMON_FAIRY_SUPPORT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Panthea.Common
{
    public static class UnityObjectExtensions
    {
        public static void SetWrapTarget(this FairyGUI.GoWrapper wrapper,UnityObject uo,bool cloneMaterial)
        {
            wrapper.SetWrapTarget(uo.GameObject, cloneMaterial);
        }
    }
}
#endif

