// Unity
using UnityEngine;

namespace GUPS.EasyPooling.Policy.Random
{
    /// <summary>
    /// Provides methods for generating random values.
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Generates a random floating-point number between -1 (inclusive) and 1 (exclusive).
        /// </summary>
        /// <returns>A random floating-point number.</returns>
        float GetRandom();

        /// <summary>
        /// Generates a random floating-point number between the specified values.
        /// </summary>
        /// <param name="_MinInclusive">The minimum value (inclusive).</param>
        /// <param name="_MaxExclusive">The maximum value (exclusive).</param>
        /// <returns></returns>
        float GetRandom(float _MinInclusive, float _MaxExclusive);

        /// <summary>
        /// Generates a random 2D vector with components between -1 (inclusive) and 1 (exclusive).
        /// </summary>
        /// <returns>A random 2D vector.</returns>
        Vector2 GetRandomVector2();

        /// <summary>
        /// Generates a random 3D vector with components between -1 (inclusive) and 1 (exclusive).
        /// </summary>
        /// <returns>A random 3D vector.</returns>
        Vector3 GetRandomVector3();
    }

}
