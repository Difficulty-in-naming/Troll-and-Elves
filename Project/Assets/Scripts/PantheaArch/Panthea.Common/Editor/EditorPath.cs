using UnityEngine;

namespace Panthea.Common
{
    public static class EditorPath
    {
        public static string AssetDir => Application.dataPath + "/";
        public static string RootDir => Application.dataPath.Remove(Application.dataPath.Length - 6);
    }
}
