using Panthea.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace EdgeStudio.UI.Component
{
    [RequireComponent(typeof(RectTransform))]
    public class DragDropHelper : BetterMonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public UnityEvent<PointerEventData, DragDropHelper> OnBeginDragEvent;
        public UnityEvent<PointerEventData, DragDropHelper> OnDragEvent;
        public UnityEvent<PointerEventData, DragDropHelper> OnEndDragEvent;
        public bool IsDragging { get; private set; }

        [SerializeField] private bool shouldGenerateCurrentObjectForDrag = false;
        [SerializeField] private Transform parentForGeneratedObject;
        [SerializeField] private GameObject virtualTargetObject;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        public GameObject DraggedObject { get; private set; }
        private void Start()
        {
            _rectTransform = RectTransform;
            _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            IsDragging = true;

            if (shouldGenerateCurrentObjectForDrag)
            {
                DraggedObject = Instantiate(
                    virtualTargetObject ? virtualTargetObject : CachedGameObject,
                    RectTransform.position,
                    RectTransform.rotation,
                    parentForGeneratedObject ? parentForGeneratedObject : RectTransform.parent
                );

                var dragRectTransform = DraggedObject.GetComponent<RectTransform>();
                _rectTransform = dragRectTransform;
                _canvasGroup = DraggedObject.GetComponent<CanvasGroup>() ?? DraggedObject.AddComponent<CanvasGroup>();
            }

            if (_canvasGroup) _canvasGroup.blocksRaycasts = false;
            OnBeginDragEvent?.Invoke(eventData, this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (IsDragging && RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    _rectTransform, eventData.position, eventData.pressEventCamera, out var globalMousePos))
            {
                _rectTransform.position = globalMousePos;
            }

            OnDragEvent?.Invoke(eventData, this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IsDragging = false;
            if (_canvasGroup) _canvasGroup.blocksRaycasts = true;
            OnEndDragEvent?.Invoke(eventData, this);
        }
    }
}