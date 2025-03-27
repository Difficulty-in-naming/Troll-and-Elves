using UnityEngine;

namespace Panthea.Common
{
    public class UnitTransform
    {
        private readonly Transform CachedTransform;
        public UnitTransform(Transform transform)
        {
            CachedTransform = transform;
        }
        
        public Vector2 scale
        {
            get => CachedTransform.localScale; set => CachedTransform.localScale = value;
        }
        
        public Vector3 position
        {
            get => CachedTransform.position; set => CachedTransform.position = value;
        }
        
        public Vector2 xy
        {
            get => CachedTransform.position; set => CachedTransform.position = value;
        }
        
        public float rotation
        {
            get => CachedTransform.localEulerAngles.z; set => CachedTransform.localEulerAngles = new Vector3(0, 0, value);
        }
        
        public float x
        {
            get => CachedTransform.position.x; set => CachedTransform.position = new Vector3(value, CachedTransform.position.y, CachedTransform.position.z);
        }
        
        public float y
        {
            get => CachedTransform.position.x; set => CachedTransform.position = new Vector3(value, CachedTransform.position.y, CachedTransform.position.z);
        }
        
        public float z
        {
            get => CachedTransform.position.x; set => CachedTransform.position = new Vector3(CachedTransform.position.x, CachedTransform.position.y, value);
        }
        
        public float scaleX
        {
            get => CachedTransform.localScale.x; set => CachedTransform.localScale = new Vector3(value, CachedTransform.localScale.y,CachedTransform.localScale.z);
        }
        
        public float scaleY
        {
            get => CachedTransform.localScale.y; set => CachedTransform.localScale = new Vector3(CachedTransform.localScale.x, value,CachedTransform.localScale.z);
        }

        public float scaleZ
        {
            get => CachedTransform.localScale.z; set => CachedTransform.localScale = new Vector3(CachedTransform.localScale.x,CachedTransform.localScale.y, value);
        }
        
        public Vector2 anchorMin
        {
            get => ((RectTransform)CachedTransform).anchorMin;
            set => ((RectTransform)CachedTransform).anchorMin = value;
        }
        
        public Vector2 anchorMax
        {
            get => ((RectTransform)CachedTransform).anchorMax;
            set => ((RectTransform)CachedTransform).anchorMax = value;
        }

        public float pivotX
        {
            get => ((RectTransform)CachedTransform).pivot.x;
            set => ((RectTransform)CachedTransform).pivot = new Vector2(value, ((RectTransform)CachedTransform).pivot.y);
        }
        
        public float pivotY
        {
            get => ((RectTransform)CachedTransform).pivot.y;
            set => ((RectTransform)CachedTransform).pivot = new Vector2(((RectTransform)CachedTransform).pivot.x, value);
        }
        
        public Vector2 anchoredPosition
        {
            get => ((RectTransform)CachedTransform).anchoredPosition;
            set => ((RectTransform)CachedTransform).anchoredPosition = value;
        }
        public Vector2 size
        {
            get => ((RectTransform)CachedTransform).sizeDelta; set =>  ((RectTransform)CachedTransform).sizeDelta = value;
        }
        
        public float width
        {
            get => ((RectTransform)CachedTransform).sizeDelta.x;
            set => ((RectTransform)CachedTransform).sizeDelta = new Vector2(value, ((RectTransform)CachedTransform).sizeDelta.y);
        }
        
        public float height
        {
            get => ((RectTransform)CachedTransform).sizeDelta.y;
            set => ((RectTransform)CachedTransform).sizeDelta = new Vector2(((RectTransform)CachedTransform).sizeDelta.x,value);
        }
    }
}