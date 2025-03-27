using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Panthea.Asset;
using Panthea.Common;
using Panthea.Utils;
using R3;
using UnityEditor;
using UnityEngine;

namespace EdgeStudio.GamePlay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class AsyncSpriteRenderer : BetterMonoBehaviour
#if UNITY_EDITOR
        ,ISerializationCallbackReceiver
#endif
    {
        [BindField("_this")] public SpriteRenderer SpriteRenderer;
        public ReactiveProperty<string> SpriteName { get; private set; }
        private CancellationTokenSource mSource;
        [SerializeField,HideInInspector] private string DefaultSpritePath;
        void Awake()
        {
            SpriteName = new ReactiveProperty<string>(DefaultSpritePath);
            SpriteName.Subscribe(OnSpriteNameChanged).AddTo(this);
        }

        protected async void OnSpriteNameChanged(string spriteName)
        {
            try
            {
                if (string.IsNullOrEmpty(spriteName))
                {
                    SpriteRenderer.sprite = null;
                    return;
                }

                mSource?.Cancel();
                mSource = new CancellationTokenSource();
                var token = mSource.Token;
                try
                {
                    var sprite = await LoadSprite(spriteName, mSource.Token);
                    if (!this)
                        return;
                    if (!token.IsCancellationRequested)
                    {
                        SpriteRenderer.sprite = sprite;
                        mSource = null;
                    }
                }
                catch(Exception e)
                {
                    Log.Error(e);
                    mSource = null;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        protected async UniTask<Sprite> LoadSprite(string spriteName, CancellationToken token) => await AssetsKit.Inst.Load<Sprite>(spriteName).AttachExternalCancellation(token);

        void OnDestroy() => mSource?.Cancel();

#if UNITY_EDITOR
        void Reset() => SpriteRenderer = GetComponent<SpriteRenderer>();
        [NonSerialized] private Sprite _asset;
        public void OnBeforeSerialize()
        {
            DefaultSpritePath = null;
            if (SpriteRenderer && SpriteRenderer.sprite)
            {
                var path = AssetDatabase.GetAssetPath(SpriteRenderer.sprite);
                if (path.Contains(EDITOR_AssetsManager.SearchPath))
                {
                    path = PathUtils.RemoveFileExtension(path.Replace(EDITOR_AssetsManager.SearchPath, "")).ToLower();
                    DefaultSpritePath = path;
                }
                else
                {
                    throw new System.Exception(path + "没有放在Res AssetBundle目录下");
                }

                if (EDITOR_TEMPVAR.IsBuilding)
                {
                    _asset = SpriteRenderer.sprite;
                    SpriteRenderer.sprite = null;
                    EDITOR_TEMPVAR.RevertActions.Add(() =>
                    {
                        SpriteRenderer.sprite = _asset;
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
