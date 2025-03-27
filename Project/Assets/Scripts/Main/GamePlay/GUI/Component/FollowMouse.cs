using Panthea.Common;
using UnityEngine;

namespace EdgeStudio
{
    public class FollowMouse : BetterMonoBehaviour
    {
        public Canvas TargetCanvas;
        protected Vector2 _newPosition;
        protected Vector2 _mousePosition;

        void Start()
        {
            TargetCanvas = GetComponentInParent<Canvas>().rootCanvas;
        }
		
        protected virtual void LateUpdate()
        {
			_mousePosition = InputManager.Inst.MousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(TargetCanvas.transform as RectTransform, _mousePosition, TargetCanvas.worldCamera, out _newPosition);
            transform.position = TargetCanvas.transform.TransformPoint(_newPosition);
        }
    }
}
