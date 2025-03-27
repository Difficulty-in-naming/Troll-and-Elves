using UnityEngine;
using UnityEngine.UI;

namespace Panthea.Utils
{

public static class UIExtensions
{
    // 共享数组，用于接收 RectTransform.GetWorldCorners 的结果
    static Vector3[] corners = new Vector3[4];

    /// <summary>
    /// 将当前 RectTransform 的边界转换到另一个变换的空间中。
    /// </summary>
    /// <param name="source">要转换的矩形</param>
    /// <param name="target">目标空间</param>
    /// <returns>转换后的边界</returns>
    public static Bounds TransformBoundsTo(this RectTransform source, Transform target)
    {
        // 基于 ScrollRect 的内部 GetBounds 和 InternalGetBounds 方法的代码
        var bounds = new Bounds();
        if (source != null)
        {
            source.GetWorldCorners(corners);

            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var matrix = target.worldToLocalMatrix;
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = matrix.MultiplyPoint3x4(corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
        }

        return bounds;
    }

    /// <summary>
    /// 标准化一个距离，以便用于 verticalNormalizedPosition 或 horizontalNormalizedPosition。
    /// </summary>
    /// <param name="axis">滚动轴，0 = 水平，1 = 垂直</param>
    /// <param name="distance">滚动矩形视图坐标空间中的距离</param>
    /// <returns>标准化的滚动距离</returns>
    public static float NormalizeScrollDistance(this ScrollRect scrollRect, int axis, float distance)
    {
        // 基于 ScrollRect 的内部 SetNormalizedPosition 方法的代码
        var viewport = scrollRect.viewport;
        var viewRect = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
        var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

        var content = scrollRect.content;
        var contentBounds = content != null ? content.TransformBoundsTo(viewRect) : new Bounds();

        var hiddenLength = contentBounds.size[axis] - viewBounds.size[axis];
        return distance / hiddenLength;
    }

    /// <summary>
    /// 将目标元素滚动到滚动矩形视图的垂直中心。
    /// 假设目标元素是滚动矩形内容的一部分。
    /// </summary>
    /// <param name="scrollRect">要滚动的滚动矩形</param>
    /// <param name="target">要垂直居中的滚动矩形内容元素</param>
    public static void ScrollToCenter(this ScrollRect scrollRect, RectTransform target)
    {
        // 滚动矩形的视图空间用于计算滚动位置
        var view = scrollRect.viewport ?? scrollRect.GetComponent<RectTransform>();

        // 计算视图空间中的滚动偏移
        var viewRect = view.rect;
        var elementBounds = target.TransformBoundsTo(view);

        // 标准化并应用计算出的偏移
        if (scrollRect.vertical)
        {
            var offset = viewRect.center.y - elementBounds.center.y;
            var scrollPos = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset);
            scrollRect.verticalNormalizedPosition = Mathf.Clamp(scrollPos, 0, 1);
        }
        if(scrollRect.horizontal)
        {
            var offset = viewRect.center.x - elementBounds.center.x;
            var scrollPos = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(0, offset);
            scrollRect.horizontalNormalizedPosition = Mathf.Clamp(scrollPos, 0, 1);
        }
    }
}
}