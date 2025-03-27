using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EdgeStudio.Config;
using EdgeStudio.UI;
using Newtonsoft.Json;
using Panthea.Asset;
using Panthea.Common;
using UnityEngine;

namespace EdgeStudio
{
    public class Entry : MonoBehaviour
    {
        public string Version = "1.0.0";
        private IABFileTrack mLocalTrack;
        async void Start()
        {
#if USEJSON || USEJSONFOREDITOR
            //加入AOT.
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<HashSet<int>>();
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = EdgeStudioJsonConverter.JsonConverters,
                DefaultValueHandling = DefaultValueHandling.Ignore, TypeNameHandling = TypeNameHandling.None,
            };
#endif
            //往GameSettings中注入版本号
            GameSettings.Version = Version;
            await ConfigureServices();
        }

        async UniTask ConfigureServices()
        {
            UIEvent.UpdateLoadingProgress.OnNext((0,"初始化资源加载"));
            await ConfigureAssetsLoader(); //初始化文件加载
            UIEvent.UpdateLoadingProgress.OnNext((10,"初始化游戏数据表"));
            await ConfigureConfigAsset(); //初始化数据表
            GameSettings.IsLoadingComplete = true;
        }
        
        async UniTask ConfigureAssetsLoader()
        {
            Log.Print("初始化AssetLoader中...");
            if (GameSettings.MobileRuntime)
            {
                //注册文件跟踪,正式版本中可能包体内和包体外都会存在AB.我们需要跟踪确认使用哪个路径下的AB
                mLocalTrack = new ABFileTrack($"all_local_file.bytes");
                //使AssetsManager支持下载功能(只是注册S3Download只是注册了服务,但是并没有开启下载功能)
                var abDownloader = new AssetBundleDownloader(mLocalTrack, null);
                //注册AssetBundle加载缓存池.加载过的物体会在池中缓存起来
                var pool = new AssetBundlePool(mLocalTrack, abDownloader);
                //注册运行时加载AB的支持模块
                var runtime = new AssetBundleRuntime(pool);
                //创建AssetManager,AssetManager可以理解为底层接口的一层包装
                var locator = new AssetsManager(mLocalTrack, runtime, abDownloader);
                await mLocalTrack.Initialize();
                AssetsKit.Inst = locator;
            }
            else
            {
#if UNITY_EDITOR
                //如果编辑器下面可以使用EDITOR_AssetManager.这个使用AssetDatabase加载资源.在编辑器下可以不需要打包,从而快速测试.
                AssetsKit.Inst = new EDITOR_AssetsManager();
#endif
            }

            Log.Print("初始化AssetLoader结束");
        }

        private async UniTask ConfigureConfigAsset()
        {
            Log.Print("初始化配置加载开始...");
            await ConfigLoader.LoadAllConfigs();
            Log.Print("初始化配置结束...");
        }
    }
}
