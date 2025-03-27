using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Panthea.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Panthea.Asset
{
    public class ResPak
    {
        private class LoadingObj
        {
            public Type Type { get; set; }
            public bool IsComplete { get; set; }
            public LoadingObj(Type type) => Type = type;
        }
        private AssetBundle AssetBundle { get; }
        /// <summary>
        /// 只是卸载了Bundle但是资源没有卸载
        /// </summary>
        public bool IsUnloadBundle;
        /// <summary>
        /// 资源和Bundle全卸载了
        /// </summary>
        public bool IsDisposed;
        public string Name { get; }
        private Dictionary<string, List<object>> mLoadedObjects = new();
        public AssetFileLog FileLog { get; set; }
        public Dictionary<object,int> ReferenceCount = new Dictionary<object, int>();
        public bool AutoDispose { get; set; } = true;
        private Dictionary<string, ListPool<LoadingObj>> LoadingList = new();
        private readonly int mBundleHashCode;
        public ResPak(AssetBundle assetBundle, AssetFileLog fileLog)
        {
            AssetBundle = assetBundle;
            mBundleHashCode = assetBundle.GetHashCode();
            Name = assetBundle.name;
            FileLog = fileLog;
            //todo 检查assetbundle
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddReferenceCount(object obj)
        {
            // todo 这里应该把GameObject引进来.
            if (!ReferenceCount.TryAdd(obj, 1))
            {
                ReferenceCount[obj]++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SubReferenceCount(object obj)
        {
            if (ReferenceCount.ContainsKey(obj))
            {
                var result = ReferenceCount[obj] -= 1;
                if (result <= 0)
                {
                    ReferenceCount.Remove(obj);
                }
            }
            
            if (ReferenceCount.Count == 0 && AutoDispose)
            {
                AssetsKit.Inst.UnloadAssetBundle(this,true);
            }
        }

        public void Unload(bool deepUnload)
        {
            AssetBundle.Unload(deepUnload);
            if (deepUnload)
            {
                mLoadedObjects.Clear();
                FileLog = null;
            }
        }

        private void AddCache(string name, Object obj)
        {
            if (!mLoadedObjects.TryGetValue(name, out var list))
            {
                list = new List<object>();
                mLoadedObjects.Add(name, list);
            }

            list.Add(obj);
        }

        public override bool Equals(object obj)
        {
            if (obj is not ResPak target)
            {
                return false;
            }

            return mBundleHashCode == target.mBundleHashCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => mBundleHashCode;

        public async UniTask<object> Load(string name, Type type)
        {
            Type mapType = type;
            if (type == typeof(AssetObject))
            {
                mapType = typeof(GameObject);
            }

            if (LoadingList.TryGetValue(name, out var list))
            {
                for (var index = list.Count - 1; index >= 0; index--)
                {
                    var node = list[index];
                    if (node.Type == mapType)
                    {
                        while (!node.IsComplete)
                        {
                            await UniTask.NextFrame();
                        }
                    }
                }
            }
            if (mLoadedObjects.TryGetValue(name, out var obj))
            {
                if (obj != null)
                {
                    foreach (var node in obj)
                    {
                        var o = node.GetType() == mapType;
                        if (o)
                        {
                            if(node is GameObject go)
                                return new AssetObject(go,this);
                            return node;
                        }
                    }
                }
            }

            var loadingObj = new LoadingObj(mapType);
            if (list == null)
            {
                LoadingList.Add(name, list = ListPool<LoadingObj>.Create());
            }

            list.Add(loadingObj);
            if (IsUnloadBundle)
            {
                throw new AssetBundleAlreadyUnload($"加载资源[{name}]失败,你已经卸载了AB[{Name}]");
            }
            var result = await AssetBundle.LoadAssetAsync(name, mapType);
            if (result == null)
            {
                throw new System.Exception(name + "没有在" + Name + "中被找到.这个AssetBundle和Filelog不匹配.请重新生成AB,以避免后续使用发生异常");
            }
            
            if (type == typeof(GameObject))
            {
                throw new System.Exception($"你无法通过{name}加载类型为GameObject的实例,请使用AssetObject代替类型加载实例");
            }

            AddCache(name, result);
            loadingObj.IsComplete = true;
            list.Remove(loadingObj);
            if (list.Count == 0)
            {
                list.Dispose();
                LoadingList.Remove(name);
            }
            
            if (type == typeof(AssetObject))
            {
                if(result is GameObject go)
                    return new AssetObject(go,this);
                throw new System.Exception($"你无法通过{name}加载类型为AssetObject的实例,因为对象实例的类型是{result.GetType()}");
            }
            
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask<T> Load<T>(string name) where T : Object => (T) await Load(name, typeof(T));
        
        public UniTask<Dictionary<string, List<object>>> LoadAll()
        {
            throw new System.Exception("禁止调用LoadAll,会严重导致内存问题和依赖引用出现问题");
            /*var files = FileLog.Files ?? Array.Empty<string>();
            Dictionary<string, List<object>> objects = new Dictionary<string, List<object>>(files.Length);
            bool needReload = false;
            //由于Unity官方并没有AssetBundle.LoadAllAsset<T>(string name)的接口.我们这里只能是先判断资源是否加载过.如果没有加载完全.我们在进行完全的加载
            for (var index = 0; index < files.Length; index++)
            {
                var node = files[index];
                if (mLoadedObjects.TryGetValue(node, out var value))
                {
                    objects[node] = value;
                }
                else
                {
                    needReload = true;
                    break;
                }
            }

            if (needReload)
            {
                Object[] assets;
                var loadAllAssets = AssetBundle.LoadAllAssetsAsync();
                await loadAllAssets;
                assets = loadAllAssets.allAssets;
                
                for (var index = 0; index < assets.Length; index++)
                {
                    var asset = assets[index];
                    var name = asset.name;
                    AddCache(name, asset);
                    objects[name] = mLoadedObjects[name];
                }
            }

            return objects;*/
        }
    }
}