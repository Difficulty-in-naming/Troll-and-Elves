using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityInternal = UnityEngine.Internal;

namespace Panthea.UIFX
{
	/// <summary>当强度 < 1.0 时，用于淡出轨迹的模式</summary>
	public enum TrailStrengthMode
	{
		/// <summary>`Damping` - 减少阻尼，使得当强度 == 0.0 时，轨迹没有滞后。</summary>
		Damping,
		/// <summary>`Layers` - 从后向前移除每一层，使得当强度 == 0 时，没有层可见。</summary>
		Layers,
		/// <summary>`FadeLayers` - 与 `Layers` 相同，但使用淡出而不是硬切。</summary>
		FadeLayers,
		/// <summary>`Fade` - 同时淡出整个轨迹。</summary>
		Fade,
	}

	[RequireComponent(typeof(Graphic))]
	public abstract class TrailEffectBase : UIBehaviour, IMeshModifier
	{
		[UnityInternal.ExcludeFromDocs]
		[LabelText("轨迹层数")]
		[TitleGroup("轨迹",Indent = true,HorizontalLine = false)]
		[SerializeField, Range(0f, 64f)] protected int _layerCount = 16;

		[UnityInternal.ExcludeFromDocs]
		[Tooltip("轨迹前端跟随移动的速度。数值越高，轨迹越不滞后。默认值为50，范围是[0..250]。")]
		[LabelText("轨迹前端跟随速度")]
		[TitleGroup("轨迹",Indent = true,HorizontalLine = false)]
		[SerializeField, Range(0f, 250f)] protected float _dampingFront = 50f;

		[UnityInternal.ExcludeFromDocs]
		[Tooltip("轨迹后端跟随移动的速度。数值越高，轨迹越不滞后。默认值为50，范围是[0..250]。")]
		[LabelText("轨迹后端跟随速度")]
		[TitleGroup("轨迹",Indent = true,HorizontalLine = false)]
		[SerializeField, Range(0f, 250f)] protected float _dampingBack = 50f;

		[UnityInternal.ExcludeFromDocs]
		[Tooltip("可选的透明度曲线。透明度也可以通过渐变属性控制，但在渐变动画时，使用此二次控制仍然有用，以便应用静态透明度衰减。")]
		[LabelText("透明度曲线")]
		[TitleGroup("轨迹",Indent = true,HorizontalLine = false)]
		[SerializeField] protected AnimationCurve _alphaCurve = new AnimationCurve(new Keyframe(0f, 1f, -1f, -1f), new Keyframe(1f, 0f, -1f, -1f));

		[UnityInternal.ExcludeFromDocs]
		[Tooltip("用于计算顶点修改器效果的顶点修改器。TransformAndVertex是最耗性能的。")]
		[LabelText("顶点修改器")]
		[TitleGroup("轨迹",Indent = true,HorizontalLine = false)]
		[SerializeField] protected VertexModifierSource _vertexModifierSource = VertexModifierSource.Transform;

		[UnityInternal.ExcludeFromDocs]
		[Tooltip("轨迹使用的渐变颜色")]
		[LabelText("渐变颜色")]
		[TitleGroup("渐变",Indent = true,HorizontalLine = false)]
		[SerializeField] protected Gradient _gradient = ColorUtils.GetBuiltInGradient(BuiltInGradient.SoftRainbow);

		[UnityInternal.ExcludeFromDocs]
		[Tooltip("应用于渐变的偏移量。渐变将使用镜像重复进行包裹。")]
		[LabelText("渐变偏移")]
		[TitleGroup("渐变",Indent = true,HorizontalLine = false)]
		[SerializeField] protected float _gradientOffset = 0f;

		[UnityInternal.ExcludeFromDocs]
		[Tooltip("应用于渐变的缩放比例。渐变将使用镜像重复进行包裹。")]
		[LabelText("渐变缩放")]
		[TitleGroup("渐变",Indent = true,HorizontalLine = false)]
		[SerializeField] protected float _gradientScale = 1f;

		[UnityInternal.ExcludeFromDocs]
		[Tooltip("渐变偏移属性的动画速度。允许在不使用脚本的情况下轻松实现简单的滚动动画。设置为零则不进行动画。")]
		[LabelText("渐变偏移动画速度")]
		[TitleGroup("动画",Indent = true,HorizontalLine = false)]
		[SerializeField] protected float _gradientOffsetSpeed = 0f;

		[UnityInternal.ExcludeFromDocs]
		[Tooltip("仅显示轨迹，隐藏原始UI图形")]
		[LabelText("仅显示轨迹")]
		[TitleGroup("应用",Indent = true,HorizontalLine = false)]
		[SerializeField] protected bool _showTrailOnly = false;

		[UnityInternal.ExcludeFromDocs]
		[Tooltip("用于混合原始顶点颜色和渐变颜色的颜色混合模式")]
		[LabelText("颜色混合模式")]
		[TitleGroup("应用",Indent = true,HorizontalLine = false)]
		[SerializeField] protected BlendMode _blendMode = BlendMode.Multiply;

		[UnityInternal.ExcludeFromDocs]
		[Tooltip("当强度 < 1.0 时，用于淡出轨迹的模式")]
		[LabelText("轨迹强度模式")]
		[TitleGroup("应用",Indent = true,HorizontalLine = false)]
		[SerializeField] protected TrailStrengthMode _strengthMode = TrailStrengthMode.FadeLayers;

		[UnityInternal.ExcludeFromDocs]
		[Tooltip("效果的强度。范围[0..1]")]
		[LabelText("效果强度")]
		[TitleGroup("应用",Indent = true,HorizontalLine = false)]
		[SerializeField, Range(0f, 1f)] protected float _strength = 1f;

		/// <summary>轨迹层数</summary>
		public int LayerCount { get { return _layerCount; } set { _layerCount = Mathf.Max(0, value); } }

		/// <summary>轨迹前端跟随移动的速度。数值越高，轨迹越不滞后。默认值为50，范围是[0..250]。</summary>
		public float DampingFront { get { return _dampingFront; } set { _dampingFront = Mathf.Max(0f, value); } }

		/// <summary>轨迹后端跟随移动的速度。数值越高，轨迹越不滞后。默认值为50，范围是[0..250]。</summary>
		public float DampingBack { get { return _dampingBack; } set { _dampingBack = Mathf.Max(0f, value); } }

		/// <summary>可选的透明度曲线。透明度也可以通过渐变属性控制，但在渐变动画时，使用此二次控制仍然有用，以便应用静态透明度衰减。</summary>
		public AnimationCurve AlphaCurve { get { return _alphaCurve; } set { _alphaCurve = value; } }

		/// <summary>用于计算顶点修改器效果的顶点修改器。TransformAndVertex是最耗性能的。</summary>
		public VertexModifierSource VertexModifierSource { get { return _vertexModifierSource; } set { _vertexModifierSource = value; OnChangedVertexModifier(); } }

		/// <summary>轨迹使用的渐变颜色</summary>
		public Gradient Gradient { get { return _gradient; } set { _gradient = value; } }

		/// <summary>应用于渐变的偏移量。渐变将使用镜像重复进行包裹。</summary>
		public float GradientOffset { get { return _gradientOffset; } set { _gradientOffset = value; } }

		/// <summary>应用于渐变的缩放比例。渐变将使用镜像重复进行包裹。</summary>
		public float GradientScale { get { return _gradientScale; } set { _gradientScale = value; } }

		/// <summary>渐变偏移属性的动画速度。允许在不使用脚本的情况下轻松实现简单的滚动动画。设置为零则不进行动画。</summary>
		public float GradientOffsetSpeed { get { return _gradientOffsetSpeed; } set { _gradientOffsetSpeed = value; } }

		/// <summary>仅显示轨迹，隐藏原始UI图形</summary>
		public bool ShowTrailOnly { get { return _showTrailOnly; } set { _showTrailOnly = value; } }

		/// <summary>用于混合原始顶点颜色和渐变颜色的颜色混合模式</summary>
		public BlendMode BlendMode { get { return _blendMode; } set { _blendMode = value; } }

		/// <summary>当强度 < 1.0 时，用于淡出轨迹的模式</summary>
		public TrailStrengthMode StrengthMode { get { return _strengthMode; } set { _strengthMode = value; } }

		/// <summary>效果的强度。范围[0..1]</summary>
		public float Strength { get { return _strength; } set { _strength = Mathf.Clamp01(value); } }

		[UnityInternal.ExcludeFromDocs]
		protected Graphic _graphic;
		protected Graphic GraphicComponent { get { if (_graphic == null) { _graphic = GetComponent<Graphic>(); } return _graphic; } }

		[UnityInternal.ExcludeFromDocs]
		protected MaskableGraphic _maskableGraphic;
		protected MaskableGraphic MaskableGraphicComponent { get { if (_maskableGraphic == null) { _maskableGraphic = GraphicComponent as MaskableGraphic; } return _maskableGraphic; } }

		[UnityInternal.ExcludeFromDocs]
		protected CanvasRenderer _canvasRenderer;
		protected CanvasRenderer CanvasRenderComponent { get { if (_canvasRenderer == null) { if (GraphicComponent) { _canvasRenderer = _graphic.canvasRenderer; } else { _canvasRenderer = GetComponent<CanvasRenderer>(); } } return _canvasRenderer; } }

		/// <summary>如果忽略时间缩放，则使用 Time.unscaledDeltaTime 更新动画</summary>
		public static bool IgnoreTimeScale { get; set ; }

		protected static float DeltaTime { get { return (IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime); } }

		protected virtual void SetDirty()
		{
		}

		/// <summary>
		/// 重置轨迹，从当前状态（变换/顶点位置）重新开始。
		/// 这在重置变换时很有用，以防止轨迹在最后一个位置和新位置之间错误地绘制。
		/// </summary>
		public virtual void ResetMotion()
		{

		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			SetDirty();
			base.OnValidate();
		}
#endif

		protected override void OnDidApplyAnimationProperties()
		{
			SetDirty();
			base.OnDidApplyAnimationProperties();
		}

		/// <summary>
		/// OnCanvasHierarchyChanged() 在 Canvas 启用/禁用时调用
		/// </summary>
		protected override void OnCanvasHierarchyChanged()
		{
			ResetMotion();
			SetDirty();
			base.OnCanvasHierarchyChanged();
		}

		protected bool CanApply()
		{
			if (!IsActive()) return false;
			if (!GraphicComponent.enabled) return false;
			if (_layerCount < 1) return false;
			if (GraphicComponent.canvas == null) return false;
			return true;
		}

		protected bool IsTrackingTransform()
		{
			return (_vertexModifierSource != VertexModifierSource.Vertex);
		}

		protected bool IsTrackingVertices()
		{
			return (_vertexModifierSource != VertexModifierSource.Transform);
		}

		[UnityInternal.ExcludeFromDocs]
		public virtual void ModifyMesh(VertexHelper vh)
		{
		}

		protected abstract void OnChangedVertexModifier();

		[UnityInternal.ExcludeFromDocs]
		[System.Obsolete("使用 IMeshModifier.ModifyMesh (VertexHelper verts) 代替", false)]
		public void ModifyMesh(Mesh mesh)
		{
			throw new System.NotImplementedException("使用 IMeshModifier.ModifyMesh (VertexHelper verts) 代替");
		}
	}
}