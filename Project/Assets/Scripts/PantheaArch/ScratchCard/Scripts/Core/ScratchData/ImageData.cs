using UnityEngine;
using UnityEngine.UI;

namespace ScratchCardAsset.Core.ScratchData
{
    public class ImageData : BaseData
    {
        private readonly Image image;
        private readonly bool isCanvasOverlay;

        public override Vector2 TextureSize => image.sprite.rect.size;
        protected override Vector2 Bounds => image.rectTransform.rect.size;
        
        public ImageData(Transform surface, Camera camera) : base(surface, camera)
        {
            if (surface.TryGetComponent(out image))
            {
                isCanvasOverlay = image.canvas.renderMode == RenderMode.ScreenSpaceOverlay;
                InitTriangle();
            }
        }

        public override Vector2 GetScratchPosition(Vector2 screenPosition)
        {
            if (isCanvasOverlay)
            {
                var scratchPosition = Vector2.zero;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)Surface, screenPosition, null, out var worldPosition))
                {
                    var pointLocal = Surface.InverseTransformPoint(worldPosition);
                    var uv = Triangle.GetUV(pointLocal);
                    scratchPosition = Vector2.Scale(TextureSize, uv);
                }
                return scratchPosition;
            }
            return base.GetScratchPosition(screenPosition);
        }
    }
}