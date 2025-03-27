using UnityEngine;
using UnityInternal = UnityEngine.Internal;

namespace Panthea.UIFX
{
	[UnityInternal.ExcludeFromDocs]
	public static class MathUtils
	{
		[UnityInternal.ExcludeFromDocs]
		public static float Snap(float v, float snap)
		{
			float isnap = 1f / snap;
			return Mathf.FloorToInt(v * isnap) / isnap;
		}

		/// <summary>
		/// 为数字添加填充，然后向上舍入到最接近的倍数。
		/// 这对于纹理非常有用，以确保它们具有恒定的最小填充量，但宽度/高度也是倍数大小。
		/// 这可以防止纹理尺寸频繁变化（例如当滤镜尺寸变化时）时频繁重新分配，
		/// 并且可以稳定因下采样非常小而导致纹理在奇数/偶数尺寸之间振荡时引起的闪烁。
		/// 例如，参数 [9, 10, 10] = 20, [10, 10, 10] = 20, [11, 10, 10] = 30
		/// </summary>
		[UnityInternal.ExcludeFromDocs]
		public static int PadAndRoundToNextMultiple(float v, int pad, int multiple)
		{
			int result = Mathf.CeilToInt(((float)System.Math.Ceiling((v + pad) / multiple)) * multiple);
			Debug.Assert(result > v);
			Debug.Assert((result % multiple) == 0);
			return result;
		}

		[UnityInternal.ExcludeFromDocs]
		public static float GetDampLerpFactor(float lambda, float deltaTime)
		{
			return 1f - Mathf.Exp(-lambda * deltaTime);
		}

		[UnityInternal.ExcludeFromDocs]
		public static float DampTowards(float a, float b, float lambda, float deltaTime)
		{
			return Mathf.Lerp(a, b, GetDampLerpFactor(lambda, deltaTime));
		}

		[UnityInternal.ExcludeFromDocs]
		public static Vector2 DampTowards(Vector2 a, Vector2 b, float lambda, float deltaTime)
		{
			return Vector2.Lerp(a, b, GetDampLerpFactor(lambda, deltaTime));
		}

		[UnityInternal.ExcludeFromDocs]
		public static Vector3 DampTowards(Vector3 a, Vector3 b, float lambda, float deltaTime)
		{
			return Vector3.Lerp(a, b, GetDampLerpFactor(lambda, deltaTime));
		}

		[UnityInternal.ExcludeFromDocs]
		public static Vector4 DampTowards(Vector4 a, Vector4 b, float lambda, float deltaTime)
		{
			return Vector4.Lerp(a, b, GetDampLerpFactor(lambda, deltaTime));
		}

		[UnityInternal.ExcludeFromDocs]
		public static Color DampTowards(Color a, Color b, float lambda, float deltaTime)
		{
			return Color.Lerp(a, b, GetDampLerpFactor(lambda, deltaTime));
		}

		[UnityInternal.ExcludeFromDocs]
		public static Matrix4x4 DampTowards(Matrix4x4 a, Matrix4x4 b, float lambda, float deltaTime)
		{
			return Matrix4x4.identity;
		}

		[UnityInternal.ExcludeFromDocs]
		public static Matrix4x4 LerpUnclamped(Matrix4x4 a, Matrix4x4 b, float t, bool preserveScale)
		{
			Vector3 targetScale = Vector3.zero;
			if (preserveScale)
			{
				targetScale = Vector3.LerpUnclamped(a.lossyScale, b.lossyScale, t);
			}

			Matrix4x4 result = new Matrix4x4();
			result.SetColumn(0, Vector4.LerpUnclamped(a.GetColumn(0), b.GetColumn(0), t));
			result.SetColumn(1, Vector4.LerpUnclamped(a.GetColumn(1), b.GetColumn(1), t));
			result.SetColumn(2, Vector4.LerpUnclamped(a.GetColumn(2), b.GetColumn(2), t));
			result.SetColumn(3, Vector4.LerpUnclamped(a.GetColumn(3), b.GetColumn(3), t));

			if (preserveScale)
			{
				Vector3 scale = result.lossyScale;
				result *= Matrix4x4.Scale(new Vector3(targetScale.x / scale.x, targetScale.y / scale.y, targetScale.z / scale.z));
			}

			return result;
		}

		[UnityInternal.ExcludeFromDocs]
		public static void LerpUnclamped(ref Matrix4x4 result, Matrix4x4 b, float t, bool preserveScale)
		{
			Vector3 targetScale = Vector3.zero;
			if (preserveScale)
			{
				targetScale = Vector3.LerpUnclamped(result.lossyScale, b.lossyScale, t);
			}

			result.SetColumn(0, Vector4.LerpUnclamped(result.GetColumn(0), b.GetColumn(0), t));
			result.SetColumn(1, Vector4.LerpUnclamped(result.GetColumn(1), b.GetColumn(1), t));
			result.SetColumn(2, Vector4.LerpUnclamped(result.GetColumn(2), b.GetColumn(2), t));
			result.SetColumn(3, Vector4.LerpUnclamped(result.GetColumn(3), b.GetColumn(3), t));

			if (preserveScale)
			{
				Vector3 scale = result.lossyScale;
				result *= Matrix4x4.Scale(new Vector3(targetScale.x / scale.x, targetScale.y / scale.y, targetScale.z / scale.z));
			}
		}

		/// <summary>
		/// 在三个值（a, b, c）之间进行插值，使用范围为 [0..1] 的 t 值
		/// </summary>
		[UnityInternal.ExcludeFromDocs]
		public static float Lerp3(float a, float b, float c, float t)
		{
			// TODO: 优化这里
			t *= 2.0f;
			float w1 = 1f - Mathf.Clamp01(t);
			float w2 = 1f - Mathf.Abs(1f - t);
			float w3 = Mathf.Clamp01(t - 1f);
			return a * w1 + b * w2 + c * w3;
		}

		[UnityInternal.ExcludeFromDocs]
		public static bool HasMatrixChanged(Matrix4x4 a, Matrix4x4 b, bool ignoreTranslation)
		{
			// 首先检查平移部分
			if (!ignoreTranslation)
			{
				if (!Mathf.Approximately(a.m03, b.m03) || !Mathf.Approximately(a.m13, b.m13) || !Mathf.Approximately(a.m23, b.m23))
				{
					return true;
				}
			}

			// 检查其余部分
			if (!Mathf.Approximately(a.m00, b.m00) || !Mathf.Approximately(a.m01, b.m01) || !Mathf.Approximately(a.m02, b.m02) ||
			    !Mathf.Approximately(a.m10, b.m10) || !Mathf.Approximately(a.m11, b.m11) || !Mathf.Approximately(a.m12, b.m12) ||
			    !Mathf.Approximately(a.m20, b.m20) || !Mathf.Approximately(a.m21, b.m21) || !Mathf.Approximately(a.m22, b.m22) ||
			    !Mathf.Approximately(a.m30, b.m30) || !Mathf.Approximately(a.m31, b.m31) || !Mathf.Approximately(a.m32, b.m32) || !Mathf.Approximately(a.m33, b.m33))
			{
				return true;
			}

			return false;
		}

		[UnityInternal.ExcludeFromDocs]
		public static void CreateRandomIndices(ref int[] array, int length)
		{
			// 仅在需要增长时重新创建
			if (array == null || array.Length < length)
			{
				array = new int[length];
			}

			// 填充
			for (int i = 0; i < length; i++)
			{
				array[i] = i;
			}

			// 打乱
			for (int i = 0; i < length; i++)
			{
				int a = Random.Range(0, length);
				int b = Random.Range(0, length);
				if (a != b)
				{
					(array[a], array[b]) = (array[b], array[a]);
				}
			}
		}

		/// <summary>
		/// 给定两个绝对坐标中的矩形，返回一个矩形，使得如果 src 相对于 dst 重新映射到范围 [0..1]
		/// 如果 src 和 dst 相同，则返回 rect(0, 0, 1, 1)
		/// 如果 src 在 dst 内，则 rect 值将 > 0 且 < 1
		/// 如果 src 大于 dst，则 rect 值将 < 0 且 > 1
		/// 返回的 rect 可以用于将一个四边形的 UV 坐标偏移和缩放到另一个四边形
		/// </summary>
		public static Rect GetRelativeRect(Rect src, Rect dst)
		{
			Rect r = Rect.zero;
			r.x = (src.x - dst.x) / dst.width;
			r.y = (src.y - dst.y) / dst.height;
			r.width = src.width / dst.width;
			r.height = src.height / dst.height;
			return r;
		}

		/// <summary>将矩形水平移动，使其宽度上的特定点与目标宽度上的等效点匹配。这对于将矩形的一个边缘对齐到另一个矩形的边缘非常有用。</summary>
		private static Rect SnapRectToRectHoriz(Rect rect, Rect target, float sizeT)
		{
			float posA = Mathf.LerpUnclamped(target.xMin, target.xMax, sizeT);
			float posB = Mathf.LerpUnclamped(rect.xMin, rect.xMax, sizeT);

			rect.x += (posA - posB);

			return rect;
		}

		/// <summary>将矩形垂直移动，使其高度上的特定点与目标高度上的等效点匹配。这对于将矩形的一个边缘对齐到另一个矩形的边缘非常有用。</summary>
		private static Rect SnapRectToRectVert(Rect rect, Rect target, float sizeT)
		{
			float posA = Mathf.LerpUnclamped(target.yMin, target.yMax, sizeT);
			float posB = Mathf.LerpUnclamped(rect.yMin, rect.yMax, sizeT);

			rect.y += (posA - posB);

			return rect;
		}

		/// <summary>
		/// 将一个矩形对齐到另一个矩形的边缘（或在两者之间的分数位置）
		/// widthT 0 表示左边缘，widthT 1 表示右边缘
		/// heightT 0 表示底部边缘，heightT 1 表示顶部边缘
		/// </summary>
		public static Rect SnapRectToRectEdges(Rect rect, Rect target, bool applyWidth, bool applyHeight, float widthT, float heightT)
		{
			if (applyWidth)
			{
				rect = SnapRectToRectHoriz(rect, target, widthT);
			}

			if (applyHeight)
			{
				rect = SnapRectToRectVert(rect, target, heightT);
			}

			return rect;
		}

		/// <summary>返回使用缩放模式的宽高比矩形</summary>
		public static Rect ResizeRectToAspectRatio(Rect rect, ScaleMode scaleMode, float aspect)
		{
			float srcAspect = aspect;
			float dstAspect = rect.width / rect.height;

			float stretch;
			// src 比 dst 更宽
			if (srcAspect > dstAspect)
			{
				stretch = dstAspect / srcAspect;
			}
			else
			{
				stretch = srcAspect / dstAspect;
			}

			Rect result = rect;
			switch (scaleMode)
			{
				case ScaleMode.StretchToFill:
					break;
				case ScaleMode.ScaleAndCrop:
				{
					// src 比 dst 更宽
					if (srcAspect > dstAspect)
					{
						float newWidth = rect.width / stretch;
						result = new Rect(rect.xMin - (newWidth - rect.width) * 0.5f, rect.yMin, newWidth, rect.height);
					}
					else
					{
						float newHeight = rect.height / stretch;
						result = new Rect(rect.xMin, rect.yMin - (newHeight - rect.height) * 0.5f, rect.width, newHeight);
					}
				}
					break;
				case ScaleMode.ScaleToFit:
				{
					// src 比 dst 更宽
					if (srcAspect > dstAspect)
					{
						result = new Rect(rect.xMin, rect.yMin + rect.height * (1f - stretch) * 0.5f, rect.width, stretch * rect.height);
					}
					else
					{
						result = new Rect(rect.xMin + rect.width * (1f - stretch) * 0.5f, rect.yMin, stretch * rect.width, rect.height);
					}
				}
					break;
			}

			return result;
		}
	}
}
