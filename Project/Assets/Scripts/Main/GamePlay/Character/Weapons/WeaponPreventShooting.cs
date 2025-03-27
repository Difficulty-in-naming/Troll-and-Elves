using EdgeStudio;

namespace MoreMountains.TopDownEngine
{
    public abstract class WeaponPreventShooting : TopDownMonoBehaviour
    {
        /// <summary>
        /// Override this method to define shooting conditions
        /// </summary>
        /// <returns></returns>
        public abstract bool ShootingAllowed();
    }
}