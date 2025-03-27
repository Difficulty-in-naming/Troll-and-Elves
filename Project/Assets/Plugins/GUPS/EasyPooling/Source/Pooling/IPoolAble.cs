namespace GUPS.EasyPooling.Pooling
{
    /// <summary>
    /// Represents an interface for poolable objects, providing methods to manage object lifecycle events within and outside of a pool.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IPoolAble"/> interface defines a contract for objects that can be used for pooling. It includes properties to identify 
    /// the owning pool and the current pooled state, along with methods to handle key lifecycle events.
    /// </para>
    /// <para>
    /// Implementations of this interface are expected to handle events such as creation, spawning, despawning, and destruction,
    /// allowing for efficient management of object reuse within a pool.
    /// </para>
    /// </remarks>
    public interface IPoolAble
    {
        /// <summary>
        /// The pool this poolable object belongs to.
        /// </summary>
        IPool Owner { get; set; }

        /// <summary>
        /// Indicating whether this poolable object is inactive in its pool.
        /// </summary>
        bool IsPooled { get; set; }

        /// <summary>
        /// Called when the poolable object is newly created by the pool and not received from any already pooled object.
        /// </summary>
        void OnCreate();

        /// <summary>
        /// Called when the poolable object is spawned from the pool or after <see cref="OnCreate"/>.
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// Called when the poolable object is despawned and returned to the pool.
        /// </summary>
        void OnDespawn();

        /// <summary>
        /// Called when the poolable object is destroyed and will not return to the pool.
        /// </summary>
        void OnDestroy();
    }
}
