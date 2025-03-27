using LitMotion;
using LitMotion.Extensions;
using Panthea.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EdgeStudio.UI.Component
{
    public class ButtonBreath : BetterMonoBehaviour,IPointerDownHandler,IPointerExitHandler,IPointerUpHandler
    {
        private MotionHandle handle;
        public Vector3 StartValue = Vector3.one;
        public Vector3 EndValue = new(1.05f,1.05f,1.05f);
        void Start() => PlayBreath();

        void PlayBreath() =>
            handle = LMotion.Create(StartValue, EndValue, 0.7F).WithLoops(-1, LoopType.Yoyo).WithEase(Ease.InOutSine)
                .BindToLocalScale(CachedTransform);

        void CancelHandle()
        {
            if(handle.IsActive()) handle.Cancel();
        }

        private void OnDestroy() => CancelHandle();

        public void OnPointerDown(PointerEventData eventData) => CancelHandle();

        public void OnPointerExit(PointerEventData eventData)
        {
            if(!handle.IsActive())
                PlayBreath();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if(!handle.IsActive())
                PlayBreath();
        }
    }
}