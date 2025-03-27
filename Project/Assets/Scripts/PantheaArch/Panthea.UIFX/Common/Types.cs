namespace Panthea.UIFX
{
	/// <summary>用于计算顶点修改器效果的顶点修改器源。</summary>
	public enum VertexModifierSource
	{
		/// <summary>仅变换变化影响效果。</summary>
		Transform,
		/// <summary>仅顶点变化（通常通过 IMeshModifier 效果）影响效果。</summary>
		Vertex,
		/// <summary>变换变化和顶点变化（通常通过 IMeshModifier 效果）都影响效果。这是最昂贵的模式。</summary>
		TranformAndVertex,
	}

	/// <summary>描述渐变如何包裹的模式。</summary>
	public enum GradientWrapMode
	{
		/// <summary>无包裹，边缘值将被使用。</summary>
		None,
		/// <summary>渐变重复。</summary>
		Wrap,
		/// <summary>渐变以镜像方式重复。</summary>
		Mirror,
	}

	/// <summary>如何对纹理进行下采样。</summary>
	public enum Downsample
	{
		/// <summary>自动下采样将取决于平台。</summary>
		Auto = 0,
		/// <summary>不下采样。</summary>
		None = 1,
		/// <summary>下采样到一半大小。</summary>
		Half = 2,
		/// <summary>下采样到四分之一大小。</summary>
		Quarter = 4,
		/// <summary>下采样到八分之一大小。</summary>
		Eighth = 8,
	}
}