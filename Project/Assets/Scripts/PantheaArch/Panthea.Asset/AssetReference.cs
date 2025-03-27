using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Panthea.Utils;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Panthea.Asset
{
    [Serializable, HideLabel]
    public class AssetReference<T>
#if UNITY_EDITOR
        : ISerializationCallbackReceiver
#endif
        where T : UnityEngine.Object
    {
        [SerializeField, HideInInspector]private string Path;

        public string GetPath => Path;
        private UniTaskCompletionSource<T> tcs;
        public async UniTask<T> Load(CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(Path))
                return null;
            if (tcs != null)
                return await tcs.Task;
            tcs = new UniTaskCompletionSource<T>();
            if (typeof(T) == typeof(GameObject))
            {
                var result = await AssetsKit.Inst.Load<AssetObject>(Path, token);
                var target = (T)(object)result.GetGameObject();
                tcs.TrySetResult(target);
                return target;
            }
            else
            {
                var result = await AssetsKit.Inst.Load<T>(Path, token);
                tcs.TrySetResult(result);
                return result;
            }
        }

#if UNITY_EDITOR
        [SerializeField] private T Asset;
        [NonSerialized] private T _asset;
        public void OnBeforeSerialize()
        {
            Path = null;
            if (Asset)
            {
                var path = AssetDatabase.GetAssetPath(Asset);
                if (path.Contains(EDITOR_AssetsManager.SearchPath))
                {
                    path = PathUtils.RemoveFileExtension(path.Replace(EDITOR_AssetsManager.SearchPath, "")).ToLower();
                    Path = path;
                }
                else
                {
                    throw new System.Exception(path + "没有放在Res AssetBundle目录下");
                }

                if (EDITOR_TEMPVAR.IsBuilding)
                {
                    _asset = Asset;
                    Asset = null;
                    EDITOR_TEMPVAR.RevertActions.Add(() =>
                    {
                        Asset = _asset;
                    });
                }
            }
        }

        public void OnAfterDeserialize()
        {
        }
#endif
    }
}