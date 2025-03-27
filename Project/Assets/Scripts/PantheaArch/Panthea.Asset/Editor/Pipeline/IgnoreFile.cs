using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Panthea.Asset.Define;
using Panthea.Utils;
using UnityEditor;

namespace Panthea.Asset
{
    public class IgnoreFile : AResPipeline
    {
        public HashSet<string> BuildFiles;

        public IgnoreFile(HashSet<string> buildFiles)
        {
            BuildFiles = buildFiles;
        }
        public override Task Do()
        {
            var allInfo = BuildPreference.Instance.GetAllInfo();
            foreach (var node in allInfo.List)
            {
                if (node.Ignore)
                {
                    var path = node.Path.Remove(0, "Assets/Res/".Length);
                    if (node.Obj is DefaultAsset defaultAsset)
                    {
                        var folderPath = AssetDatabase.GetAssetPath(defaultAsset);
                        if (AssetDatabase.IsValidFolder(folderPath))
                            BuildFiles.RemoveWhere(t1 =>
                                string.Compare(PathUtils.FormatFilePath(Path.GetDirectoryName(t1)), path, StringComparison.OrdinalIgnoreCase) == 0);
                    }
                    else
                        BuildFiles.Remove(path.ToLower());
                }
            }
            return Task.CompletedTask;
        }
    }
}