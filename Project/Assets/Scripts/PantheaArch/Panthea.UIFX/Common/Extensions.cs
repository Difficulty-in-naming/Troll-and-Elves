using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Panthea.UIFX
{
	// ReSharper disable once InconsistentNaming
	public static class Matrix4x4Helper
	{
		public static Matrix4x4 Rotate(Quaternion rotation) => Matrix4x4.Rotate(rotation);
	}

	public static class ObjectHelper
	{
		public static void Destroy<T>(ref T obj) where T : Object
		{
			if (obj)
			{
				Destroy(obj);
				obj = null;
			}
		}

		public static void Destroy<T>(T obj) where T : Object
		{
			if (obj)
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					Object.DestroyImmediate(obj);
				}
				else
#endif
				{
					Object.Destroy(obj);
				}
			}
		}

		public static void Dispose<T>(ref T obj) where T : System.IDisposable
		{
			if (obj != null)
			{
				obj.Dispose();
				obj = default(T);
			}
		}

		public static bool ChangeProperty<T>(ref T backing, T value) where T : struct
		{
			if (!(EqualityComparer<T>.Default.Equals(backing, value)))
			{
				backing = value;
				return true;
			}
			return false;
		}

		public static void ChangeProperty<T>(ref T backing, T value, ref bool hasChanged) where T : struct
		{
			if (!(EqualityComparer<T>.Default.Equals(backing, value)))
			{
				backing = value;
				hasChanged = true;
			}
			hasChanged = false;
		}
	}

	public static class RenderTextureHelper
	{
		public static void ReleaseTemporary(ref RenderTexture rt)
		{
			if (rt)
			{
				RenderTexture.ReleaseTemporary(rt); rt = null;
			}
		}
	}

	public static class VertexHelperExtensions
	{
		public static void ReplaceUIVertexTriangleStream(this VertexHelper vh, List<UIVertex> vertices)
		{
			// 注意：尽管其名称如此，但此方法实际上会替换顶点，而不会添加顶点
			vh.AddUIVertexTriangleStream(vertices);
		}
	}

	public static class MaterialHelper
	{
		public static bool MaterialOutputsPremultipliedAlpha(Material material)
		{
			bool result;

			if (material.HasProperty(UnityShaderProp.BlendSrc) && material.HasProperty(UnityShaderProp.BlendDst))
			{
				result = ((material.GetInt(UnityShaderProp.BlendSrc) == (int)UnityEngine.Rendering.BlendMode.One) && (material.GetInt(UnityShaderProp.BlendDst) == (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
				return result;
			}

			string tag = material.GetTag("OutputsPremultipliedAlpha", false, string.Empty);
			if (!string.IsNullOrEmpty(tag))
			{
				result = (tag.ToLower() == "true");
				return result;
			}

			return true;
		}
	}

	public static class EditorHelper
	{
		public static bool IsInContextPrefabMode()
		{
			bool result = false;
#if UNITY_EDITOR
			var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
			if (stage && stage.mode == UnityEditor.SceneManagement.PrefabStage.Mode.InContext)
			{
				result = true;
			}
#endif
			return result;
		}
	}
}