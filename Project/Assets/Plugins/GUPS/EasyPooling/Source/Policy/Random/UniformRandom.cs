// Unity
using UnityEngine;

namespace GUPS.EasyPooling.Policy.Random
{
    /// <summary>
    /// Generates random points distributed uniformly across space, ensuring that any region of the space is equally likely to contain a point.
    /// </summary>
    public class UniformRandom : IRandom
    {
        private System.Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniformRandom"/> class with a random seed.
        /// </summary>
        public UniformRandom()
        {
            this.random = new System.Random();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniformRandom"/> class with the specified seed.
        /// </summary>
        /// <param name="_Seed">The seed value for randomization.</param>
        public UniformRandom(int _Seed)
        {
            this.random = new System.Random(_Seed);
        }

        /// <summary>
        /// Generates a random float value.
        /// </summary>
        /// <returns>Generates a random floating-point number between -1 (inclusive) and 1 (exclusive).</returns>
        public float GetRandom()
        {
            return (float)(this.random.NextDouble() - 0.5) * 2f;
        }

        /// <summary>
        ///  Generates a random float value between the specified values.
        /// </summary>
        /// <param name="_MinInclusive">The minimum value (inclusive).</param>
        /// <param name="_MaxExclusive">The maximum value (exclusive).</param>
        /// <returns>Generates a random floating-point number between the passed min (inclusive) and max (exclusive) value.</returns>
        public float GetRandom(float _MinInclusive, float _MaxExclusive)
        {
            float var_Deviation = _MaxExclusive - _MinInclusive;

            return (this.GetRandom() + 1f) * 0.5f * var_Deviation + _MinInclusive;
        }

        /// <summary>
        /// Generates a random 2D vector.
        /// </summary>
        /// <returns>A random 2D vector with numbers between -1 (inclusive) and 1 (exclusive).</returns>
        public Vector2 GetRandomVector2()
        {
            float x = this.GetRandom();
            float y = this.GetRandom();

            return new Vector2(x, y);
        }

        /// <summary>
        /// Generates a random 3D vector.
        /// </summary>
        /// <returns>A random 3D vector with numbers between -1 (inclusive) and 1 (exclusive).</returns>
        public Vector3 GetRandomVector3()
        {
            float x = this.GetRandom();
            float y = this.GetRandom();
            float z = this.GetRandom();

            return new Vector3(x, y, z);
        }
    }
}
