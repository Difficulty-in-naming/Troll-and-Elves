namespace GUPS.EasyPooling
{
    /// <summary>
    /// Manages a scene-specific local pool of GameObjects to facilitate efficient object reuse.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="ScenePool"/> class extends the functionality of the <see cref="AGamePool{T}"/> base class and serves
    /// as a specialized pool manager for GameObjects within a specific scene. It implements a local pool that enables the
    /// efficient reuse of GameObject instances, reducing the overhead of constant object instantiation and destruction.
    /// </para>
    /// <para>
    /// Scene pools are designed to be scene-specific, meaning that they persist only throughout the duration of a single
    /// scene. This behavior is particularly useful when certain GameObjects need to be recycled within the context of a
    /// specific scene without impacting the overall application state.
    /// </para>
    /// </remarks>
    public class ScenePool : AGamePool<ScenePool>
    {
        /// <summary>
        /// The Scene Pool is only persistent throughout the scene.
        /// </summary>
        public override bool IsPersistent => false;
    }
}
