// Unity
using UnityEngine;

namespace GUPS.EasyPooling.Decorator
{
    /// <summary>
    /// Represents an interface for reverting the decoration applied to a GameObject. 
    /// This interface is called after the <see cref="IPoolAble.OnDespawn"/> method.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IUnDecorator"/> interface defines methods for removing previously applied decoration from a GameObject.
    /// Implementations of this interface are called during the object despawning process, specifically after the 
    /// <see cref="IPoolAble.OnDespawn"/> method, to revert any modifications made during the decoration process.
    /// </para>
    /// </remarks>
    public interface IUnDecorator
    {
        /// <summary>
        /// Called to remove decoration to a GameObject.
        /// </summary>
        /// <param name="_GameObject">The GameObject to un-decorate.</param>
        void OnUnDecorate(GameObject _GameObject);
    }
}