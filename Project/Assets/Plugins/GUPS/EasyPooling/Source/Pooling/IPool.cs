// Unity
using UnityEngine;

// GUPS
using GUPS.EasyPooling.Decorator;
using GUPS.EasyPooling.Strategy;

namespace GUPS.EasyPooling.Pooling
{
    /// <summary>
    /// Represents a generic pool interface for managing and reusing GameObjects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IPool"/> interface defines a contract for managing a pool of GameObjects, allowing for efficient object reuse.
    /// It includes methods to spawn and despawn GameObjects, each with various options for customization.
    /// </para>
    /// <para>
    /// The pool is characterized by its pooling strategy, capacity, and current count of GameObjects in the pool.
    /// </para>
    /// </remarks>
    public interface IPool
    {
        /// <summary>
        /// Gets the pooling strategy of the pool.
        /// </summary>
        EPoolingStrategy Strategy { get; }

        /// <summary>
        /// Gets the capacity of the pool.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Gets the number of GameObjects in the pool.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Spawns a GameObject using the default settings.
        /// </summary>
        GameObject Spawn();

        /// <summary>
        /// Spawns a GameObject at the specified position and rotation.
        /// </summary>
        /// <param name="_Position">The position at which to spawn the GameObject.</param>
        /// <param name="_Rotation">The rotation at which to spawn the GameObject.</param>
        GameObject Spawn(Vector3 _Position, Quaternion _Rotation);

        /// <summary>
        /// Spawns a GameObject with a provided decorator.
        /// </summary>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        GameObject Spawn(IDecorator _Decorator);

        /// <summary>
        /// Spawns a GameObject with a provided decorator at the specified position and rotation.
        /// </summary>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <param name="_Position">The position at which to spawn the GameObject.</param>
        /// <param name="_Rotation">The rotation at which to spawn the GameObject.</param>
        GameObject Spawn(IDecorator _Decorator, Vector3 _Position, Quaternion _Rotation);

        /// <summary>
        /// Despawns a poolable GameObject back into the pool.
        /// </summary>
        /// <param name="_PoolAble">The poolable GameObject to despawn.</param>
        void Despawn(GameObject _PoolAble);

        /// <summary>
        /// Despawns a poolable GameObject back into the pool using a provided un-decorator.
        /// </summary>
        /// <param name="_PoolAble">The poolable GameObject to despawn.</param>
        /// <param name="_UnDecorator">The un-decorator to apply to the despawned GameObject.</param>
        void Despawn(GameObject _PoolAble, IUnDecorator _UnDecorator);
    }
}
