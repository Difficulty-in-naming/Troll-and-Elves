using LitMotion;
using LitMotion.Extensions;
using Panthea.Common;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace EdgeStudio.UI.Component.Tween
{
    public class AlphaTween : BetterMonoBehaviour
    {
        public float DelayTime;
        public float Duration = 1;
        [SerializeField,HideIf(nameof(CanvasGroup))] private Graphic Graphic;
        [SerializeField,HideIf(nameof(Graphic))] private CanvasGroup CanvasGroup;
        void Start()
        {
            if (Graphic)
            {
                Graphic.color = new Color(Graphic.color.r, Graphic.color.g, Graphic.color.b, 0);
                LMotion.Create(0f, 1f, Duration).WithDelay(DelayTime).BindToColorA(Graphic).AddTo(this);
            }
            else if (CanvasGroup)
            {
                CanvasGroup.alpha = 0.4f;
                LMotion.Create(0.4f, 1f, Duration).WithDelay(DelayTime).BindToAlpha(CanvasGroup).AddTo(this);
            }
        }
        
#if UNITY_EDITOR
        void Reset()
        {
            Graphic = GetComponent<Graphic>();
            CanvasGroup = GetComponent<CanvasGroup>();
        }
#endif
    }
}
