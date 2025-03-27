using UnityEngine;

namespace Panthea.Common
{
    public class BetterMonoBehaviour : MonoBehaviour
    {
        private Transform mTransform;
        public Transform CachedTransform
        {
            get
            {
                if(this)
                    return mTransform ? mTransform : mTransform = transform;
                return null;
            }
        }

        private GameObject mGameObject;
        public GameObject CachedGameObject
        {
            get
            {
                if(this)
                    return mGameObject ? mGameObject : mGameObject = gameObject;
                return null;
            }
        }
        
        public RectTransform RectTransform => (RectTransform)CachedTransform;
    }
}