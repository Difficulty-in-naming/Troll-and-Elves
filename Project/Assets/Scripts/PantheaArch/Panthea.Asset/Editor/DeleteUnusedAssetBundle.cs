using System.Collections.Generic;
using System.IO;
using System.Linq;
using Panthea.Utils;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace Panthea.Asset
{
    public class DeleteUnusedAssetBundle: IBuildTask
    {
        private void RemoveEmptyDir(string path)
        {
            var dir = new DirectoryInfo(path);
            var allDir = dir.GetDirectories();
            var allFile = dir.GetFiles();
            if (allDir.Length != 0)
            {
                foreach (var node in allDir)
                {
                    RemoveEmptyDir(node.FullName);
                }
            }
            if ((allFile.Length == 0 || allFile.All(t1 => t1.Extension == ".meta")) && dir.GetDirectories().Length == 0)
            {
                FileUtil.DeleteFileOrDirectory(PathUtils.FullPathToUnityPath(dir.FullName));
                var metaPath = dir.FullName + ".meta";
                if (File.Exists(metaPath))
                {
                    FileUtil.DeleteFileOrDirectory(PathUtils.FullPathToUnityPath(metaPath));
                }
            }
        }

        public ReturnCode Run()
        {
            var outputFolder = BuildPreference.Instance.OutputPath;
            var groups = BuildPreference.Instance.Groups;
            var hashMap = new Dictionary<string,BuildObject>();
            foreach (var node in groups)
            {
                var dir = node.name.Replace("-", "/");
                if(node.PackingMode == BuildObject.BundlePackingMode.PackTogether)
                    hashMap.Add(dir + AssetsConfig.Suffix, node);
                else
                {
                    foreach (var file in node.Files)
                    {
                        hashMap.Add(dir + "/" + file.Key + AssetsConfig.Suffix, node);
                    }
                }
            }
            var allAssetbundle = Directory.GetFiles(outputFolder + "/", "*" + AssetsConfig.Suffix, SearchOption.AllDirectories);
            foreach (var node in allAssetbundle)
            {
                string path = PathUtils.FormatFilePath(node.Replace(outputFolder + "/", ""));
                if (path == AssetBundleBuilder.BuiltInShaderBundleName || path == AssetBundleBuilder.BuiltInMonoScriptBundleName)
                    continue;
                if (!hashMap.ContainsKey(path))
                {
                    File.Delete(node);
                }
            }

            RemoveEmptyDir(outputFolder + "/");
            return ReturnCode.Success;
        }

        public int Version => 1;
    }
}