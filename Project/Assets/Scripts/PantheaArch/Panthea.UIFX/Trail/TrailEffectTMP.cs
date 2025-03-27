#if UIFX_TMPRO

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityInternal = UnityEngine.Internal;

namespace Panthea.UIFX
{
	// NOTE: Since TMP doesn't derive from Graphic, we could, and so instead of modify the TMP mesh, we could just generate the mesh using OnPopulateMesh() which may be simpler!
	// However, we would then need to render using TMP materials, which may be more difficult...

	/// <summary>
	/// This component is an effect for Text Mesh Pro which renders a trail that follows
	/// the motion of the TMP_Text component.
	/// </summary>
	/// <inheritdoc/>
	[RequireComponent(typeof(TMP_Text))]
	[HelpURL("https://www.chocdino.com/products/unity-assets/")]
	[AddComponentMenu("UI/Panthea.UIFX/Effects/UIFX - Trail TMP")]
	public class TrailEffectTMP : TrailEffectBase
	{
		private TMP_Text _textMeshPro;

		// NOTE: Usually it's fine to just modify the TMP mesh, but when there is ANOTHER script that's modifying the TMP mesh then there will be a conflict because this script increases the number of vertices.
		// In that case the CanvasRenderer can be set to render our own mesh.
		public enum TargetMesh
		{
			// Modifies the TMP mesh
			TextMeshPro,

			// Doesn't modify the TMP mesh, instead assigns a new mesh to the CanvasRenderer
			Internal,
		}

		private class LayerVertices
		{
			public Vector3[] positions;
			public Vector2[] uvs0;
			public Vector4[] uvs0_v4;
			public Vector2[] uvs1;
			public Color32[] colors;

			public LayerVertices(int vertexCount)
			{
				positions = new Vector3[vertexCount];
				if (TrailEffectTMP.s_isUV0Vector4) {	uvs0_v4 = new Vector4[vertexCount]; } else { uvs0 = new Vector2[vertexCount];	}
				uvs1 = new Vector2[vertexCount];
				colors = new Color32[vertexCount];
			}

			public int VertexCount { get { return positions.Length; } }

			public void CopyTo(LayerVertices dst)
			{
				Debug.Assert(positions.Length == dst.positions.Length);
				positions.CopyTo(dst.positions, 0);
				if (uvs0 != null) { uvs0.CopyTo(dst.uvs0, 0); }
				if (uvs0_v4 != null) { uvs0_v4.CopyTo(dst.uvs0_v4, 0); }
				uvs1.CopyTo(dst.uvs1, 0);
				colors.CopyTo(dst.colors, 0);
			}

			public void CopyTo(LayerVertices dst, int dstOffset, int count)
			{
				Debug.Assert(positions.Length >= count);
				Debug.Assert(dst.positions.Length >= (dstOffset + count));
				System.Array.Copy(positions, 0, dst.positions, dstOffset, count);
				if (uvs0 != null) { System.Array.Copy(uvs0, 0, dst.uvs0, dstOffset, count); }
				if (uvs0_v4 != null) { System.Array.Copy(uvs0_v4, 0, dst.uvs0_v4, dstOffset, count); }
				System.Array.Copy(uvs1, 0, dst.uvs1, dstOffset, count);
				System.Array.Copy(colors, 0, dst.colors, dstOffset, count);
			}

			public void CopyTo(int srcOffset, LayerVertices dst, int dstOffset, int count)
			{
				Debug.Assert(positions.Length >= (srcOffset + count));
				Debug.Assert(dst.positions.Length >= (dstOffset + count));
				System.Array.Copy(positions, srcOffset, dst.positions, dstOffset, count);
				if (uvs0 != null) { System.Array.Copy(uvs0, srcOffset, dst.uvs0, dstOffset, count); }
				if (uvs0_v4 != null) { System.Array.Copy(uvs0_v4, srcOffset, dst.uvs0_v4, dstOffset, count); }
				System.Array.Copy(uvs1, srcOffset, dst.uvs1, dstOffset, count);
				System.Array.Copy(colors, srcOffset, dst.colors, dstOffset, count);
			}
		}
	
		private class TrailLayer
		{
			internal LayerVertices vertices;
			internal Matrix4x4 matrix;
			internal Color color;
			internal float alpha;
		}

		private LayerVertices _originalVerts;
		private LayerVertices _outputVerts;

		// TrailLayer index 0..trailCount order = newest..oldest, front..back
		private List<TrailLayer> _layers = new List<TrailLayer>(16);

		private bool _appliedLastFrame = false;
		private static bool s_isUV0Vector4 = false;

		private TargetMesh _targetMesh = TargetMesh.TextMeshPro;
		private int[] _triangleIndices;
		private Mesh _mesh;

		static TrailEffectTMP()
		{
			// Note: Most "preview" TMP versions have changed MeshInfo.UVS0 from Vector2 to Vector4
			s_isUV0Vector4 = typeof(TMP_MeshInfo).GetField("uvs0").FieldType == typeof(Vector4[]);
		}

		[UnityInternal.ExcludeFromDocs]
		protected override void Awake()
		{
			_textMeshPro = GetComponent<TMP_Text>();
			Debug.Assert(_textMeshPro != null);
			base.Awake();
		}

		[UnityInternal.ExcludeFromDocs]
		protected override void OnEnable()
		{
			if (_textMeshPro != null)  { _textMeshPro = GetComponent<TMP_Text>(); Debug.Assert(_textMeshPro != null); }
			_textMeshPro.renderMode = TextRenderFlags.DontRender;

			ResetMotion();
			SetDirty();

			if (_targetMesh == TargetMesh.Internal)
			{
				Canvas.willRenderCanvases += WillRenderCanvases;
			}

			base.OnEnable();
		}

		[UnityInternal.ExcludeFromDocs]
		protected override void OnDisable()
		{
			Canvas.willRenderCanvases -= WillRenderCanvases;

			_textMeshPro.renderMode = TextRenderFlags.Render;

			// Forces the mesh to regenerate to the orignal (pre-trail) state
			{
				ForceMeshBackToOriginal();
				_textMeshPro.ForceMeshUpdate(false, false);
			}

			ObjectHelper.Destroy(ref _mesh);
			
			SetDirty();
			base.OnDisable();
		}

		/// <inheritdoc/>
		public override void ResetMotion()
		{
			_layers = null;
			ForceMeshBackToOriginal();
		}

		#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
		}
		#endif

		private void ForceMeshBackToOriginal()
		{
			// This sets the mesh back to the original state (without trail)
			// NOTE: We have to do this since we're modifying the size of the mesh, which TMP doesn't expect...Otherwise we get out of bounds errors for the triangle/vertex arrays.
			if (_textMeshPro && _targetMesh == TargetMesh.TextMeshPro)
			{
				for (int i = 0; i < _textMeshPro.textInfo.materialCount; i++)
				{
					if (_textMeshPro.textInfo.meshInfo[i].vertexCount > 0)
					{
						_textMeshPro.textInfo.meshInfo[i].ResizeMeshInfo(_textMeshPro.textInfo.meshInfo[i].vertexCount / 4, false);
					}
				}
			}
		}

		private bool HasGeometryToProcess()
		{
			if (_originalVerts == null || _originalVerts.VertexCount <= 0) return false;
			return true;
		}

		void LateUpdate()
		{
			if (CanApply())
			{
				{
					// Detected changes to vertex count or effective layer count
					if (_textMeshPro.havePropertiesChanged)
					{
						ForceMeshBackToOriginal();
					}
					else if (_outputVerts != null)
					{
						int textVertexCount = 0;
						for (int i = 0; i < _textMeshPro.textInfo.materialCount; i++)
						{
							textVertexCount += _textMeshPro.textInfo.meshInfo[i].vertexCount;
						}

						int layerCount = _layerCount + (_showTrailOnly ? 0 : 1);
						int trailVertexCount = textVertexCount * layerCount;
						if (trailVertexCount != _outputVerts.VertexCount)
						{
							ForceMeshBackToOriginal();
						}
					}

					// Force the text mesh to be regenerated
					if (_targetMesh == TargetMesh.TextMeshPro)
					{
						_textMeshPro.renderMode = TextRenderFlags.DontRender;
						_textMeshPro.ForceMeshUpdate(false, false);
					}

					// NOTE: We call this from LateUpdate() instead of from the OnPreRenderText action as otherwise
					// adjusting the number of triangles causes an error to be thrown, I think because TMP
					// doesn't call SetTriangles() on its main mesh before calling SetVertices().
					ModifyGeometry(_textMeshPro.textInfo);
				}
			}
			else
			{
				if (_appliedLastFrame)
				{
					// Revert back to the original mesh so that trail is no longer applied
					_textMeshPro.renderMode = TextRenderFlags.Render;
					ForceMeshBackToOriginal();
					_textMeshPro.ForceMeshUpdate(false, false);
					_appliedLastFrame = false;
				}
				if (_layerCount <= 0)
				{
					PrepareTrail();
				}
			}
		}

		void ModifyGeometry(TMP_TextInfo textInfo)
		{
			if (_textMeshPro == null || !_textMeshPro.IsActive()) return;
		
			StoreOriginalVertices(textInfo);

			if (HasGeometryToProcess())
			{
				// Update gradient animation
				if (_gradientOffsetSpeed != 0f)
				{
					_gradientOffset += DeltaTime * _gradientOffsetSpeed;
				}

				PrepareTrail();

				InterpolateTrail();
				UpdateTrailColors();

				GenerateTrailGeometry();

				if (_targetMesh == TargetMesh.TextMeshPro)
				{
					AssignTrailGeometryToMesh(textInfo);
				}

				_appliedLastFrame = true;
			}
			else
			{
				ResetMotion();
			}
		}

		protected override void SetDirty()
		{
			GraphicComponent.SetVerticesDirty();
		}

		void StoreOriginalVertices(TMP_TextInfo textInfo)
		{
			// Allocate for original mesh vertices if we have to
			{
				int totalVertexCount = 0;
				for (int i = 0; i < textInfo.materialCount; i++)
				{
					TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
					totalVertexCount += meshInfo.vertexCount;
				}

				if (_originalVerts != null && totalVertexCount != _originalVerts.VertexCount)
				{
					_originalVerts = null;
				}
				if (_originalVerts == null)
				{
					_originalVerts = new LayerVertices(totalVertexCount);
				}
			}

			// Store the original mesh vertices
			{
				int vertexOffset = 0;
				for (int i = 0; i < textInfo.materialCount; i++)
				{
					TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
					int vertexCount = meshInfo.vertexCount;
					// NOTE: that meshInfo.vertices (etc) can be larger than meshInfo.vertexCount because TMP over allocates (power-of-2 sizes) to prevent frequent reallocation.
					if (vertexCount > 0)
					{
						if (_targetMesh == TargetMesh.TextMeshPro)
						{
							System.Array.Copy(meshInfo.vertices, 0, _originalVerts.positions, vertexOffset, vertexCount);
							System.Array.Copy(meshInfo.colors32, 0, _originalVerts.colors, vertexOffset, vertexCount);
						}
						else
						{
							System.Array.Copy(meshInfo.mesh.vertices, 0, _originalVerts.positions, vertexOffset, vertexCount);
							System.Array.Copy(meshInfo.mesh.colors32, 0, _originalVerts.colors, vertexOffset, vertexCount);
						}
						if (_originalVerts.uvs0 != null) { System.Array.Copy(meshInfo.uvs0, 0, _originalVerts.uvs0, vertexOffset, vertexCount); }
						if (_originalVerts.uvs0_v4 != null) { System.Array.Copy(meshInfo.uvs0, 0, _originalVerts.uvs0_v4, vertexOffset, vertexCount); }
						System.Array.Copy(meshInfo.uvs2, 0, _originalVerts.uvs1, vertexOffset, vertexCount);
						vertexOffset += vertexCount;
					}
				}
			}

			if (IsTrackingTransform() && IsTrackingVertices())
			{
				// Convert to world space
				int vertexCount = _originalVerts.VertexCount;
				Matrix4x4 localToWorld = this.transform.localToWorldMatrix;
				for (int i = 0; i < vertexCount; i++)
				{
					_originalVerts.positions[i] = localToWorld.MultiplyPoint3x4(_originalVerts.positions[i]);
				}
			}
		}

		void SetupLayer(TrailLayer layer, int layerIndex)
		{
			if (IsTrackingTransform())
			{
				if (layerIndex > 0)
				{
					// Use the last layer matrix
					layer.matrix = _layers[layerIndex - 1].matrix;
				}
				else
				{
					// Use the current matrix
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
			// Generate and prime layer vertex arrays
			if (layer.vertices == null)
			{
				layer.vertices = new LayerVertices(_originalVerts.VertexCount);
				if (layerIndex > 0)
				{
					// Use the last layer vertices
					_layers[layerIndex - 1].vertices.CopyTo(layer.vertices);
				}
				else
				{
					// Use the current vertices
					_originalVerts.CopyTo(layer.vertices);
				}
			}
			// If vertex count has changed, try to preserve existing vertex data
			else if (layer.vertices.VertexCount != _originalVerts.VertexCount)
			{
				LayerVertices oldVertices = layer.vertices;
				layer.vertices = new LayerVertices(_originalVerts.VertexCount);

				if (layer.vertices.VertexCount > 0)
				{	
					if (layerIndex == 0)
					{
						_originalVerts.CopyTo(layer.vertices);
					}
					else
					{
						// Copy from old vertices
						oldVertices.CopyTo(layer.vertices, 0, Mathf.Min(layer.vertices.VertexCount, oldVertices.VertexCount));
				
						// More vertices added
						if (layer.vertices.VertexCount > oldVertices.VertexCount)
						{
							// Copy from new vertices
							if (layerIndex > 0)
							{
								// Use the last layer vertices
								_layers[layerIndex - 1].vertices.CopyTo(oldVertices.VertexCount, layer.vertices, oldVertices.VertexCount, (_originalVerts.VertexCount - oldVertices.VertexCount));
								_originalVerts.CopyTo(oldVertices.VertexCount, layer.vertices, oldVertices.VertexCount, (_originalVerts.VertexCount - oldVertices.VertexCount));
							}
							else
							{
								// Use the current vertices
								_originalVerts.CopyTo(oldVertices.VertexCount, layer.vertices, oldVertices.VertexCount, (_originalVerts.VertexCount - oldVertices.VertexCount));
							}
						}
					}
				}
			}
		}

		void AddTrailLayer()
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
			// Update vertices on existing layers if the count has changed
			if (_layers != null && _layers.Count > 0 && _originalVerts != null)
			{
				if (_layers[0].vertices != null && _layers[0].vertices.VertexCount != _originalVerts.VertexCount && IsTrackingVertices())
				{
					for (int i = 0; i < _layers.Count; i++)
					{
						SetupLayerVertices(_layers[i], i);
					}
				}
			}
			
			// Add / Remove trail layers
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

			// Create trail layers
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
				// first trail layer chases the original (current) matrix
				Matrix4x4 targetMatrix = this.transform.localToWorldMatrix;
				for (int j = 0; j < _layerCount; j++)
				{
					TrailLayer layer = _layers[j];
					MathUtils.LerpUnclamped(ref layer.matrix, targetMatrix, t, true);

					// other trail layers chase the previous matrix
					targetMatrix = layer.matrix;

					t += tStep;
				}
			}

			if (IsTrackingVertices())
			{
				float t = tStart;
				// first trail layer vertices chase the original vertices
				LayerVertices targetVertices = _originalVerts;
				for (int j = 0; j < _layerCount; j++)
				{
					TrailLayer layer = _layers[j];

					LayerVertices sourceVertices = layer.vertices;
				
					int vertexCount = _originalVerts.VertexCount;
					for (int i = 0; i < vertexCount; i++)
					{
						sourceVertices.positions[i] = Vector3.LerpUnclamped(sourceVertices.positions[i], targetVertices.positions[i], t);
						// NOTE: In most cases interpolating UVs doesn't make sense, for example with text.. perhaps in the future this can be exposed as an option.
						//sourceVertices.uvs0[i] = Vector2.LerpUnclamped(sourceVertices.uvs0[i], targetVertices.uvs0[i], t);
						//sourceVertices.uvs1[i] = Vector2.LerpUnclamped(sourceVertices.uvs1[i], targetVertices.uvs1[i], t);
						sourceVertices.uvs1[i] = targetVertices.uvs1[i];
						sourceVertices.colors[i] = Color.LerpUnclamped(sourceVertices.colors[i], targetVertices.colors[i], t);
					}

					if (s_isUV0Vector4)
					{
						Debug.Assert(sourceVertices.uvs0_v4.Length == vertexCount);
						targetVertices.uvs0_v4.CopyTo(sourceVertices.uvs0_v4, 0);
					}
					else
					{
						Debug.Assert(sourceVertices.uvs0.Length == vertexCount);
						targetVertices.uvs0.CopyTo(sourceVertices.uvs0, 0);
					}

					// other trail layers chase the previous vertices
					targetVertices = sourceVertices;

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
					// Prevent divide by zero
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

		void GenerateTrailGeometry()
		{
			int layerVertexCount = _originalVerts.VertexCount;

			// Allocate for output mesh vertices if we have to
			{
				int trailVertexCount = layerVertexCount * _layerCount;
				if (!_showTrailOnly)
				{
					trailVertexCount += layerVertexCount;
				}

				if (_outputVerts != null && trailVertexCount != _outputVerts.VertexCount)
				{
					_outputVerts = null;
				}
				if (_outputVerts == null)
				{
					_outputVerts = new LayerVertices(trailVertexCount);
				}
			}

			Matrix4x4 worldToLocal = this.transform.worldToLocalMatrix;

			// Add trail vertices (in back-to-front order for correct alpha blending)
			int k = 0;
			int vertexOffset = 0;
			for (int j = (_layerCount - 1); j >= 0; j--)
			{
				TrailLayer layer = _layers[j];

				if (IsTrackingTransform())
				{
					Matrix4x4 xform = worldToLocal;
					LayerVertices vertices = layer.vertices;
					if (!IsTrackingVertices())
					{
						Matrix4x4 localToWorld = layer.matrix;
						xform = worldToLocal * localToWorld;
						vertices = _originalVerts;
					}

					for (int i = 0; i < layerVertexCount; i++)
					{
						Vector3 pp = xform.MultiplyPoint3x4(vertices.positions[i]);
						_outputVerts.positions[k] = pp;
						Color cc = ColorUtils.Blend(vertices.colors[i], layer.color, _blendMode);
						cc.a *= layer.alpha;
						_outputVerts.colors[k] = cc;
						k++;
					}
					if (s_isUV0Vector4) { System.Array.Copy(vertices.uvs0_v4, 0, _outputVerts.uvs0_v4, vertexOffset, layerVertexCount); }
					else { System.Array.Copy(vertices.uvs0, 0, _outputVerts.uvs0, vertexOffset, layerVertexCount); }
					System.Array.Copy(vertices.uvs1, 0, _outputVerts.uvs1, vertexOffset, layerVertexCount);
					vertexOffset += layerVertexCount;
				}
				else
				{
					for (int i = 0; i < layerVertexCount; i++)
					{
						Color cc = ColorUtils.Blend(layer.vertices.colors[i], layer.color, _blendMode);
						cc.a *= layer.alpha;
						_outputVerts.colors[k] = cc;
						k++;
					}
					System.Array.Copy(layer.vertices.positions, 0, _outputVerts.positions, vertexOffset, layerVertexCount);
					if (s_isUV0Vector4)  { System.Array.Copy(layer.vertices.uvs0_v4, 0, _outputVerts.uvs0_v4, vertexOffset, layerVertexCount); }
					else { System.Array.Copy(layer.vertices.uvs0, 0, _outputVerts.uvs0, vertexOffset, layerVertexCount); }
					System.Array.Copy(layer.vertices.uvs1, 0, _outputVerts.uvs1, vertexOffset, layerVertexCount);
					vertexOffset += layerVertexCount;
				}
			}

			// Add the original vertices
			if (!_showTrailOnly)
			{
				if (IsTrackingTransform() && IsTrackingVertices())
				{
					for (int i = 0; i < layerVertexCount; i++)
					{
						_outputVerts.positions[k] = worldToLocal.MultiplyPoint3x4(_originalVerts.positions[i]);
						k++;
					}
				}
				else
				{
					System.Array.Copy(_originalVerts.positions, 0, _outputVerts.positions, vertexOffset, layerVertexCount);
				}
				
				if (s_isUV0Vector4) { System.Array.Copy(_originalVerts.uvs0_v4, 0, _outputVerts.uvs0_v4, vertexOffset, layerVertexCount); }
				else { System.Array.Copy(_originalVerts.uvs0, 0, _outputVerts.uvs0, vertexOffset, layerVertexCount); }
				System.Array.Copy(_originalVerts.uvs1, 0, _outputVerts.uvs1, vertexOffset, layerVertexCount);
				System.Array.Copy(_originalVerts.colors, 0, _outputVerts.colors, vertexOffset, layerVertexCount);
			}
		}

		void AssignTrailGeometryToMesh(TMP_TextInfo textInfo)
		{
			// NOTE: The TMP component has a master Mesh which textInfo.meshInfo[0].mesh is assigned to.
			// For materialIndex > 0, TMP uses SubMeshes which are hidden GameObjects so that a different
			// material can be used for rendering.  Each of these submesh has a mesh assigned to the
			// textInfo.meshInfo[i>0].mesh.

			if (textInfo.materialCount == 1)
			{
				TMP_MeshInfo meshInfo = textInfo.meshInfo[0];

				int oldQuadCount = meshInfo.vertexCount / 4;
				int newQuadCount = (oldQuadCount * _layerCount);
				if (!_showTrailOnly)
				{
					newQuadCount += oldQuadCount;
				}
				
				// Resize geometry arrays and mesh
				// TODO: this can be optimised so we don't need to resize (generages garbage)
				// We just need to store own own Mesh and set the CanvasRenderer to use this
				// The tricky part will be the submeshes as there is no direct way to access them,
				// we would have to search and store them which isn't fast - or perhaps there is a
				// submesh create/destroy event we could hook into?
				meshInfo.ResizeMeshInfo(newQuadCount);

				// Assign to the mesh
				meshInfo.mesh.triangles = meshInfo.triangles;
				meshInfo.mesh.vertices = _outputVerts.positions;
				meshInfo.mesh.SetColors(_outputVerts.colors);
				if (s_isUV0Vector4)
				{
					meshInfo.mesh.SetUVs(0, _outputVerts.uvs0_v4);
				}
				else
				{
					meshInfo.mesh.SetUVs(0, _outputVerts.uvs0);
				}
				meshInfo.mesh.SetUVs(1, _outputVerts.uvs1);

				_textMeshPro.UpdateGeometry(meshInfo.mesh, 0);
			}
			else if (textInfo.materialCount > 1)
			{
				int totalQuadsPerLayer = _originalVerts.VertexCount / 4;

				int startSrcOffset = 0;
				for (int i = 0; i < textInfo.materialCount; i++)
				{
					TMP_MeshInfo meshInfo = textInfo.meshInfo[i];

					int layerCount = _layerCount;
					int oldQuadCount = meshInfo.vertexCount / 4;

					// Resize geometry arrays and mesh
					{
						if (!_showTrailOnly)
						{
							layerCount++;
						}
						int newQuadCount = (oldQuadCount * layerCount);
						// TODO: this can be optimised so we don't need to resize (generages garbage)
						// We just need to store own own Mesh and set the CanvasRenderer to use this
						// The tricky part will be the submeshes as there is no direct way to access them,
						// we would have to search and store them which isn't fast - or perhaps there is a
						// submesh create/destroy event we could hook into?
						meshInfo.ResizeMeshInfo(newQuadCount);
					}

					// Copy the vertices (quads)
					for (int jj = 0; jj < layerCount; jj++)
					{
						int ll = (startSrcOffset + jj * totalQuadsPerLayer) * 4;
						int kk = jj * oldQuadCount * 4;
						System.Array.Copy(_outputVerts.positions, ll, meshInfo.vertices, kk, oldQuadCount * 4);
						if (s_isUV0Vector4) { System.Array.Copy(_outputVerts.uvs0_v4, ll, meshInfo.uvs0, kk, oldQuadCount * 4); }
						else { System.Array.Copy(_outputVerts.uvs0, ll, meshInfo.uvs0, kk, oldQuadCount * 4); }
						System.Array.Copy(_outputVerts.uvs1, ll, meshInfo.uvs2, kk, oldQuadCount * 4);
						System.Array.Copy(_outputVerts.colors, ll, meshInfo.colors32, kk, oldQuadCount * 4);
					}
					startSrcOffset += oldQuadCount;

					meshInfo.mesh.triangles = meshInfo.triangles;
					meshInfo.mesh.vertices = meshInfo.vertices;
					meshInfo.mesh.SetColors(meshInfo.colors32);
					meshInfo.mesh.SetUVs(0, meshInfo.uvs0);
					meshInfo.mesh.SetUVs(1, meshInfo.uvs2);

					_textMeshPro.UpdateGeometry(meshInfo.mesh, i);
				}
			}
		}

		private int _lastRenderFrame = -1;

		void WillRenderCanvases()
		{
			if (_textMeshPro == null || !_textMeshPro.IsActive()) return;
			if (_targetMesh != TargetMesh.Internal) return;

			if (CanApply() && HasGeometryToProcess())
			{
				if (_lastRenderFrame != Time.frameCount)
				{
					// Prevent re-rendering unnecessarily
					_lastRenderFrame = Time.frameCount;

					ApplyOutputMeshAndMaterial(_textMeshPro.textInfo);
				}
			}
		}

		private void PrepareMesh(TMP_TextInfo textInfo)
		{
			if (_mesh == null)
			{
				_mesh = new Mesh();
			}

			{
				int layerCount = _layerCount;
				TMP_MeshInfo meshInfo = textInfo.meshInfo[0];
				int oldQuadCount = meshInfo.vertexCount / 4;

				// Resize geometry arrays and mesh
				if (!_showTrailOnly)
				{
					layerCount++;
				}
				int newQuadCount = (oldQuadCount * layerCount);
				if (_triangleIndices == null || _triangleIndices.Length != (newQuadCount * 6))
				{
					_mesh.Clear();
					_mesh.MarkDynamic();
					_triangleIndices = new int[newQuadCount * 6];
					for (int i = 0; i < newQuadCount; i++)
					{
						int index_X4 = i * 4;
						int index_X6 = i * 6;
						_triangleIndices[0 + index_X6] = 0 + index_X4;
						_triangleIndices[1 + index_X6] = 1 + index_X4;
						_triangleIndices[2 + index_X6] = 2 + index_X4;
						_triangleIndices[3 + index_X6] = 2 + index_X4;
						_triangleIndices[4 + index_X6] = 3 + index_X4;
						_triangleIndices[5 + index_X6] = 0 + index_X4;
					}
				}
			}
		}

		private void ApplyOutputMeshAndMaterial(TMP_TextInfo textInfo)
		{
			// Update the mesh
			{
				PrepareMesh(textInfo);
				_mesh.vertices = _outputVerts.positions;
				_mesh.triangles = _triangleIndices;
				_mesh.SetColors(_outputVerts.colors);
				if (s_isUV0Vector4) 
				{
					_mesh.SetUVs(0, _outputVerts.uvs0_v4);
				}
				else
				{
					_mesh.SetUVs(0, _outputVerts.uvs0);
				}
				_mesh.SetUVs(1, _outputVerts.uvs1);
			}
			
			var cr = _textMeshPro.canvasRenderer;
			cr.SetMesh(_mesh);
		}
	}
}
#endif