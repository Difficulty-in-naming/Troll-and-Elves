// Unity
using UnityEngine;

namespace GUPS.EasyPooling.Policy.Random
{
    /// <summary>
    /// Generates random values using Perlin noise, a gradient noise function producing natural-looking random patterns.
    /// </summary>
    public class PerlinNoiseRandom : IRandom
    {
        private float range;
        private System.Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerlinNoiseRandom"/> class.
        /// </summary>
        /// <param name="_Range">The applicable range for the noise map used to calculate random values.</param>
        public PerlinNoiseRandom(float _Range)
        {
            this.range = _Range;
            this.random = new System.Random();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerlinNoiseRandom"/> class with the seed.
        /// </summary>
        /// <param name="_Range">The applicable range for the noise map used to calculate random values.</param>
        /// <param name="_Seed">The seed value for the random number generator.</param>
        public PerlinNoiseRandom(float _Range, int _Seed)
        {
            this.range = _Range;
            this.random = new System.Random(_Seed);
        }

        /// <summary>
        /// Generates a random floating-point number using Perlin noise.
        /// </summary>
        /// <returns>Generates a random floating-point number between -1 (inclusive) and 1 (exclusive).</returns>
        public float GetRandom()
        {
            float x = (float)this.random.NextDouble() * this.range;
            float y = (float)this.random.NextDouble() * this.range;
            float var_Result = (Mathf.PerlinNoise(x, y) - 0.5f) * 2f;

            var_Result = var_Result < -1.0f ? -1.0f : var_Result;
            var_Result = var_Result >= 1.0f ? 1.0f - 1.0e-32f : var_Result;

            return var_Result;
        }

        /// <summary>
        /// Generates a random floating-point number using Perlin noise between the specified values.
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
        /// Generates a random 2D vector with components using Perlin noise.
        /// </summary>
        /// <returns>A random 2D vector with numbers between -1 (inclusive) and 1 (exclusive).</returns>
        public Vector2 GetRandomVector2()
        {
            float x = this.GetRandom();
            float y = this.GetRandom();
            return new Vector2(x, y);
        }

        /// <summary>
        /// Generates a random 3D vector with components using Perlin noise.
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
