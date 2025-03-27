using Cysharp.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EdgeStudio.UI.Component
{
    public class ProgressTextUI : MonoBehaviour
    {
        [SerializeField, BindField]private Image ProgressBar;
        [SerializeField, BindField]private TextMeshProUGUI Text;
        [SerializeField] private float mMax;
        [SerializeField] private float mValue;
        public float MaxValue
        {
            get => mMax;
            set
            {
                mMax = value;
                OnValueChanged(mValue);
            }
        }

        public float Value
        {
            get => mValue;
            set
            {
                mValue = value;
                OnValueChanged(value);
            }
        }

        private void OnValueChanged(float value)
        {
            using var str = ZString.CreateStringBuilder();
            str.Append(value);
            str.Append("/");
            str.Append(mMax);
            Text.SetText(str);
            ProgressBar.fillAmount = value / MaxValue;
        }
    }
}
