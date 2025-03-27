namespace GUPS.EasyPooling
{
    /// <summary>
    /// Manages a global pool of GameObjects to optimize object reuse across the entire application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="GlobalPool"/> class extends the functionality of the <see cref="AGamePool{T}"/> base class and serves
    /// as a centralized manager for GameObjects, providing an efficient mechanism for object reuse throughout the entire
    /// application lifecycle. It implements a global pool that enables the recycling of GameObject instances, minimizing the
    /// overhead associated with constant object creation and destruction.
    /// </para>
    /// <para>
    /// Global pools are designed to be persistent across the whole application, making them suitable for scenarios where
    /// certain GameObjects need to be reused consistently across different scenes and states of the application.
    /// </para>
    /// </remarks>
    public class GlobalPool : AGamePool<GlobalPool>
    {
        /// <summary>
        /// The Global Pool is persistent across the whole application.
        /// </summary>
        public override bool IsPersistent => true;
    }
}