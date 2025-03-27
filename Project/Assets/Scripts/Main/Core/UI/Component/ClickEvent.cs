using System;
using Panthea.Common;
using UnityEngine.EventSystems;

namespace EdgeStudio.UI.Component
{
    public class ClickEvent : BetterMonoBehaviour, IPointerClickHandler
    {
        public Action ClickAction;
        public void OnPointerClick(PointerEventData eventData) => ClickAction?.Invoke();
    }
}