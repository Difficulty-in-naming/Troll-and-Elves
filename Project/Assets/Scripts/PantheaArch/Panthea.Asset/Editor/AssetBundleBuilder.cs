using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Panthea.Asset.Define;
using Panthea.Asset.OneHeart;
using Panthea.Common;
using UnityEngine;

//using UnityEditor.AddressableAssets;
//using UnityEditor.AddressableAssets.Build;

namespace Panthea.Asset
{
    public class AssetBundleBuilder
    {
        private List<Type> mProcess = new List<Type>();
        private const string mPackPath = AssetsPackPath.Path;
        private Dictionary<string, object> mInject = new Dictionary<string, object>();
        public const string BuiltInShaderBundleName = "unitybuiltinshaders" + AssetsConfig.Suffix;
        public const string BuiltInMonoScriptBundleName = "monoscript" + AssetsConfig.Suffix;
        public async void Init()
        {
            //BuildScript.buildCompleted -= OnBuildCompleted;
            //BuildScript.buildCompleted += OnBuildCompleted;
            EDITOR_TEMPVAR.IsBuilding = true;
            mInject.Add("inject", mInject);
            mInject.Add("packPath", mPackPath);
            //mInject.Add("settings", AddressableAssetSettingsDefaultObject.GetSettings(true));

            //把Json配置表转换为二进制配置表
#if !USEJSON
            AddProcess<ConvertJsonConfigToBinaryConfig>();
#endif
            
            //收集所有的需要打包的文件
            AddProcess<CollectAllAssets>();
            //将上面收集到的文件路径全部转换为小写
            AddProcess<LowerCasePath>();
            //过滤掉一些不需要的文件
            AddProcess<IgnoreFile>();
            //清除掉TMP的缓存内容
            AddProcess<ClearFontData>();
            //过滤配置表目录下的所有Json文件
#if !USEJSON
            AddProcess<IgnoreConfigFolderAllJsonFile>();
#endif
            
            //压缩字体文件(壹心项目使用)
            AddProcess<CompressFont>();
            //将收集到的文件自动分为不同的Group,你可以在后续调整每个Group的参数.让某个Group使用不同的压缩或者分包
            AddProcess<BuildGroup>();
            //打包内容
            AddProcess<BuildContent>();
            
            //最后把二进制表删掉.还原回正常项目
#if !USEJSON
            AddProcess<DeleteBinaryConfig>();
#endif
            //压缩内容
            //AddProcess<ZipAssets>();
            //提交服务器
            // AddProcess<UploadTencentCos>();
            await DoPipeline();
            EDITOR_TEMPVAR.IsBuilding = false;
            foreach (var node in EDITOR_TEMPVAR.RevertActions)
            {
                try
                {
                    node.Invoke();
                }
                catch(System.Exception e)
                {
                    Log.Error(e);
                    continue;
                }
            }
            EDITOR_TEMPVAR.RevertActions.Clear();
        }

        /*private void OnBuildCompleted(AddressableAssetBuildResult result)
        {
            if (!string.IsNullOrEmpty(result.Error))
            {
                throw new Exception(result.Error);
            }
        }*/

        public static void Pack()
        {
            var builder = new AssetBundleBuilder();
            builder.Init();
        }

        private void AddProcess<T>()
        {
            mProcess.Add(typeof (T));
        }

        public async Task DoPipeline()
        {
            List<AResPipeline> pipelines = new List<AResPipeline>();
            try
            {
                for (var index = 0; index < mProcess.Count; index++)
                {
                    var node = mProcess[index];
                    var constructor = node.GetConstructors()[0];
                    var paramters = constructor.GetParameters();
                    List<object> args = new List<object>();
                    foreach (var p in paramters)
                    {
                        foreach (var inject in mInject)
                        {
                            if (inject.Value.GetType() == p.ParameterType)
                            {
                                args.Add(inject.Value);
                            }
                        }
                    }

                    AResPipeline pipeline = constructor.Invoke(args.ToArray()) as AResPipeline;
                    if (pipeline == null)
                    {
                        Debug.LogError(node + "没有继承自 AResPipeline");
                        continue;
                    }
                    pipelines.Add(pipeline);
                    await pipeline.Do();
                }

                Debug.Log("打包完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError("打包失败,可能是场景没有保存,具体错误看下面内容");
                Debug.LogError(e);
                for (var index = 0; index < pipelines.Count; index++)
                {
                    try
                    {
                        var node = pipelines[index];
                        await node.Rollback();
                    }
                    catch(System.Exception ex)
                    {
                        Debug.LogError(ex);
                        continue;
                    }
                }
            }
            finally
            {
                for (var index = 0; index < pipelines.Count; index++)
                {
                    try
                    {
                        var node = pipelines[index];
                        await node.OnComplete();
                    }
                    catch(System.Exception ex)
                    {
                        Debug.LogError(ex);
                        continue;
                    }
                }
            }
        }
    }
}