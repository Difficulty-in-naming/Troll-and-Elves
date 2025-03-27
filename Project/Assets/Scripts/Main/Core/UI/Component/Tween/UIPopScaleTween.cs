using LitMotion;
using LitMotion.Extensions;
using Panthea.Common;
using UnityEngine;

namespace EdgeStudio.UI.Component.Tween
{
    public class UIPopScaleTween : BetterMonoBehaviour
    {
        public float Delay = 0.04f;
        public float Duration = 0.4f;
        public Ease Ease = Ease.OutBack;

        void Start()
        {
            CachedTransform.localScale = Vector3.zero;
            LMotion.Create(Vector3.zero, Vector3.one, Duration).WithDelay(Delay).WithEase(Ease).WithOnComplete(()=>
            {
                var com = GetComponent<IPopScaleTweenComplete>();
                if (com != null)
                {
                    com.ScaleTweenComplete();
                }
            }).BindToLocalScale(CachedTransform).AddTo(this);
        }
    }

    public interface IPopScaleTweenComplete
    {
        void ScaleTweenComplete();
    }
}
