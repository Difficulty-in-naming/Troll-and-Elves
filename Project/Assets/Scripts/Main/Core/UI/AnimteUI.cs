using System;
using Cysharp.Threading.Tasks;
using EdgeStudio.Manager.Panthea.UI;
using LitMotion;
using LitMotion.Adapters;
using LitMotion.Extensions;
using UnityEngine;

namespace EdgeStudio.UI
{


    public class AnimateUI
    {
        public AnimType EnterAnim { get; set; } = AnimType.None;
        public AnimType ExitAnim { get; set; } = AnimType.None;
        /// <summary>
        /// 正在播放动画
        /// </summary>
        public bool InAnimation
        {
            get;
            private set;
        }

        private UIBase mBase { get; }
        private int TweenCount { get; set; }
        public AnimateUI(UIBase @base)
        {
            mBase = @base;
        }

        public async UniTask WaitComplete()
        {
            while (InAnimation) await UniTask.NextFrame();
        }

        public void OnClose(bool forceDestroy = false)
        {
            mBase.OnFinishExitAnimation();
            UIKit.Inst.Destroy(mBase, forceDestroy);
            InAnimation = false;
        }

        public void OnCreate()
        {
            InAnimation = false;
            mBase.touchable = true;
            mBase.OnFinishPopAnimation();
        }

        internal void DoShowAnimation()
        {
            mBase.OnPreparePopAnimation();
            InAnimation = true;
            mBase.touchable = false;
            if (EnterAnim == AnimType.Custom)
            {
                mBase.DoShowAnimation();
            }
            else if(EnterAnim != AnimType.None)
            {
                PlayAnimation(EnterAnim,true, mBase.DoShowAnimation);
            }
            else
            {
                OnCreate();
            }
        }

        internal void DoHideAnimation()
        {
            if (!mBase)
            {
                OnClose();
                return;
            }
            mBase.OnPrepareExitAnimation();
            InAnimation = true;
            mBase.touchable = false;
            if (ExitAnim == AnimType.Custom)
            {
                mBase.DoHideAnimation();
            }
            else if(ExitAnim != AnimType.None)
            {
                PlayAnimation(ExitAnim,false, mBase.DoHideAnimation);
            }
            else
            {
                OnClose();
            }
        }
        
        private void PlayAnimation(AnimType type,bool show,Action callback)
        {
            float duration = 0.5f;
            if ((type & AnimType.Fall) != 0)
            {
                TweenCount++;
                if (show)
                {
                    mBase.localY = -UIKit.Inst.height;
                    LMotion.Create(mBase.localY, GetCenterPoint().y, duration).WithOnComplete(()=>TweenComplete(callback)).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                        .WithEase(Ease.OutBack).BindToAnchoredPositionY(mBase.RectTransform).AddTo(mBase.CachedGameObject);
                }
                else
                {
                    LMotion.Create(mBase.localY, UIKit.Inst.height, duration).WithOnComplete(()=>TweenComplete(callback)).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                        .WithEase(Ease.InBack).BindToAnchoredPositionY(mBase.RectTransform).AddTo(mBase.CachedGameObject);
                }
            }
            if ((type & AnimType.Rise) != 0)
            {
                TweenCount++;
                if (show)
                {
                    mBase.localY = UIKit.Inst.height;
                    LMotion.Create(mBase.localY, GetCenterPoint().y, duration).WithOnComplete(()=>TweenComplete(callback)).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                        .WithEase(Ease.OutBack).BindToAnchoredPositionY(mBase.RectTransform).AddTo(mBase.CachedGameObject);
                }
                else
                {
                    LMotion.Create(mBase.localY, -UIKit.Inst.height, duration).WithOnComplete(()=>TweenComplete(callback)).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                        .WithEase(Ease.InBack).BindToAnchoredPositionY(mBase.RectTransform).AddTo(mBase.CachedGameObject);
                }
            }
            if ((type & AnimType.FadeIn) != 0)
            {
                TweenCount++;
                mBase.alpha = 0;
                
                LMotion.Create(mBase.alpha, 1, duration).WithOnComplete(()=>TweenComplete(callback)).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithEase(Ease.OutQuad).BindToAlpha(mBase.CanvasGroup).AddTo(mBase.CachedGameObject);
            }
            if ((type & AnimType.FadeOut) != 0)
            {
                TweenCount++;
                LMotion.Create(mBase.alpha, 0, duration).WithOnComplete(()=>TweenComplete(callback)).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithEase(Ease.InQuad).BindToAlpha(mBase.CanvasGroup).AddTo(mBase.CachedGameObject);
            }
            if ((type & AnimType.ScaleToNormal) != 0)
            {
                TweenCount++;
                mBase.scale = Vector2.zero;
                mBase.SetPivot(0.5f,0.5f);
                
                LMotion.Create(Vector2.zero, Vector2.one, duration).WithOnComplete(()=>TweenComplete(callback)).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithEase(Ease.OutBack).BindToLocalScaleXY(mBase.CachedTransform).AddTo(mBase.CachedGameObject);
            }
            if ((type & AnimType.ScaleToZero) != 0)
            {
                TweenCount++;
                mBase.SetPivot(0.5f,0.5f);
                LMotion.Create(Vector2.one, Vector2.zero, duration).WithOnComplete(()=>TweenComplete(callback)).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .WithEase(Ease.InBack).BindToLocalScaleXY(mBase.CachedTransform).AddTo(mBase.CachedGameObject);
            }

            if ((type & AnimType.Jelly) != 0)
            {
                TweenCount++;
                mBase.SetPivot(0.5f,0.5f);
                mBase.scale = Vector2.zero;
                TweenScale2(Vector2.zero, new Vector2(1.0625f, 0.8875f),0.15f).WithOnComplete(() =>
                {
                    TweenScale2(new Vector2(1.0625f, 0.8875f), new Vector2(0.95f, 1.0227f),0.15f).WithOnComplete(() =>
                    {
                        TweenScale2(new Vector2(0.95f, 1.0227f), Vector2.one, 0.083f).WithOnComplete(() =>
                        {
                            TweenScale2(Vector2.one, new Vector2(1.0122f, 1), 0.1f).WithOnComplete(() =>
                            {
                                TweenScale2(new Vector2(1.0122f, 1), Vector2.one, 0.1f).WithOnComplete(() =>
                                {
                                    TweenComplete(callback);
                                }).BindToLocalScaleXY(mBase.CachedTransform).AddTo(mBase.CachedGameObject);
                            }).BindToLocalScaleXY(mBase.CachedTransform).AddTo(mBase.CachedGameObject);
                        }).BindToLocalScaleXY(mBase.CachedTransform).AddTo(mBase.CachedGameObject);
                    }).BindToLocalScaleXY(mBase.CachedTransform).AddTo(mBase.CachedGameObject);
                }).BindToLocalScaleXY(mBase.CachedTransform).AddTo(mBase.CachedGameObject);
            }
        }

        private void TweenComplete(Action action)
        {
            TweenCount--;
            if (TweenCount > 1)
                return;
            action?.Invoke();
        }

        private MotionBuilder<Vector2, NoOptions, Vector2MotionAdapter> TweenScale2(Vector2 from, Vector2 to, float duration)
        {
            return LMotion.Create(from, to, duration).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale).WithEase(Ease.Linear);
        }
    
        public Vector2 GetCenterPoint() => Vector2.zero;
    }
}