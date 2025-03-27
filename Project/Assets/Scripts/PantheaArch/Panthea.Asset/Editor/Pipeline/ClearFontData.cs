using System.Reflection;
using System.Threading.Tasks;
using Panthea.Asset.Define;
using TMPro;
using UnityEditor;
namespace Panthea.Asset
{
    public class ClearFontData : AResPipeline
    {
        public override Task Do()
        {
            var assets = AssetDatabase.FindAssets("t:TMP_FontAsset");
            foreach (var node in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(node);
                var tmp = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (tmp.atlasPopulationMode == AtlasPopulationMode.Dynamic)
                {
                    tmp.GetType().GetMethod("ClearFontAssetDataInternal",BindingFlags.Instance | BindingFlags.NonPublic).Invoke(tmp, null);
                }
            }

            return Task.CompletedTask;
        }
    }
}