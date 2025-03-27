using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace EdgeStudio.UI.Component
{
	[RequireComponent(typeof(CanvasRenderer))]
	public class UIMaskableMesh : MonoBehaviour
	{
		private const string MASKABLE_MESH_SHADER_NAME = "UI/Maskable Mesh Unlit";
		public bool InitOnEnable = false;
		[BindField("_this")]public Transform CachedTransform;
		[BindField("_this")]public CanvasRenderer CanvasRenderer;
		private Mesh Mesh;
		private Material[] Materials;

		private Shader mShader;
		private Shader MaskableShader
		{
			get
			{
				if (!mShader)
				{
					mShader = Shader.Find(MASKABLE_MESH_SHADER_NAME);
				}
				return mShader;
			}
		}

		void Reset()
		{
			CanvasRenderer = GetComponent<CanvasRenderer>();
			CachedTransform = transform;
		}
	
		public void Initialize(Mesh mesh,Material[] materials)
		{
			CanvasRenderer.materialCount = materials.Length;
			CanvasRenderer.SetMesh(mesh);
			var rootCanvas = MaskUtilities.FindRootSortOverrideCanvas(CachedTransform);
			var stencilValue = MaskUtilities.GetStencilDepth(CachedTransform, rootCanvas);

			for (var index = 0; index < materials.Length; index++)
			{
				var node = new Material(materials[index]);
				node.shader = MaskableShader;
				var maskMat = StencilMaterial.Add(node, (1 << stencilValue) - 1, StencilOp.Keep, CompareFunction.Equal, ColorWriteMask.All, (1 << stencilValue) - 1, 0);
				StencilMaterial.Remove(node);
				CanvasRenderer.SetMaterial(maskMat, index);
			}
		}

		void OnValidate()
		{
			if (Mesh && Materials != null)
			{
				Initialize(Mesh, Materials);
			}
		}
	}
}
