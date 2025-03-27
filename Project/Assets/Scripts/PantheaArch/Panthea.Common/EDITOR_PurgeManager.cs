#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Panthea.Common
{
    public class EDITOR_PurgeManager
    { 
        static List<IPurge> Purges = new List<IPurge>();
        private static List<Action> PurgeAction = new(); 
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Clear()
        {
            foreach (var node in Purges)
            {
                node.Clear();
            }
            Purges.Clear();
            foreach (var node in PurgeAction)
            {
                node();
            }
            PurgeAction.Clear();
        }

        [Conditional("UNITY_EDITOR")]
        public static void Add(IPurge purge)
        {
            Purges.Add(purge);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void Add(Action purge)
        {
            PurgeAction.Add(purge);
        }
    }
}
#endif