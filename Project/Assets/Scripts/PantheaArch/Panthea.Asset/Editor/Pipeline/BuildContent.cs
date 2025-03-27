using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Panthea.Asset.Define;
using Panthea.Common;
using Panthea.Utils;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine;
using BuildCompression = UnityEngine.BuildCompression;
using CompressionType = UnityEngine.CompressionType;

namespace Panthea.Asset
{
    public class BuildContent: AResPipeline
    {
        public override Task Do()
        {
            var preference = BuildPreference.Instance;
            var buildTasks = RuntimeDataBuildTasks();
            var lzmaFile = new List<AssetBundleBuild>();
            var lz4File = new List<AssetBundleBuild>();
            var unCompressFile = new List<AssetBundleBuild>();
            var dict = preference.Groups.ToDictionary(t1 => t1.name);
            foreach (var node in dict)
            {
                if (node.Value.PackingMode == BuildObject.BundlePackingMode.PackSeparately)
                {
                    foreach (var file in node.Value.Files)
                    {
                        var build = new AssetBundleBuild { assetBundleName = node.Key.Replace("-", "/") + "/" + file.Key + AssetsConfig.Suffix };
                        build.assetNames = new[] { AssetDatabase.GUIDToAssetPath(file.GUID) };
                        var addrName = string.IsNullOrEmpty(file.Address) ? PathUtils.RemoveFileExtension(file.Key.Remove(0, "Assets/Res/".Length)).ToLower() : file.Address.ToLower();
                        build.addressableNames = new[] { addrName };
                        if(node.Value.CompressionMode == CompressionType.Lzma)
                            lzmaFile.Add(build);
                        else if (node.Value.CompressionMode == CompressionType.Lz4)
                            lz4File.Add(build);
                        else
                            unCompressFile.Add(build);
                    }
                }
                else
                {
                    var build = new AssetBundleBuild { assetBundleName = node.Key.Replace("-", "/") + AssetsConfig.Suffix };
                    var assetsName = new List<string>();
                    var addressableName = new List<string>();
                    foreach (var file in node.Value.Files)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(file.GUID);
                        assetsName.Add(path);
                        var addrName = string.IsNullOrEmpty(file.Address) ? PathUtils.RemoveFileExtension(path.Remove(0, "Assets/Res/".Length)).ToLower() : file.Address.ToLower();
                        addressableName.Add(addrName);
                    }

                    build.addressableNames = addressableName.ToArray();
                    build.assetNames = assetsName.ToArray();
                    if(node.Value.CompressionMode == CompressionType.Lzma)
                        lzmaFile.Add(build);
                    else if (node.Value.CompressionMode == CompressionType.Lz4)
                        lz4File.Add(build);
                    else
                        unCompressFile.Add(build);
                }
            }
            var outputPath = preference.OutputPath;
            if (lzmaFile.Count > 0)
                BuildBundle(outputPath, lzmaFile, BuildCompression.LZMA, buildTasks);
            if (lz4File.Count > 0)
                BuildBundle(outputPath, lz4File, BuildCompression.LZ4, buildTasks);
            if (unCompressFile.Count > 0)
                BuildBundle(outputPath, unCompressFile, BuildCompression.Uncompressed, buildTasks);
            return Task.CompletedTask;
        }

        private static void BuildBundle(string outputPath, List<AssetBundleBuild> file,BuildCompression compression, IList<IBuildTask> buildTasks)
        {
            bool hasDuplicate = false;
            foreach (var node in file)
            {
                var duplicates = node.addressableNames.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
                if (duplicates.Length > 0)
                {
                    // 如果 duplicates 不为空,说明有重复项
                    foreach(var duplicate in duplicates)
                    {
                        Log.Error($"AssetBundle文件名称 '{duplicate}' 有重复项!");
                    }
                
                    hasDuplicate = true;
                }

            }

            if (hasDuplicate)
            {
                throw new System.Exception("存在重复项.中止打包");
            }


            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
#if TUANJIE_WEIXINMINIGAME
            if (buildTarget == BuildTarget.WeixinMiniGame)
            {
                buildTarget = BuildTarget.WebGL;
            }
#endif
            var buildParams = new BundleBuildParameters(buildTarget, EditorUserBuildSettings.selectedBuildTargetGroup, outputPath);
            buildParams.BundleCompression = compression;
            //该参数有一定风险可能会导致哈希碰撞,但是会提高5-10%的加载性能
            buildParams.ContiguousBundles = true;
            if (Application.isBatchMode)
            {
                //设置这个参数可以减少包体的大小并提高性能,但是升级Unity版本会导致AB需要重新全部下载
                //对于Jenkins等特殊构建环境我们启用DisableWriteTypeTree可以提升真机性能
                buildParams.ContentBuildFlags = ContentBuildFlags.DisableWriteTypeTree;
            }

            if(BuildPreference.Instance.AppendHash)
                buildParams.AppendHash = true;
#if WECHAT
            buildParams.UseCache = false;
#endif
            var exitCode = ContentPipeline.BuildAssetBundles(buildParams, new BundleBuildContent(file), out var result, buildTasks);
            if (exitCode < ReturnCode.Success)
            {
                throw new System.Exception("SBP Error " + exitCode);
            }
        }

        private static IList<IBuildTask> RuntimeDataBuildTasks()
        {
            var buildTasks = new List<IBuildTask>();

            // Setup
            //buildTasks.Add(new SwitchToBuildPlatform());
            //buildTasks.Add(new RebuildSpriteAtlasCache());

            // Player Scripts
            //buildTasks.Add(new BuildPlayerScripts());

            // Dependency
            buildTasks.Add(new CalculateSceneDependencyData());
            buildTasks.Add(new CalculateCustomDependencyData());
            buildTasks.Add(new CalculateAssetDependencyData());
            buildTasks.Add(new StripUnusedSpriteSources());
            buildTasks.Add(new CreateBuiltInShadersBundle(AssetBundleBuilder.BuiltInShaderBundleName));
            buildTasks.Add(new CreateMonoScriptBundle(AssetBundleBuilder.BuiltInMonoScriptBundleName));
// #if UNITY_2022_2_OR_NEWER
            // Packing
            // buildTasks.Add(new ClusterBuildLayout());
            // buildTasks.Add(new PostPackingCallback());
// #else
            // Packing
            buildTasks.Add(new GenerateBundlePacking());
            buildTasks.Add(new UpdateBundleObjectLayout());
            buildTasks.Add(new GenerateBundleCommands());
            buildTasks.Add(new GenerateSubAssetPathMaps());
            buildTasks.Add(new GenerateBundleMaps());
            buildTasks.Add(new PostPackingCallback());
// #endif
            
            // Writing
            buildTasks.Add(new WriteSerializedFiles());
            buildTasks.Add(new ArchiveAndCompressBundles());

            //XAssetsFramework Need
            buildTasks.Add(new DeleteUnusedAssetBundle());
            buildTasks.Add(new AppendBundleHash());
            buildTasks.Add(new GenerateFileLog());
            buildTasks.Add(new GenerateLinkXml());
            buildTasks.Add(new PostWritingCallback());
            
            return buildTasks;
        }
    }
}