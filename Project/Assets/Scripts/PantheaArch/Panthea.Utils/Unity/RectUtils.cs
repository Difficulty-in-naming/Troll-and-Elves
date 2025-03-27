using UnityEngine;

namespace Panthea.Utils
{
    public static class RectUtils
    {
        public static Rect ExtendRect(this Rect rect1, Rect rect2)
        {
            float minX = Mathf.Min(rect1.xMin, rect2.xMin);
            float minY = Mathf.Min(rect1.yMin, rect2.yMin);

            float maxX = Mathf.Max(rect1.xMax, rect2.xMax);
            float maxY = Mathf.Max(rect1.yMax, rect2.yMax);

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }
        
        public static void ExtendRectRef(this ref Rect rect1, Rect rect2) => rect1 = ExtendRect(rect1, rect2);
    }
}