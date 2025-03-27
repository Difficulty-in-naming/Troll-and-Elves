using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EdgeStudio.UI.Component
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class BetterContentSizeFitter : UIBehaviour, ILayoutSelfController
    {
        /// <summary> 可用的尺寸适应模式。 </summary>
        public enum FitMode
        {
            /// <summary> 不执行任何尺寸调整。 </summary>
            Unconstrained = 0,

            /// <summary> 调整到内容的最小尺寸。 </summary>
            MinSize,

            /// <summary> 调整到内容的优选尺寸。 </summary>
            PreferredSize,

            /// <summary> 调整到内容的优选尺寸，并应用尺寸限制。 </summary>
            LimitedPreferredSize
        }

        private Vector2 mContentSize = Vector2.zero;
        [SerializeField] private RectOffset m_Padding = new RectOffset();

        /// <summary> 用于控制上下左右的间距。 </summary>
        public RectOffset Padding
        {
            get => m_Padding;
            set
            {
                if (SetPropertyUtility.SetClass(ref m_Padding, value)) SetDirty();
            }
        }

        [SerializeField] private RectTransform m_ReferenceTransform = null;

        [SerializeField] private FitMode m_HorizontalFit = FitMode.Unconstrained;

        [SerializeField] private float m_HorizontalMinLimit = 100;
        [SerializeField] private float m_HorizontalMaxLimit = 200;

        [SerializeField] private FitMode m_VerticalFit = FitMode.Unconstrained;

        [SerializeField] private float m_VerticalMinLimit = 100;
        [SerializeField] private float m_VerticalMaxLimit = 200;

        [SerializeField] private bool m_ScaleAffectsPreferredSize = false; // 新增：是否考虑目标的 Scale
        [SerializeField] private bool useLayoutSize = true;
        /// <summary> 用于确定宽度的适应模式。 </summary>
        public FitMode HorizontalFit
        {
            get => m_HorizontalFit;
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_HorizontalFit, value)) SetDirty();
            }
        }

        /// <summary> 用于确定高度的适应模式。 </summary>
        public FitMode VerticalFit
        {
            get => m_VerticalFit;
            set
            {
                if (SetPropertyUtility.SetStruct(ref m_VerticalFit, value)) SetDirty();
            }
        }

        [System.NonSerialized] private RectTransform mRect;

        private RectTransform RectTransform
        {
            get
            {
                if (mRect == null)
                    mRect = GetComponent<RectTransform>();
                return mRect;
            }
        }

        private DrivenRectTransformTracker mTracker;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();

            mContentSize = m_ReferenceTransform.rect.size;
        }

        protected override void OnDisable()
        {
            mTracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
            base.OnDisable();
        }

        private void LateUpdate()
        {
            if (m_ReferenceTransform == null)
                return;

            Vector2 newSize = m_ReferenceTransform.rect.size;
            bool isDirty = false;
            if (HorizontalFit != FitMode.Unconstrained)
            {
                if (!Mathf.Approximately(mContentSize.x, newSize.x))
                    isDirty = true;
                mContentSize.x = newSize.x;
            }

            if (VerticalFit != FitMode.Unconstrained)
            {
                if (!Mathf.Approximately(mContentSize.y, newSize.y))
                    isDirty = true;
                mContentSize.y = newSize.y;
            }

            if (isDirty)
                this.SetDirty();
        }

        protected override void OnRectTransformDimensionsChange() => SetDirty();

        private void HandleSelfFittingAlongAxis(int axis)
        {
            FitMode fitting = (axis == 0 ? HorizontalFit : VerticalFit);
            if (fitting == FitMode.Unconstrained)
            {
                mTracker.Add(this, RectTransform, DrivenTransformProperties.None);
                return;
            }

            mTracker.Add(this, RectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

            if (m_ReferenceTransform == null) return;

            // 获取基础尺寸
            float baseSize = useLayoutSize
                ? GetLayoutSize(m_ReferenceTransform, axis, fitting)
                : (axis == 0 ? m_ReferenceTransform.rect.width : m_ReferenceTransform.rect.height);

            // 处理缩放影响
            if (m_ScaleAffectsPreferredSize)
            {
                float scale = (axis == 0 ? m_ReferenceTransform.localScale.x : m_ReferenceTransform.localScale.y);
                baseSize *= scale;
            }

            // 计算最终尺寸
            float finalSize = fitting switch
            {
                FitMode.MinSize => baseSize,
                FitMode.PreferredSize => baseSize,
                FitMode.LimitedPreferredSize => CalculateAdjustedSize(axis, baseSize),
                _ => 0
            };

            // 应用尺寸（包含padding）
            RectTransform.SetSizeWithCurrentAnchors(
                (RectTransform.Axis)axis,
                finalSize + (axis == 0 ? m_Padding.horizontal : m_Padding.vertical)
            );
        }
        
        private float CalculateAdjustedSize(int axis, float baseSize)
        {
            float minLimit = axis == 0 ? m_HorizontalMinLimit : m_VerticalMinLimit;
            float maxLimit = axis == 0 ? m_HorizontalMaxLimit : m_VerticalMaxLimit;
            float paddingForAxis = axis == 0 ? m_Padding.horizontal : m_Padding.vertical;

            // 调整限制范围（减去padding）
            float adjustedMin = minLimit - paddingForAxis;
            float adjustedMax = maxLimit - paddingForAxis;

            // 确保有效范围
            adjustedMin = Mathf.Max(adjustedMin, 0);
            adjustedMax = Mathf.Max(adjustedMax, adjustedMin);

            return Mathf.Clamp(baseSize, adjustedMin, adjustedMax);
        }

        private float GetLayoutSize(RectTransform target, int axis, FitMode fitMode)
        {
            return fitMode switch
            {
                FitMode.MinSize => LayoutUtility.GetMinSize(target, axis),
                FitMode.PreferredSize => LayoutUtility.GetPreferredSize(target, axis),
                FitMode.LimitedPreferredSize => LayoutUtility.GetPreferredSize(target, axis),
                _ => 0
            };
        }

        /// <summary>
        /// 获取 PreferredSize，即使组件被禁用也可以调用。
        /// 该方法的结果与 HandleSelfFittingAlongAxis 的计算逻辑一致。
        /// </summary>
        /// <returns>返回 PreferredSize 的 Vector2 值。</returns>
        public Vector2 GetPreferredSize()
        {
            if (m_ReferenceTransform == null)
                return Vector2.zero;

            Vector2 preferredSize = Vector2.zero;

            for (int axis = 0; axis < 2; axis++)
            {
                FitMode fitting = (axis == 0 ? HorizontalFit : VerticalFit);
                if (fitting == FitMode.Unconstrained) continue;

                float baseSize = useLayoutSize ? 
                    GetLayoutSize(m_ReferenceTransform, axis, fitting) : 
                    (axis == 0 ? m_ReferenceTransform.rect.width : m_ReferenceTransform.rect.height);

                float padding = (axis == 0 ? m_Padding.horizontal : m_Padding.vertical);

                if (m_ScaleAffectsPreferredSize)
                {
                    float scale = (axis == 0 ? 
                        m_ReferenceTransform.localScale.x : 
                        m_ReferenceTransform.localScale.y);
                    baseSize *= scale;
                }

                float finalSize = fitting switch
                {
                    FitMode.MinSize => baseSize,
                    FitMode.PreferredSize => baseSize,
                    FitMode.LimitedPreferredSize => CalculateAdjustedSize(axis, baseSize),
                    _ => 0
                };

                if (axis == 0)
                    preferredSize.x = finalSize + (fitting == FitMode.LimitedPreferredSize ? 0 : m_Padding.horizontal);
                else
                    preferredSize.y = finalSize + (fitting == FitMode.LimitedPreferredSize ? 0 : m_Padding.vertical);
            }

            return preferredSize;
        }

        /// <summary> 计算并应用水平方向的尺寸到 RectTransform。 </summary>
        public void SetLayoutHorizontal()
        {
            mTracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }

        /// <summary> 计算并应用垂直方向的尺寸到 RectTransform。 </summary>
        public void SetLayoutVertical()
        {
            HandleSelfFittingAlongAxis(1);
        }

        private void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate() => SetDirty();
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BetterContentSizeFitter), true)]
    [CanEditMultipleObjects]
    public sealed class NestedContentSizeFitterEditor : SelfControllerEditor
    {
        SerializedProperty m_ReferenceTransform;
        SerializedProperty m_HorizontalFit;
        SerializedProperty m_HorizontalMinLimit;
        SerializedProperty m_HorizontalMaxLimit;
        SerializedProperty m_VerticalFit;
        SerializedProperty m_VerticalMinLimit;
        SerializedProperty m_VerticalMaxLimit;
        SerializedProperty m_Padding;
        SerializedProperty m_ScaleAffectsPreferredSize; // 新增：是否考虑目标的 Scale
        SerializedProperty m_UseLayoutSize; // 新增：是否考虑目标的 Scale

        private void OnEnable()
        {
            m_ReferenceTransform = serializedObject.FindProperty("m_ReferenceTransform");
            m_HorizontalFit = serializedObject.FindProperty("m_HorizontalFit");
            m_HorizontalMinLimit = serializedObject.FindProperty("m_HorizontalMinLimit");
            m_HorizontalMaxLimit = serializedObject.FindProperty("m_HorizontalMaxLimit");
            m_VerticalFit = serializedObject.FindProperty("m_VerticalFit");
            m_VerticalMinLimit = serializedObject.FindProperty("m_VerticalMinLimit");
            m_VerticalMaxLimit = serializedObject.FindProperty("m_VerticalMaxLimit");
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_ScaleAffectsPreferredSize = serializedObject.FindProperty("m_ScaleAffectsPreferredSize"); // 新增
            m_UseLayoutSize = serializedObject.FindProperty("useLayoutSize"); // 新增

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_ReferenceTransform, true);
            EditorGUILayout.PropertyField(m_Padding, true); // 直接使用 Unity 的默认 RectOffset 绘制

            if (m_ReferenceTransform.objectReferenceValue == null)
                EditorGUILayout.HelpBox($"对于 {nameof(BetterContentSizeFitter.FitMode.PreferredSize)} 或 {nameof(BetterContentSizeFitter.FitMode.LimitedPreferredSize)}，需要一个 {nameof(RectTransform)}", MessageType.Warning);
            EditorGUILayout.PropertyField(m_HorizontalFit, true);
            if (m_HorizontalFit.enumValueIndex == 3)
            {
                EditorGUILayout.PropertyField(m_HorizontalMinLimit, true);
                EditorGUILayout.PropertyField(m_HorizontalMaxLimit, true);
            }
            EditorGUILayout.PropertyField(m_VerticalFit, true);
            if (m_VerticalFit.enumValueIndex == 3)
            {
                EditorGUILayout.PropertyField(m_VerticalMinLimit, true);
                EditorGUILayout.PropertyField(m_VerticalMaxLimit, true);
            }
            EditorGUILayout.PropertyField(m_ScaleAffectsPreferredSize, new GUIContent("Scale Affects Preferred Size"));
            EditorGUILayout.PropertyField(m_UseLayoutSize, new GUIContent("use Layout Size"));
            serializedObject.ApplyModifiedProperties();

            // base.OnInspectorGUI();
        }
    }
#endif

    public static class SetPropertyUtility
    {
        public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
                return false;

            currentValue = newValue;
            return true;
        }
        
        public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return false;

            currentValue = newValue;
            return true;
        }
    }
}