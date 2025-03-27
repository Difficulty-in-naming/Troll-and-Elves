using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace EdgeStudio.UI.Component
{
    public class LongPressEventTrigger : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Tooltip("按钮被按下时，事件应该触发的频率（值越低，触发越频繁；值越高，触发越不频繁）")] public float Tick = 0.062f;
        [Space(10)] public UnityEvent OnLongPress = new();
        private bool _isPointerDown;
        protected override void Start() => EventUpdate().Forget();

        protected override void OnDisable()
        {
            base.OnDisable();
            _isPointerDown = false;
        }

        private async UniTask EventUpdate()
        {
            while (true)
            {
                if (!_isPointerDown)
                {
                    await UniTask.NextFrame();
                    continue;
                } 
                if (_isPointerDown) OnLongPress.Invoke();
                await UniTask.WaitForSeconds(Tick);
            }
        }

        public void OnPointerDown(PointerEventData eventData) => _isPointerDown = true;

        public void OnPointerUp(PointerEventData eventData) => _isPointerDown = false;

        public void OnPointerExit(PointerEventData eventData) => _isPointerDown = false;
    }
}