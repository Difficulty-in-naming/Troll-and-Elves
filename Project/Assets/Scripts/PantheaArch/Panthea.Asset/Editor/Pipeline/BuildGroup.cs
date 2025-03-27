using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Panthea.Asset.Define;
using Panthea.Utils;
using UnityEditor;
using UnityEngine;

namespace Panthea.Asset
{
    public class BuildGroup: AResPipeline
    {
        protected HashSet<string> BuildFiles;

        public BuildGroup(HashSet<string> buildFiles) => BuildFiles = buildFiles;

        readonly Dictionary<string, BuildObject> mBuildGroupMapping = new Dictionary<string, BuildObject>();
        public override Task Do()
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                var preference = BuildPreference.Instance;
                preference.Groups.RemoveAll(t1 => t1 == null);
                foreach (var file in BuildFiles)
                {
                    var dirInfo = BuildPreference.Instance.GetInfo(PathUtils.FormatFilePath(Path.GetDirectoryName(file)));
                    var fileInfo = BuildPreference.Instance.GetInfo(file) ?? new SetInfo { Path = file };

                    var info = dirInfo != null ? fileInfo.IsOverride ? fileInfo : dirInfo : fileInfo;

                    var dir = PathUtils.FormatFilePath(Path.GetDirectoryName(file));
                    var key = dir;

                    if (!mBuildGroupMapping.TryGetValue(key, out var group))
                    {
                        group = preference.FindGroup(key);
                        if (group == null)
                        {
                            group = ScriptableObject.CreateInstance<BuildObject>();
                            group.name = key;
                            group = preference.AddGroup(group);
                        }
                        group.Files.Clear();
                        group.PackingMode = info.PackingMode;
                        group.Include = info.Include;
                        group.CompressionMode = info.CompressionType;
                        group.RealAddress = dir;
                        mBuildGroupMapping.Add(key, group);
                        EditorUtility.SetDirty(group);
                    }
                    var address = string.IsNullOrEmpty(fileInfo.Address) ? PathUtils.RemoveFileExtension(fileInfo.Path) : fileInfo.Address;
                    group.Files.Add(new BuildObject.BuildFileKeyValue(Path.GetFileNameWithoutExtension(file).ToLower(), AssetDatabase.AssetPathToGUID("Assets/Res/" + file),
                        address
                    ));
                }
                
                for (var index = preference.Groups.Count - 1; index >= 0; index--)
                {
                    var node = preference.Groups[index];
                    if (!mBuildGroupMapping.ContainsKey(node.name.Replace("-", "/"))) preference.RemoveGroup(node);
                }
                EditorUtility.SetDirty(preference);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            return Task.CompletedTask;
        }
    }
}