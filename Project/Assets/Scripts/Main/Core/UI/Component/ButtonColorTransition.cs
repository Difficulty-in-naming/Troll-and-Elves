using Com.LuisPedroFonseca.ProCamera2D;
using EdgeStudio.Odin;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace EdgeStudio.UI.Component
{
    public class ButtonColorTransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        // 目标图像组件
        [FormerlySerializedAs("mTargetImage")] [ColorFoldout("目标设置"), SerializeField] 
        private Graphic Target;
        
        [FormerlySerializedAs("mNormalColor")] [ColorFoldout("颜色设置"), SerializeField] 
        private Color NormalColor = Color.white;
        [FormerlySerializedAs("mHoverColor")] [ColorFoldout("颜色设置"), SerializeField] 
        private Color HoverColor = new(0.9f, 0.9f, 0.9f);
        [FormerlySerializedAs("mPressedColor")] [ColorFoldout("颜色设置"), SerializeField] 
        private Color PressedColor = new(0.7f, 0.7f, 0.7f);

        [FormerlySerializedAs("mTransitionDuration")] [ColorFoldout("动画设置"), SerializeField] 
        private float TransitionDuration = 0.25f;
        [FormerlySerializedAs("mEaseType")] [ColorFoldout("动画设置"),SerializeField] private Ease EaseType = Ease.Linear;


        private bool mIsPointerDown = false;

        private MotionHandle? mCurrentMotion;

        private void Awake()
        {
            if (Target == null)
            {
                Debug.LogError("ButtonColorTransition: 找不到Image组件!");
            }

            // 初始化颜色
            if (Target != null)
            {
                NormalColor = Target.color;
            }
        }

        private void Start()
        {
            // 确保初始状态为正常颜色
            if (Target != null)
            {
                Target.color = NormalColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!mIsPointerDown)
            {
                TransitionToColor(HoverColor);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!mIsPointerDown)
            {
                TransitionToColor(NormalColor);
            }

            mIsPointerDown = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            mIsPointerDown = true;
            TransitionToColor(PressedColor);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            mIsPointerDown = false;
            // 如果鼠标仍在按钮上方，则变为悬停颜色，否则恢复正常颜色
            if (IsPointerOverGameObject())
            {
                TransitionToColor(HoverColor);
            }
            else
            {
                TransitionToColor(NormalColor);
            }
        }

        private bool IsPointerOverGameObject()
        {
            // 检查鼠标是否仍然在游戏对象上方
            return EventSystem.current.IsPointerOverGameObject();
        }

        private void TransitionToColor(Color targetColor)
        {
            if (Target == null) return;

            // 取消当前正在进行的动画
            if (mCurrentMotion.HasValue && mCurrentMotion.Value.IsActive())
            {
                mCurrentMotion.Value.Cancel();
            }

            // 使用LitMotion创建颜色过渡动画
            mCurrentMotion = LMotion
                .Create(Target.color, targetColor, TransitionDuration)
                .WithEase(EaseType)
                .BindToColor(Target)
                .AddTo(this);
        }

        // 提供公共方法用于外部调用
        public void SetNormalColor(Color color)
        {
            NormalColor = color;
            if (!mIsPointerDown && !IsPointerOverGameObject())
            {
                TransitionToColor(NormalColor);
            }
        }

        public void SetHoverColor(Color color)
        {
            HoverColor = color;
            if (!mIsPointerDown && IsPointerOverGameObject())
            {
                TransitionToColor(HoverColor);
            }
        }

        public void SetPressedColor(Color color)
        {
            PressedColor = color;
            if (mIsPointerDown)
            {
                TransitionToColor(PressedColor);
            }
        }

        public void SetTransitionDuration(float duration)
        {
            TransitionDuration = Mathf.Max(0.01f, duration);
        }

        public void SetEaseType(Ease ease)
        {
            EaseType = ease;
        }
    }
}
