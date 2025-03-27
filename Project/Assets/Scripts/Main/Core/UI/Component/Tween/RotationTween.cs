using LitMotion;
using LitMotion.Extensions;
using Panthea.Common;

namespace EdgeStudio.UI.Component.Tween
{
    public class RotationTween : BetterMonoBehaviour
    {
        public float DelayTime;
        public float Duration = 1;
        void Start()
        {
            LMotion.Create(0f,360f, Duration).WithLoops(-1).WithDelay(DelayTime).BindToLocalEulerAnglesZ(RectTransform).AddTo(this);
        }
    }
}