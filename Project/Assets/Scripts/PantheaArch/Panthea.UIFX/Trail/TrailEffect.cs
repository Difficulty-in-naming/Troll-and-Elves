using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityInternal = UnityEngine.Internal;

namespace Panthea.UIFX
{
	/// <summary>
	/// 该组件是一个用于 uGUI 视觉组件的效果，渲染一个跟随组件运动的轨迹。
	/// </summary>
	/// <inheritdoc/>
	[ExecuteAlways]
	[RequireComponent(typeof(Graphic))]
	[AddComponentMenu("UI/Panthea.UIFX/Effects/UIFX - Trail")]
	public partial class TrailEffect : TrailEffectBase
	{
		// 当前帧顶点的副本
		private List<UIVertex> _vertices;

		private class TrailLayer
		{
			internal UIVertex[] vertices;
			internal Matrix4x4 matrix;
			internal Color color;
			internal float alpha;
		}

		// TrailLayer 索引 0..trailCount 顺序 = 最新..最旧, 前..后
		private List<TrailLayer> _layers = new List<TrailLayer>(16);

		// 输出顶点
		private List<UIVertex> _outputVerts;

		[UnityInternal.ExcludeFromDocs]
		protected override void OnEnable()
		{
			ResetMotion();
			SetDirty();

			if (MaskableGraphicComponent)
			{
				MaskableGraphicComponent.onCullStateChanged.AddListener(OnCullingChanged);
			}

			base.OnEnable();
		}

		private void OnCullingChanged(bool culled)
		{
			// 如果使用了 Rect2DMask，剔除会在 MaskableGraphic 上发生，
			// 这会导致轨迹逻辑停止运行，因此我们必须强制禁用剔除（如果已应用）。
			// TODO: 找到一个更优雅的解决方案
			if (culled)
			{
				CanvasRenderComponent.cull = false;
			}
		}

		[UnityInternal.ExcludeFromDocs]
		protected override void OnDisable()
		{
			if (MaskableGraphicComponent)
			{
				MaskableGraphicComponent.onCullStateChanged.RemoveListener(OnCullingChanged);
			}

			SetDirty();
			base.OnDisable();
		}

		/// <inheritdoc/>
		public override void ResetMotion()
		{
			_layers = null;
		}

		private bool HasGeometryToProcess()
		{
			return (_vertices != null && _vertices.Count > 0);
		}

		[UnityInternal.ExcludeFromDocs]
		public override void ModifyMesh(VertexHelper vh)
		{
			if (CanApply())
			{
				StoreOriginalVertices(vh);

				if (HasGeometryToProcess())
				{
					PrepareTrail();
					InterpolateTrail();
					UpdateTrailColors();

					GenerateTrailGeometry(vh);
				}
				else
				{
					ResetMotion();
				}
			}
			else if (_layerCount <= 0)
			{
				PrepareTrail();
			}
		}

		void LateUpdate()
		{
			if (CanApply() && HasGeometryToProcess())
			{
				// 更新渐变动画
				if (_gradientOffsetSpeed != 0f)
				{
					_gradientOffset += DeltaTime * _gradientOffsetSpeed;
				}

				// TODO: 仅在状态（变换/顶点）变化时设置脏标记
				SetDirty();
			}
		}
		
		protected override void SetDirty()
		{
			GraphicComponent.SetVerticesDirty();
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			SetDirty();
			base.OnValidate();
		}
#endif

		void StoreOriginalVertices(VertexHelper vh)
		{
			if (_vertices != null && vh.currentIndexCount != _vertices.Capacity)
			{
				_vertices = null;
			}
			if (_vertices == null)
			{
				_vertices = new List<UIVertex>(vh.currentIndexCount);
			}
			vh.GetUIVertexStream(_vertices);

			if (IsTrackingTransform() && IsTrackingVertices())
			{
				// 转换到世界空间
				int vertexCount = _vertices.Count;
				for (int i = 0; i < vertexCount; i++)
				{
					UIVertex vv = _vertices[i];
					vv.position = this.transform.localToWorldMatrix.MultiplyPoint3x4(vv.position);
					_vertices[i] = vv;
				}
			}
		}

		private void SetupLayer(TrailLayer layer, int layerIndex)
		{
			if (IsTrackingTransform())
			{
				if (layerIndex > 0)
				{
					// 使用上一个层的矩阵
					layer.matrix = _layers[layerIndex - 1].matrix;
				}
				else
				{
					// 使用当前矩阵
					layer.matrix = this.transform.localToWorldMatrix;
				}
			}
			if (IsTrackingVertices())
			{
				SetupLayerVertices(layer, layerIndex);
			}
		}

		private void SetupLayerVertices(TrailLayer layer, int layerIndex)
		{
			// 生成并初始化层顶点数组
			if (layer.vertices == null)
			{
				layer.vertices = new UIVertex[_vertices.Count];
				if (layerIndex > 0)
				{
					// 使用上一个层的顶点
					_layers[layerIndex - 1].vertices.CopyTo(layer.vertices, 0);
				}
				else
				{
					// 使用当前顶点
					_vertices.CopyTo(layer.vertices);
				}
			}
			// 如果顶点数量发生变化，尝试保留现有顶点数据
			else if (layer.vertices.Length != _vertices.Count)
			{
				UIVertex[] oldVertices = layer.vertices;
				layer.vertices = new UIVertex[_vertices.Count];

				if (layer.vertices.Length > 0)
				{	
					if (layerIndex == 0)
					{
						_vertices.CopyTo(layer.vertices);
					}
					else
					{
						// 从旧顶点复制
						System.Array.Copy(oldVertices, 0, layer.vertices, 0, Mathf.Min(layer.vertices.Length, oldVertices.Length));
				
						// 添加更多顶点
						if (layer.vertices.Length > oldVertices.Length)
						{
							// 从新顶点复制
							if (layerIndex > 0)
							{
								// 使用上一个层的顶点
								System.Array.Copy(_layers[layerIndex - 1].vertices, oldVertices.Length, layer.vertices, oldVertices.Length, (_vertices.Count - oldVertices.Length));
								_vertices.CopyTo(oldVertices.Length, layer.vertices, oldVertices.Length, (_vertices.Count - oldVertices.Length));
							}
							else
							{
								// 使用当前顶点
								_vertices.CopyTo(oldVertices.Length, layer.vertices, oldVertices.Length, (_vertices.Count - oldVertices.Length));
							}
						}
					}
				}
			}
		}

		private void AddTrailLayer()
		{
			var layer = new TrailLayer();
			SetupLayer(layer, _layers.Count);
			_layers.Add(layer);
		}

		protected override void OnChangedVertexModifier()
		{
			if (_layers != null)
			{
				for (int i = 0; i < _layers.Count; i++)
				{
					SetupLayer(_layers[i], i);
				}
			}
		}

		private void PrepareTrail()
		{
			// 如果层顶点数量发生变化，更新现有层的顶点
			if (_layers != null && _layers.Count > 0 && _vertices != null)
			{
				if (_layers[0].vertices != null && _layers[0].vertices.Length != _vertices.Count && IsTrackingVertices())
				{
					for (int i = 0; i < _layers.Count; i++)
					{
						SetupLayerVertices(_layers[i], i);
					}
				}
			}

			// 添加/移除轨迹层
			if (_layers != null && _layers.Count != _layerCount)
			{
				int layersToRemove = _layers.Count - _layerCount;
				for (int i = 0; i < layersToRemove; i++)
				{
					_layers.RemoveAt(_layers.Count - 1);
				}
				int layersToAdd = _layerCount - _layers.Count;
				for (int i = 0; i < layersToAdd; i++)
				{
					AddTrailLayer();
				}
			}

			// 创建轨迹层
			if (_layers == null)
			{
				_layers = new List<TrailLayer>(_layerCount);
				for (int i = 0; i < _layerCount; i++)
				{
					AddTrailLayer();
				}
			}
		}

		void InterpolateTrail()
		{
			float tStart = Mathf.Clamp01(MathUtils.GetDampLerpFactor(_dampingFront, DeltaTime));
			float tEnd = Mathf.Clamp01(MathUtils.GetDampLerpFactor(_dampingBack, DeltaTime));

			if (_strength < 1f && _strengthMode == TrailStrengthMode.Damping)
			{
				tStart = Mathf.LerpUnclamped(1f, tStart, _strength);
				tEnd = Mathf.LerpUnclamped(1f, tEnd, _strength);
			}

			float tStep = 0f;
			if (_layerCount > 0)
			{
				tStep = (tEnd - tStart) / _layerCount;
			}

			if (IsTrackingTransform())
			{
				float t = tStart;
				// 第一个轨迹层追逐原始（当前）矩阵
				Matrix4x4 targetMatrix = this.transform.localToWorldMatrix;
				for (int j = 0; j < _layerCount; j++)
				{
					TrailLayer layer = _layers[j];
					MathUtils.LerpUnclamped(ref layer.matrix, targetMatrix, t, true);

					// 其他轨迹层追逐前一个矩阵
					targetMatrix = _layers[j].matrix;

					t += tStep;
				}
			}

			if (IsTrackingVertices())
			{
				float t = tStart;
				// 第一个轨迹层顶点追逐原始顶点
				IList<UIVertex> targetVertices = _vertices;
				for (int j = 0; j < _layerCount; j++)
				{
					TrailLayer layer = _layers[j];
					
					int vertexCount = _vertices.Count;
					for (int i = 0; i < vertexCount; i++)
					{
						UIVertex source = layer.vertices[i];
						UIVertex target = targetVertices[i];
						target.position = Vector3.LerpUnclamped(source.position, target.position, t);
						// 注意：在大多数情况下，插值 UV 没有意义，例如对于文本.. 也许将来可以将其作为一个选项暴露出来。
						//target.uv0 = Vector2.LerpUnclamped(source.uv0, target.uv0, t);
						target.color = Color.LerpUnclamped(source.color, target.color, t);
						layer.vertices[i] = target;
					}
					// 其他轨迹层追逐前一个顶点
					targetVertices = _layers[j].vertices;

					t += tStep;
				}
			}
		}

		void UpdateTrailColors()
		{
			for (int j = 0; j < _layerCount; j++)
			{
				TrailLayer layer = _layers[j];

				float t = 0f;
				if (_layerCount > 1)
				{
					// 防止除以零
					t = j / (float)(_layerCount - 1);
				}

				layer.color = Color.white;
				if (_gradient != null)
				{
					layer.color = ColorUtils.EvalGradient(t, _gradient, GradientWrapMode.Mirror, _gradientOffset, _gradientScale);
				}
				
				layer.alpha = 1f;
				if (_alphaCurve != null)
				{
					layer.alpha = _alphaCurve.Evaluate(t);
				}

				if (_strength < 1f)
				{
					if (_strengthMode == TrailStrengthMode.Fade)
					{
						layer.alpha *= _strength;
					}
					else if (_strengthMode == TrailStrengthMode.Layers)
					{
						layer.alpha = t < _strength ? layer.alpha : 0f;
					}
					else if (_strengthMode == TrailStrengthMode.FadeLayers)
					{
						float step = 1f / _layerCount;
						float tmin = _strength - step;
						float tmax = _strength;
						float tt = 1f - Mathf.InverseLerp(tmin, tmax, t);
						layer.alpha *= tt;
					}
				}
			}
		}

		void GenerateTrailGeometry(VertexHelper vh)
		{
			int vertexCount = _vertices.Count;

			int trailVertexCount = vertexCount * _layerCount;
			if (!_showTrailOnly)
			{
				trailVertexCount += vertexCount;
			}
			
			if (_outputVerts != null && _outputVerts.Capacity != trailVertexCount)
			{
				_outputVerts = null;
			}
			if (_outputVerts == null)
			{
				_outputVerts = new List<UIVertex>(trailVertexCount);
			}

			_outputVerts.Clear();

			Matrix4x4 worldToLocal = this.transform.worldToLocalMatrix;

			// 添加轨迹顶点（按从后到前的顺序以正确混合透明度）
			for (int j = (_layerCount - 1); j >= 0; j--)
			{
				TrailLayer layer = _layers[j];

				if (IsTrackingTransform())
				{
					if (IsTrackingVertices())
					{
						for (int i = 0; i < vertexCount; i++)
						{
							UIVertex vv = layer.vertices[i];

							vv.position = worldToLocal.MultiplyPoint3x4(vv.position);
							Color cc = ColorUtils.Blend(vv.color, layer.color, _blendMode);
							cc.a *= layer.alpha;
							vv.color = cc;
							_outputVerts.Add(vv);
						}
					}
					else
					{
						Matrix4x4 localToWorld = layer.matrix;
						Matrix4x4 xform = worldToLocal * localToWorld;
						for (int i = 0; i < vertexCount; i++)
						{
							UIVertex vv = _vertices[i];
							vv.position = xform.MultiplyPoint3x4(vv.position);
							Color cc = ColorUtils.Blend(vv.color, layer.color, _blendMode);
							cc.a *= layer.alpha;
							vv.color = cc;
							_outputVerts.Add(vv);
						}
					}
				}
				else
				{
					for (int i = 0; i < vertexCount; i++)
					{
						UIVertex vv = layer.vertices[i];
						Color cc = ColorUtils.Blend(vv.color, layer.color, _blendMode);
						cc.a *= layer.alpha;
						vv.color = cc;
						_outputVerts.Add(vv);
					}
				}
			}

			if (!_showTrailOnly)
			{
				AddOriginalVertices(vertexCount, worldToLocal);
			}

			// 注意：尽管名字是 AddUIVertexTriangleStream，但实际上它会替换顶点，而不是添加到它们
			vh.AddUIVertexTriangleStream(_outputVerts);
		}

		void AddOriginalVertices(int vertexCount, Matrix4x4 worldToLocal)
		{
			if (IsTrackingTransform() && IsTrackingVertices())
			{
				for (int i = 0; i < vertexCount; i++)
				{
					UIVertex vv = _vertices[i];
					vv.position = worldToLocal.MultiplyPoint3x4(vv.position);
					_outputVerts.Add(vv);
				}
			}
			else
			{
				_outputVerts.AddRange(_vertices);
			}
		}
	}
}