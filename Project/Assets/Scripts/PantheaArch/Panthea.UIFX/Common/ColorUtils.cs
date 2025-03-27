using System.Collections.Generic;
using UnityEngine;
using UnityInternal = UnityEngine.Internal;

namespace Panthea.UIFX
{
	/// <summary>
	/// 用于组合两种颜色时的混合模式
	/// </summary>
	public enum BlendMode
	{
		/// <summary>`Source` - 仅使用原始颜色，忽略任何轨迹渐变/透明度设置。</summary>
		Source,
		/// <summary>`Replace` - 忽略原始颜色，并使用轨迹渐变/透明度设置替换。<br/>`Replace_Multiply`</summary>
		Replace,
		/// <summary>`Replace_Multiply` - 对于RGB部分与`Replace`相同，但将原始透明度与轨迹渐变透明度相乘。</summary>
		// ReSharper disable once InconsistentNaming
		Replace_Multiply,
		/// <summary>`Multiply` - 将原始颜色与轨迹渐变/透明度设置相乘。</summary>
		Multiply,
		/// <summary>`Add_Multiply` - 将原始颜色的RGB部分与渐变颜色相加，但将透明度值相乘。</summary>
		// ReSharper disable once InconsistentNaming
		Add_Multiply
	}

	public enum BuiltInGradient
	{
		SoftRainbow,
		Grey80ToClear,
	}

	[UnityInternal.ExcludeFromDocs]
	public static class ColorUtils
	{
		private static readonly Gradient SoftRainbowGradient;
		private static readonly Gradient Grey80ToClearGradient;

		[UnityInternal.ExcludeFromDocs]
		public static Gradient GetBuiltInGradient(BuiltInGradient b)
		{
			Gradient result = b switch
			{
				BuiltInGradient.SoftRainbow => SoftRainbowGradient,
				BuiltInGradient.Grey80ToClear => Grey80ToClearGradient,
				_ => null
			};

			return CloneGradient(result);
		}

		static ColorUtils()
		{
			{
				GradientColorKey[] keys = new GradientColorKey[5];
				keys[0].time = 0f;
				keys[0].color = new Color(0.03116761f, 0.9716981f, 0.5848317f);
				keys[1].time = 0.25f;
				keys[1].color = new Color(0.3635457f, 0.4478632f, 0.8679245f);
				keys[2].time = 0.5f;
				keys[2].color = new Color(0.8301887f, 0.4025632f, 0.829066f);
				keys[3].time = 0.75f;
				keys[3].color = new Color(0.8490566f, 0.1874332f, 0.5700046f);
				keys[4].time = 1f;
				keys[4].color = new Color(0.8207547f, 0.7230087f, 0.1966714f);
				GradientAlphaKey[] alpha = new GradientAlphaKey[2];
				alpha[0].time = 0f;
				alpha[0].alpha = 1f;
				alpha[1].time = 1f;
				alpha[1].alpha = 1f;
				SoftRainbowGradient = new Gradient();
				SoftRainbowGradient.SetKeys(keys, alpha);
			}
			{
				GradientColorKey[] keys = new GradientColorKey[2];
				keys[0].time = 0f;
				keys[0].color = Color.white * 0.8f;
				keys[1].time = 1f;
				keys[1].color = Color.white * 0.8f;
				GradientAlphaKey[] alpha = new GradientAlphaKey[2];
				alpha[0].time = 0f;
				alpha[0].alpha = 1f;
				alpha[1].time = 1f;
				alpha[1].alpha = 0f;
				Grey80ToClearGradient = new Gradient();
				Grey80ToClearGradient.SetKeys(keys, alpha);
			}
		}

		public static Gradient CloneGradient(Gradient gradient)
		{
			Gradient result = new Gradient();
			result.SetKeys(gradient.colorKeys, gradient.alphaKeys);
			return result;
		}

		public static Color Blend(Color a, Color b, BlendMode mode)
		{
			switch (mode)
			{
				case BlendMode.Source: return a;
				case BlendMode.Replace: return b;
				case BlendMode.Replace_Multiply:
				{
					b.a *= a.a;
					return b;
				}
				case BlendMode.Multiply: return a * b;
				case BlendMode.Add_Multiply:
				{
					a.r += b.r;
					a.g += b.g;
					a.b += b.b;
					a.a *= b.a;
					return a;
				}
			}
			return a;
		}	
		
		public static Color EvalGradient(float t, Gradient gradient, GradientWrapMode wrapMode, float offset = 0f, float scale = 1f, float scalePivot = 0f)
		{
			t -= scalePivot;
			t *= scale;
			t += scalePivot;
			t += offset;

			if (wrapMode == GradientWrapMode.Wrap)
			{
				// 注意：只有当我们超出范围时才进行换行，否则对于t=1.0（这种情况经常发生），我们将计算0.0，这在大多数情况下不是我们想要的
				if (t < 0f || t > 1f)
				{
					t = Mathf.Repeat(t, 1f);
				}
			}
			else if (wrapMode == GradientWrapMode.Mirror)
			{
				t = Mathf.PingPong(t, 1f);
				if (Mathf.Sign(scale) < 0f)
				{
					t = 1f - t;
				}
			}

			return gradient.Evaluate(t);
		}
		
		public static Vector3 LinearToOklab(Color c)
		{
			float l = 0.4122214708f * c.r + 0.5363325363f * c.g + 0.0514459929f * c.b;
			float m = 0.2119034982f * c.r + 0.6806995451f * c.g + 0.1073969566f * c.b;
			float s = 0.0883024619f * c.r + 0.2817188376f * c.g + 0.6299787005f * c.b;

			float l_ = Cbrtf(l);
			float m_ = Cbrtf(m);
			float s_ = Cbrtf(s);

			return new Vector3 {
				x = 0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_,
				y = 1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_,
				z = 0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_
			};
		}

		private static float Cbrtf(float v) => Mathf.Sign(v) * Mathf.Pow(Mathf.Abs(v), 1f / 3f);

		internal static void ConvertMeshVertexColorsToLinear(Mesh mesh, ref List<Color> colorCache)
		{
			int vertexCount = mesh.vertexCount;
			if (colorCache != null && colorCache.Count != vertexCount)
			{
				colorCache = null;
			}
			colorCache ??= new List<Color>(vertexCount);
			mesh.GetColors(colorCache);

			// 在极少数情况下，可能没有颜色
			if (colorCache.Count > 0)
			{
				Debug.Assert(colorCache.Count == vertexCount);
				for (var i = 0; i < vertexCount; i++)
				{
					colorCache[i] = colorCache[i].linear;
				}
				mesh.SetColors(colorCache);
			}
		}
	}
}