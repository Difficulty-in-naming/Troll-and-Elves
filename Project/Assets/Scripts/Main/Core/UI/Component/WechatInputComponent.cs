#if WECHAT
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using WeChatWASM;

// 添加 InputField 组件的依赖
namespace EdgeStudio.UI.Component
{
    [RequireComponent(typeof(TMP_InputField))]
    public class WechatInputComponent : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
    {
        private TMP_InputField _inputField;
        private bool _isShowKeyboard = false;

        private void Start()
        {
            _inputField = GetComponent<TMP_InputField>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ShowKeyboard();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_inputField.isFocused)
            {
                HideKeyboard();
            }
        }

        private void OnInput(OnKeyboardInputListenerResult v)
        {
            if (_inputField.isFocused)
            {
                _inputField.text = v.value;
            }
        }

        private void OnConfirm(OnKeyboardInputListenerResult v)
        {
            HideKeyboard();
        }

        private void OnComplete(OnKeyboardInputListenerResult v)
        {
            HideKeyboard();
        }

        private void ShowKeyboard()
        {
            if (_isShowKeyboard) return;
        
            WX.ShowKeyboard(new ShowKeyboardOption()
            {
                defaultValue = "",
                maxLength = 9999,
                confirmType = "确认"
            });

            //绑定回调
            WX.OnKeyboardConfirm(this.OnConfirm);
            WX.OnKeyboardComplete(this.OnComplete);
            WX.OnKeyboardInput(this.OnInput);
            _isShowKeyboard = true;
        }

        private void HideKeyboard()
        {
            if (!_isShowKeyboard) return;
        
            WX.HideKeyboard(new HideKeyboardOption());
            //删除掉相关事件监听
            WX.OffKeyboardInput(this.OnInput);
            WX.OffKeyboardConfirm(this.OnConfirm);
            WX.OffKeyboardComplete(this.OnComplete);
            _isShowKeyboard = false;
        }
    }
}
#endif