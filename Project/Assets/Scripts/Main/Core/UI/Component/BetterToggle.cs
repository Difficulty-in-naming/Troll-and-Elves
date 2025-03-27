using System;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EdgeStudio.UI.Component
{
    [AddComponentMenu("UI/BetterToggle", 30)]
    [RequireComponent(typeof(RectTransform))]
    public class BetterToggle : Selectable, IPointerClickHandler, ISubmitHandler, ICanvasElement
    {
        [Serializable]
        public class ToggleEvent : UnityEvent<bool> {}

        [HideInInspector] public Toggle.ToggleTransition toggleTransition = Toggle.ToggleTransition.Fade;

        [HideInInspector] public Graphic graphic;

        [SerializeField, HideInInspector] private BetterToggleGroup m_Group;

        public BetterToggleGroup group
        {
            get { return m_Group; }
            set
            {
                SetToggleGroup(value, true);
                PlayEffect(true);
            }
        }

        [HideInInspector] public ToggleEvent onValueChanged = new ToggleEvent();

        [Tooltip("Is the toggle currently on or off?")]
        [SerializeField, HideInInspector]
        private bool m_IsOn;

        public bool isOn
        {
            get => m_IsOn;
            set => Set(value);
        }

        [SerializeField, ShowInInspector] public Graphic[] EnableDisplays;
        [SerializeField] public Graphic[] DisableDisplays;

        protected override void Start()
        {
            base.Start();
            PlayEffect(true);
            UpdateDisplayState(isOn);
        }
        
        private void UpdateDisplayState(bool on)
        {
            if (EnableDisplays != null)
            {
                foreach (var node in EnableDisplays)
                {
                    if (node)
                    {
                        var color = node.color;
                        color.a = on ? 1 : 0;
                        node.color = color;
                    }
                }
            }

            if (DisableDisplays != null)
            {
                foreach (var node in DisableDisplays)
                {
                    if (node)
                    {
                        var color = node.color;
                        color.a = on ? 0 : 1;
                        node.color = color;
                    }
                }
            }
        }

        #region Toggle 原始方法

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

#endif 

        public virtual void Rebuild(CanvasUpdate executing)
        {
#if UNITY_EDITOR
            if (executing == CanvasUpdate.Prelayout)
                onValueChanged.Invoke(m_IsOn);
#endif
        }

        public virtual void LayoutComplete()
        {}

        public virtual void GraphicUpdateComplete()
        {}

        protected override void OnDestroy()
        {
            if (m_Group != null)
                m_Group.EnsureValidState();
            base.OnDestroy();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetToggleGroup(m_Group, false);
            PlayEffect(true);
        }

        protected override void OnDisable()
        {
            SetToggleGroup(null, false);
            base.OnDisable();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            if (graphic != null)
            {
                bool oldValue = !Mathf.Approximately(graphic.canvasRenderer.GetColor().a, 0);
                if (m_IsOn != oldValue)
                {
                    m_IsOn = oldValue;
                    Set(!oldValue);
                }
            }

            base.OnDidApplyAnimationProperties();
        }

        private void SetToggleGroup(BetterToggleGroup newGroup, bool setMemberValue)
        {
            if (m_Group)
                m_Group.UnregisterToggle(this);

            if (setMemberValue)
                m_Group = newGroup;

            if (newGroup && IsActive())
                newGroup.RegisterToggle(this);
        }

        public void SetIsOnWithoutNotify(bool value)
        {
            Set(value, false);
        }
        
        public void SetIsOnWithoutNotifyGroup(bool value)
        {
            Set(value, false, false);
        }

        void Set(bool value, bool sendCallback = true,bool groupNotify = true)
        {
            m_IsOn = value;
            if (m_Group && groupNotify)
                m_Group.OnToggleValueChanged(this, value);
            PlayEffect(toggleTransition == Toggle.ToggleTransition.None);
            UpdateDisplayState(m_IsOn);
            if (sendCallback)
            {
                UISystemProfilerApi.AddMarker("Toggle.value", this);
                onValueChanged.Invoke(m_IsOn);
            }
        }

        private void PlayEffect(bool instant)
        {
            if (!graphic)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                graphic.canvasRenderer.SetAlpha(m_IsOn ? 1f : 0f);
            else
#endif
                graphic.CrossFadeAlpha(m_IsOn ? 1f : 0f, instant ? 0f : 0.1f, true);
        }

        private void InternalToggle()
        {
            if (!IsActive() || !IsInteractable())
                return;

            isOn = !isOn;
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            InternalToggle();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            InternalToggle();
        }
        #endregion
    }

    public static class BetterToggleExtensions
    {
        public static Observable<bool> OnValueChangedAsObservable(this BetterToggle toggle)
        {
            return Observable.Create<bool, BetterToggle>(toggle, static (observer, t) =>
            {
                observer.OnNext(t.isOn);
                return t.onValueChanged.AsObservable(t.destroyCancellationToken).Subscribe(observer);
            });
        }
    }
}