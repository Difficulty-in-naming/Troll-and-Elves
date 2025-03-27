// Unity
using UnityEngine;

namespace GUPS.EasyPooling.Policy.Random
{
    /// <summary>
    /// Generates random values based on a Gaussian or normal distribution.
    /// Points are distributed around a central point with decreasing probability as distance increases.
    /// </summary>
    public class GaussianRandom : IRandom
    {
        private System.Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianRandom"/> class with the specified standard deviation and seed.
        /// </summary>
        /// <param name="_Seed">The seed value for the random number generator.</param>
        public GaussianRandom(int _Seed)
        {
            this.random = new System.Random(_Seed);
        }

        /// <summary>
        /// Generates a random floating-point number based on a Gaussian distribution, with a mean of 0 and a standard deviation of 1.
        /// </summary>
        /// <returns>Generates a random floating-point number between -1 (inclusive) and 1 (exclusive).</returns>
        public float GetRandom()
        {
            double u1 = 1.0 - this.random.NextDouble(); // (0, 1]
            double u2 = 1.0 - this.random.NextDouble(); // (0, 1]
            double var_Result = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Sin(2.0 * System.Math.PI * u2);

            var_Result = var_Result < -1.0 ? -1.0 : var_Result;
            var_Result = var_Result >= 1.0 ? 1.0 - 1.0e-32 : var_Result;

            return (float)(var_Result);
        }

        /// <summary>
        /// Generates a random floating-point number based on a Gaussian distribution, between the specified values.
        /// </summary>
        /// <param name="_MinInclusive">The minimum value (inclusive).</param>
        /// <param name="_MaxExclusive">The maximum value (exclusive).</param>
        /// <returns>Generates a random floating-point number between the passed min (inclusive) and max (exclusive) value.</returns>
        public float GetRandom(float _MinInclusive, float _MaxExclusive)
        {
            float var_Deviation = _MaxExclusive - _MinInclusive;

            return (this.GetRandom() + 1f ) * 0.5f * var_Deviation + _MinInclusive;
        }

        /// <summary>
        /// Generates a random 2D vector with numbers based on a Gaussian distribution.
        /// </summary>
        /// <returns>A random 2D vector with numbers between -1 (inclusive) and 1 (exclusive).</returns>
        public Vector2 GetRandomVector2()
        {
            float x = this.GetRandom();
            float y = this.GetRandom();
            return new Vector2(x, y);
        }

        /// <summary>
        /// Generates a random 3D vector with numbers based on a Gaussian distribution.
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
