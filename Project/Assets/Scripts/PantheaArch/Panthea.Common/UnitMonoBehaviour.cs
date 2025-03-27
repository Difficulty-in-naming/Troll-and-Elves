using UnityEngine;

namespace Panthea.Common
{
    public class UnitMonoBehaviour : BetterMonoBehaviour
    {
        public Vector2 scale
        {
            get => CachedTransform.localScale; set => CachedTransform.localScale = value;
        }
        
        public Vector3 position
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
        
        public float width
        {
            get => CachedTransform.localScale.x; set => CachedTransform.localScale = new Vector3(value, CachedTransform.localScale.y);
        }
        
        public float height
        {
            get => CachedTransform.localScale.y; set => CachedTransform.localScale = new Vector3(CachedTransform.localScale.x, value);
        }
    }
}