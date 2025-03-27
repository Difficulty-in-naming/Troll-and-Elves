// Unity
using UnityEngine;

namespace GUPS.EasyPooling.Decorator
{
    /// <summary>
    /// Implementation of <see cref="IUnDecorator"/> that removes all components from a GameObject, but leaves the Transform. Is called after IPoolAble.OnDespawn.
    /// </summary>
    public class FullUnDecorator : IUnDecorator
    {
        /// <summary>
        /// Removes all components from a GameObject, but leaves the Transform.
        /// </summary>
        /// <param name="_GameObject">The GameObject to un-decorate.</param>
        public void OnUnDecorate(GameObject _GameObject)
        {
            // Remove all components except the Transform, bottom up in case of references.
            Component[] var_Components = _GameObject.GetComponents<Component>();
            for (int i = var_Components.Length - 1; i >= 0; i--)
            {
                if (!(var_Components[i] is Transform))
                {
                    Object.Destroy(var_Components[i]);
                }
            }
        }
    }
}
