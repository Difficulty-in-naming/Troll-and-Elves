using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Panthea.Asset;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace EdgeStudio.UI.Component
{
    public class AsyncImage : Image
    {
        public readonly ReactiveProperty<string> SpriteName = new("");
        [LabelText("使用图片自动大小")]public bool AutoSetNativeSize;
        private Material originMaterial;
        private CancellationTokenSource source;
        [FormerlySerializedAs("loadingMaterial")] [ReadOnly] public Material LoadingMaterial;

        protected override void Awake()
        {
            base.Awake();
            if (!Application.isPlaying)
                return;
            SpriteName.Skip(1).Subscribe(OnSpriteNameChanged).AddTo(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!Application.isPlaying)
                return;
            this.originMaterial = this.material;
        }

        protected async void OnSpriteNameChanged(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                this.material = originMaterial;
                this.sprite = null;
                this.color = Color.clear;
                return;
            }

            this.source?.Cancel();

            this.source = new CancellationTokenSource();
            var token = this.source.Token;
            try
            {
                this.material = LoadingMaterial;
                var sprite = await LoadSprite(spriteName, this.source.Token);
                if (!this)
                    return;
                if (!token.IsCancellationRequested)
                {
                    this.material = originMaterial;
                    this.sprite = sprite;
                    this.source = null;
                    this.color = Color.white;
                    if(AutoSetNativeSize)
                        this.SetNativeSize();
                }
            }
            catch
            {
                if (!token.IsCancellationRequested)
                {
                    this.material = originMaterial;
                    this.sprite = null;
                    this.source = null;
                    this.color = Color.clear;
                }
            }
        }

        protected async Task<Sprite> LoadSprite(string spriteName, CancellationToken token) => await AssetsKit.Inst.Load<Sprite>(spriteName).AttachExternalCancellation(token);

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.source?.Cancel();
        }
    }
}