// Unity
using UnityEngine;

// GUPS
using GUPS.EasyPooling.Pooling;

namespace GUPS.EasyPooling
{
    /// <summary>
    /// Represents a default implementation of the <see cref="IPoolAble"/> interface for making a GameObject poolable.
    /// This implementation does nothing in terms of custom behavior, serving as a simple marker for poolable GameObjects.
    /// For specific and custom functionality, it is recommended to implement the <see cref="IPoolAble"/> interface directly
    /// on your own MonoBehaviour and define the desired behaviors in the corresponding methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DefaultPoolAble"/> class extends <see cref="MonoBehaviour"/> and implements the <see cref="IPoolAble"/> interface.
    /// It provides default, empty implementations for each method defined in the <see cref="IPoolAble"/> interface: <see cref="OnCreate"/>,
    /// <see cref="OnSpawn"/>, <see cref="OnDespawn"/>, and <see cref="OnDestroy"/>.
    /// </para>
    /// </remarks>
    public class DefaultPoolAble : MonoBehaviour, IPoolAble
    {
        /// <inheritdoc/>
        public IPool Owner { get; set; }

        /// <inheritdoc/>
        public bool IsPooled { get; set; }

        /// <inheritdoc/>
        public void OnCreate()
        {
            // Does nothing.
        }

        /// <inheritdoc/>
        public void OnSpawn()
        {
            // Does nothing.
        }

        /// <inheritdoc/>
        public void OnDespawn()
        {
            // Does nothing.
        }

        /// <inheritdoc/>
        public void OnDestroy()
        {
            // Does nothing.
        }
    }
}
