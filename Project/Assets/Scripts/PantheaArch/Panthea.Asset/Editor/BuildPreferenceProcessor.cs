using UnityEditor;

namespace Panthea.Asset
{
    public class BuildPreferenceProcessor : AssetModificationProcessor
    {
        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            var info = BuildPreference.Instance.GetInfo(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(sourcePath));
            info.Path = destinationPath;
            BuildPreference.Instance.WriteInfo(info);
            return AssetMoveResult.DidNotMove;
        }
    }
}
