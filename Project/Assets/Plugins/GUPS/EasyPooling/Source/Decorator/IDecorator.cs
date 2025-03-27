// Unity
using UnityEngine;

namespace GUPS.EasyPooling.Decorator
{
    /// <summary>
    /// Represents an interface for decorators that can be applied to GameObjects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IDecorator"/> interface defines methods for applying decoration to GameObjects.
    /// Decorators can be used to modify the appearance, behavior, or other properties of GameObjects based on specific requirements.
    /// </para>
    /// </remarks>
    public interface IDecorator
    {
        /// <summary>
        /// Gets a value indicating whether this decorator should only be applied when a new GameObject is created.
        /// </summary>
        bool OnCreateOnly { get; }

        /// <summary>
        /// Called to apply decoration to a GameObject.
        /// </summary>
        /// <param name="_GameObject">The GameObject to decorate.</param>
        void OnDecorate(GameObject _GameObject);
    }
}