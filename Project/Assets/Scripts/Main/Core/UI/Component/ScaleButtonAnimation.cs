using LitMotion;
using LitMotion.Extensions;
using Panthea.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EdgeStudio.UI.Component
{
    public class ScaleButtonAnimation : BetterMonoBehaviour,IPointerDownHandler,IPointerExitHandler,IPointerUpHandler
    {
        private MotionHandle motion;
        private bool isDown = false;
        public Vector2 ScaleTo = new(1.15f,1.15f);
        public Vector2 ResetScale = Vector2.one;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if(motion.IsActive())
                motion.Cancel();
            isDown = true;
            motion = LMotion.Create(ResetScale, ScaleTo, 0.05f).BindToLocalScaleXY(CachedTransform).AddTo(CachedGameObject);
        }

        /// <summary>
        /// 这里注册Exit主要是为了防止出现意外导致Up不触发的情况
        /// 比如
        /// 点击按钮后暂停游戏移出鼠标点击屏幕一下.解除暂停则会导致Up不生效.在微信小游戏中这类第三方维护点击的情况有概率会发生这种情况
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isDown)
                return;
            if(motion.IsActive())
                motion.Cancel();
            isDown = false;
            motion = LMotion.Create(ScaleTo, ResetScale, 0.05f).BindToLocalScaleXY(CachedTransform).AddTo(CachedGameObject);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isDown)
                return;
            if(motion.IsActive())
                motion.Cancel();
            isDown = false;
            motion = LMotion.Create(ScaleTo, ResetScale, 0.05f).BindToLocalScaleXY(CachedTransform).AddTo(CachedGameObject);
        }
    }
}