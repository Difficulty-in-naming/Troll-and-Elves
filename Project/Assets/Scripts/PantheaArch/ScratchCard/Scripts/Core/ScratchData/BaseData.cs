using UnityEngine;

namespace ScratchCardAsset.Core.ScratchData
{
    public abstract class BaseData
    {
	    public Camera Camera { protected get; set; }
	    public abstract Vector2 TextureSize { get; }
	    protected abstract Vector2 Bounds { get; }
	    protected virtual bool IsOrthographic => Camera.orthographic;
	    protected Transform Surface { get; }
	    protected Triangle Triangle { get; private set; }

        protected BaseData(Transform surface, Camera camera)
        {
            Surface = surface;
            Camera = camera;
        }
        
        protected void InitTriangle()
        {
	        var bounds = Bounds;
	        //bottom left
	        var position0 = new Vector3(-bounds.x / 2f, -bounds.y / 2f, 0);
	        var uv0 = Vector2.zero;
	        //upper left
	        var position1 = new Vector3(-bounds.x / 2f, bounds.y / 2f, 0);
	        var uv1 = Vector2.up;
	        //upper right
	        var position2 = new Vector3(bounds.x / 2f, bounds.y / 2f, 0);
	        var uv2 = Vector2.one;
	        Triangle = new Triangle(position0, position1, position2, uv0, uv1, uv2);
        }

        public virtual Vector2 GetScratchPosition(Vector2 screenPosition)
        {
	        var scratchPosition = Vector2.zero;
	        var plane = new Plane(Surface.forward, Surface.position);
	        var ray = Camera.ScreenPointToRay(screenPosition);
	        if (plane.Raycast(ray, out var enter))
	        {
		        var point = ray.GetPoint(enter);
		        var pointLocal = Surface.InverseTransformPoint(point);
		        var uv = Triangle.GetUV(pointLocal);
		        scratchPosition = Vector2.Scale(TextureSize, uv);
	        }
	        
	        return scratchPosition;
        }

        public Vector2 GetLocalPosition(Vector2 texturePosition)
        {
	        return Triangle.GetLocalPosition(texturePosition, TextureSize);
        }
    }
}