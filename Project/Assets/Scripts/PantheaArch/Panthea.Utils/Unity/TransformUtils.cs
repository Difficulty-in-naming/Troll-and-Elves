using System.Collections.Generic;
using UnityEngine;

namespace Panthea.Utils
{
    public static class TransformUtils
    {
        public static Transform[] GetChildren(this Transform tr)
        {
            int childCount = tr.childCount;
            Transform[] result = new Transform[childCount];
            for (int i = 0; i < childCount; ++i)
                result[i] = tr.GetChild(i);

            return result;
        }
        public static Transform[] GetChildren(this Transform tr,bool includeInactive)
        {
            int childCount = tr.childCount;
            List<Transform> result = new List<Transform>();
            for (int i = 0; i < childCount; ++i)
            {
                var node = tr.GetChild(i);
                if(includeInactive)
                    result.Add(node);
                else
                {
                    if(node.gameObject.activeSelf)
                        result.Add(node);
                }
            }

            return result.ToArray();
        }
        
        public static Vector3 GetUILossyScale(this RectTransform rectTransform)
        {
            Vector3 cumulativeScaleFactor = rectTransform.localScale;
            Transform currentTransform = rectTransform;
            var root = rectTransform.GetComponentInParent<Canvas>().rootCanvas.transform;
            while (currentTransform != root)
            {
                cumulativeScaleFactor *= currentTransform.localScale.x;
                currentTransform = currentTransform.parent;
            }

            return cumulativeScaleFactor;
        }
        
        public static void ChangeLayersRecursively(this Transform transform, int layerIndex)
        {
            transform.gameObject.layer = layerIndex;
            foreach (Transform child in transform)
            {
                child.ChangeLayersRecursively(layerIndex);
            }
        }
    }
}