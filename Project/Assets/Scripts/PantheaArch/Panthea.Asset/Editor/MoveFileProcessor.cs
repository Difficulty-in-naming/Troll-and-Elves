using UnityEditor.Experimental;

//using UnityEditor.AddressableAssets;

namespace Panthea.Asset
{
    public class MoveFileProcessor: AssetsModifiedProcessor
    {
        private string flag = $"Assets/{AssetsPackPath.Path}";

        protected override void OnAssetsModified(string[] changedAssets, string[] addedAssets, string[] deletedAssets, AssetMoveInfo[] movedAssets)
        {
            /*var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            foreach (var node in movedAssets)
            {
                var source = node.sourceAssetPath;
                var destination = node.destinationAssetPath;
                if (source.StartsWith(flag))
                {
                    if (!destination.StartsWith(flag))
                    {
                        settings.RemoveAssetEntry(AssetDatabase.AssetPathToGUID(destination), false);
                    }
                }
            }*/
        }
    }
}