using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Panthea.Common;
using R3;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace EdgeStudio.UI
{
    [Preserve]
    [RequireComponent(typeof(Canvas),typeof(GraphicRaycaster))]
    public class UIBase : BetterMonoBehaviour
    {
        [BindField("_This")]public new GameObject CachedGameObject;
        [BindField("_This")]public new RectTransform CachedTransform;
        [BindField]public Button CloseBtn;
        [BindField("_This")]public Canvas Canvas;
        [BindField("_This")]public GraphicRaycaster Raycaster;
        private CanvasGroup mCanvasGroup;

        public void Reset()
        {
            CachedGameObject = gameObject;
            CachedTransform = (RectTransform)transform;
            Canvas = GetComponent<Canvas>();
            Raycaster = GetComponent<GraphicRaycaster>();
        }
        
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (!mCanvasGroup)
                {
                    mCanvasGroup = CachedGameObject.GetComponent<CanvasGroup>();
                    if (!mCanvasGroup)
                        mCanvasGroup = CachedGameObject.AddComponent<CanvasGroup>();
                }

                return mCanvasGroup;
            }
        }

        public string Name => CachedGameObject.name;
        internal UIWidget Widget { get; set; }
        internal Type Type { get; set; }
        private Subject<UIBase> mOnClose { get; set; }

        /// 当前界面是否激活.
        public bool Active
        {
            get => CachedGameObject && CachedGameObject.activeSelf;
            set => CachedGameObject.SetActive(value);
        }

        public bool IsLoading { get; set; }

        /// 界面已经被销毁了.
        public bool IsDisposed => !CachedGameObject;

        public Observable<UIBase> OnClose => mOnClose ??= new Subject<UIBase>();

        public bool visible
        {
            get => CachedGameObject && Canvas.enabled;
            set => Canvas.enabled = value;
        }

        public float x
        {
            get => CachedTransform.position.x;
            set => CachedTransform.position = new Vector3(value, CachedTransform.position.y, CachedTransform.position.z);
        }
        
        public float y
        {
            get => CachedTransform.position.y;
            set => CachedTransform.position = new Vector3(CachedTransform.position.x, value, CachedTransform.position.z);
        }
        
        public float z
        {
            get => CachedTransform.position.z;
            set => CachedTransform.position = new Vector3(CachedTransform.position.x, CachedTransform.position.y, value);
        }

        public float localX
        {
            get => CachedTransform.anchoredPosition3D.x;
            set => CachedTransform.anchoredPosition3D = new Vector3(value, CachedTransform.anchoredPosition3D.y, CachedTransform.anchoredPosition3D.z);
        }
        
        public float localY
        {
            get => CachedTransform.anchoredPosition3D.y;
            set => CachedTransform.anchoredPosition3D = new Vector3(CachedTransform.anchoredPosition3D.x, value, CachedTransform.anchoredPosition3D.z);
        }
        
        public float localZ
        {
            get => CachedTransform.anchoredPosition3D.z;
            set => CachedTransform.anchoredPosition3D = new Vector3(CachedTransform.anchoredPosition3D.x, CachedTransform.anchoredPosition3D.y, value);
        }
        
        public Vector3 position
        {
            get => CachedTransform.position;
            set => CachedTransform.position = value;
        }

        public float width
        {
            get => CachedTransform.sizeDelta.x;
            set => CachedTransform.sizeDelta = new Vector2(value, CachedTransform.sizeDelta.y);
        }
        
        public float height
        {
            get => CachedTransform.sizeDelta.x;
            set => CachedTransform.sizeDelta = new Vector2(CachedTransform.sizeDelta.x, value);
        }

        public Vector2 size
        {
            get => CachedTransform.sizeDelta;
            set => CachedTransform.sizeDelta = value;
        }

        public float alpha
        {
            get => CanvasGroup.alpha;
            set => CanvasGroup.alpha = value;
        }
        
        public bool touchable
        {
            get => Raycaster.enabled;
            set => Raycaster.enabled = value;
        }
        
        public Vector2 scale
        {
            get => CachedTransform.localScale;
            set => CachedTransform.localScale = value;
        }
        public float scaleX
        {
            get => CachedTransform.localScale.x;
            set => CachedTransform.localScale = new Vector2(value, CachedTransform.localScale.y);
        }
        public float scaleY
        {
            get => CachedTransform.localScale.y;
            set => CachedTransform.localScale = new Vector2(CachedTransform.localScale.x, value);
        }
        
        public void SetPivot(Vector2 pivot,bool asAnchor = false)
        {
            CachedTransform.pivot = pivot;
            if (asAnchor)
            {
                CachedTransform.anchorMin = pivot;
                CachedTransform.anchorMax = pivot;
            }
        }

        public void MakeFullScreen()
        {
            CachedTransform.anchorMin = Vector2.zero;
            CachedTransform.anchorMax = Vector2.one;
            CachedTransform.offsetMax = CachedTransform.offsetMin = Vector2.zero;
        }
        
        public void SetPivot(float x, float y,bool asAnchor = false) => SetPivot(new Vector2(x, y), asAnchor);

        public AnimateUI Animate { get; set; }

        public void SetAtTop()
        {
            CachedTransform.SetAsLastSibling();
        }

        #region virtual

        /// 仅当UI界面被创建的时候触发
        [RequiredMember]
        protected virtual void OnInit<T>(T param)
        {
        }

        /// 当UI界面被销毁时触发
        [RequiredMember]
        protected virtual void OnRelease()
        {
        }

        /// 当UI界面处于活跃状态时调用一次在OnInit之后触发
        [RequiredMember]
        protected virtual void OnActivate<T>(T param, bool refresh)
        {
        }

        /// 当UI界面处于非活跃状态是调用一次在OnClose之前触发
        [RequiredMember]
        protected virtual void OnDeactivate(bool refresh)
        {
        }

        /// 当动画播放前的时候触发
        [RequiredMember]
        protected internal virtual void OnPreparePopAnimation()
        {
        }

        /// 当动画播放前的时候触发
        [RequiredMember]
        protected internal virtual void OnPrepareExitAnimation()
        {
        }

        /// 当UIWindow标记Animation为Custom的时候调用该函数
        [RequiredMember]
        protected internal virtual void DoShowAnimation()
        {
            Animate.OnCreate();
        }

        /// 当UIWindow标记Animation为Custom的时候调用该函数
        [RequiredMember]
        protected internal virtual void DoHideAnimation()
        {
            Animate.OnClose();
        }

        /// 当动画播放结束的时候触发
        [RequiredMember]
        protected internal virtual void OnFinishPopAnimation()
        {
        }

        /// 当动画播放结束的时候触发
        [RequiredMember]
        protected internal virtual void OnFinishExitAnimation()
        {
        }

        /// 当界面处在最上层的时候被响应
        [RequiredMember]
        protected internal virtual void OnFocus()
        {
        }

        /// 当界面从最上层移出时响应
        [RequiredMember]
        protected internal virtual void OnLostFocus()
        {
        }

        /// <summary> 准备资源 </summary>
        [RequiredMember]
        protected internal virtual UniTask PrepareAsset<T>(T param) => UniTask.CompletedTask;

        [RequiredMember]
        protected internal virtual ValueTask CloseBtnOnClicked(Unit unit, CancellationToken cancellationToken) => CloseMySelf();
        #endregion

        #region Base

        internal void BaseStart<T>(T args)
        {
            AutoSet();
            OnInit(args);
        }

        internal void BaseEnable<T>(T args)
        {
            bool refresh = Active == false;
            Active = true;
            OnActivate(args, refresh);
        }

        internal void BaseDisable()
        {
            bool refresh = Active;
            Active = false;
            //RemoveAllEvent();//暂时没有用到.我们删除掉避免无意义的性能消耗
            OnDeactivate(refresh);
            if (Widget.Pool)
            {
                mOnClose?.OnNext(this);
            }
        }

        internal void BaseDestroy()
        {
            if (!Widget.Pool) //不是池的话就不执行了因为是再同一帧里面执行的Disable和Destroy.切换场景的时候会调用强制销毁有可能事件会被执行两次发生错误
            {
                mOnClose?.OnNext(this);
            }

            OnRelease();
        }

        #endregion

        #region Method

        /// 销毁自身
        [RequiredMember]
        public virtual ValueTask CloseMySelf()
        {
            Animate?.DoHideAnimation();
            return new ValueTask();
        }

        #endregion

        public async UniTask WaitClose()
        {
            while (!IsDisposed)
            {
                await UniTask.NextFrame();
            }
        }
        
        private void AutoSet()
        {
            if (CloseBtn)
                CloseBtn.OnClickAsObservable().Where(_ => Animate.InAnimation == false).SubscribeAwait(CloseBtnOnClicked,AwaitOperation.Drop).AddTo(this);
        }

    }
}