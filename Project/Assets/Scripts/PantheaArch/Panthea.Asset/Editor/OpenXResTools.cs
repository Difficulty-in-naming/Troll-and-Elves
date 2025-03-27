using UnityEditor;

namespace Panthea.Asset
{
    public class OpenXResTools: UnityEditor.Editor
    {
        [MenuItem("Tools/Addressable/Pack")]
        public static void Init()
        {
            AssetBundleBuilder.Pack();
        }
    }
}