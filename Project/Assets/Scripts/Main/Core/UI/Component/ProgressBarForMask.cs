using System;
using Cysharp.Text;
using LitMotion;
using Panthea.Common;
using Panthea.Utils;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EdgeStudio.UI.Component
{
    [RequireComponent(typeof(RectMask2D)),ExecuteAlways]
    public class ProgressBarForMask : BetterMonoBehaviour
    {
        [SerializeField] private RectMask2D Mask2D;
        [SerializeField] private SerializableReactiveProperty<double> Value = new ();
        [SerializeField] private SerializableReactiveProperty<double> MinValue = new ();
        [SerializeField] private SerializableReactiveProperty<double> MaxValue = new ();
        public TextMeshProUGUI TargetText;
        public double TargetValue;
        private MotionHandle MotionHandle;
        private void Reset()
        {
            Mask2D = GetComponent<RectMask2D>();
        }
        
        public void SetValue(double value,float duration = 0)
        {
            if (TargetValue == value)
                return;
            TargetValue = value;
            if (duration != 0)
            {
                if(MotionHandle.IsActive())
                    MotionHandle.Cancel();
                MotionHandle = LMotion.Create(Value.Value, value, 0.1f).WithOnComplete(() => Value.Value = TargetValue = value).Bind(Value, (d, p) => p.Value = d).AddTo(this);
            }
            else
                Value.Value = value;
        }

        public void SetMaxValue(double value)
        {
            MaxValue.Value = value;
            Value.Value = Math.Clamp(Value.Value, MinValue.Value, MaxValue.Value);
        }

        public void SetMinValue(double value) => MinValue.Value = value;

        public bool IsMax() => Value.Value >= MaxValue.Value;

        void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            Value.Subscribe(OnValueChanged).AddTo(this);
            MinValue.Subscribe(OnValueChanged).AddTo(this);
            MaxValue.Subscribe(OnValueChanged).AddTo(this);
            TargetValue = Value.Value;
        }

        private void OnValueChanged(double value)
        {
            float remappedValue = (float)MathUtils.RemapClamped(Value.Value, MinValue.Value, MaxValue.Value, 0, 1);
            float actualWidth = RectTransform.rect.width; // Use rect.width instead of sizeDelta
            float rightPadding = actualWidth * (1 - remappedValue);
            Mask2D.padding = new Vector4(0, 0, rightPadding, 0);
            if (TargetText)
            {
                using var str = ZString.CreateStringBuilder();
                var value2 = Math.Round(Math.Clamp(Value.Value, MinValue.Value, MaxValue.Value));
                str.AppendFormat("{0}/{1}", value2, (int)MaxValue.Value);
                TargetText.SetText(str);
            }
        }
        
#if UNITY_EDITOR
        private void Update()
        {
            if (Value == null || MinValue == null || MaxValue == null) return;
            OnValueChanged(Value.Value);
        }
#endif
    }
}