using System;
using System.Collections.Generic;

#if UNITY_EDITOR
namespace Panthea.Asset
{
    public class EDITOR_TEMPVAR
    {
        public static bool IsBuilding = false;
        public static List<Action> RevertActions = new();
    }
}
#endif