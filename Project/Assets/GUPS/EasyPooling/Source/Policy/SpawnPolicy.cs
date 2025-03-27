// Unity
using UnityEngine;

// GUPS
using GUPS.EasyPooling.Policy.Random;
using GUPS.EasyPooling.Shapes;
using System;

namespace GUPS.EasyPooling.Policy
{
    /// <summary>
    /// Defines the parameters for object spawning, including the randomization method, dimensionality, shape, 
    /// center, radius, spawn position limits, and initial velocity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="SpawnPolicy"/> class encapsulates the specifications for spawning objects within a game environment.
    /// It provides detailed configurations such as the randomization method, dimensionality (2D or 3D), spawning shape 
    /// (sphere or square), spawn limits, center position, radius, boundaries, and initial velocity.
    /// </para>
    /// <para>
    /// Instances of this class serve as a scheme for applying spawning policies to GameObjects, facilitating controlled
    /// and dynamic object generation within a game scene.
    /// </para>
    /// </remarks>
    public class SpawnPolicy
    {
        /// <summary>
        /// Gets or sets the randomization method used for spawning.
        /// </summary>
        public IRandom Random { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the spawning is in 2D space.
        /// </summary>
        public bool Is2D { get; set; }

        /// <summary>
        /// Gets or sets the 2D plane. Default is the XY plane.
        /// </summary>
        public EPlane Plane2D { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether objects should be spawned in a sphere (true) or a square (false).
        /// </summary>
        public bool InSphere { get; set; }

        /// <summary>
        /// Gets or sets the spawn limits for objects in a sphere in radian [0, 2pi].
        /// </summary>
        public Tuple<float, float>[] SphereLimits { get; set; }

        /// <summary>
        /// Gets or sets the center position for spawning objects.
        /// </summary>
        public Vector3 Center { get; set; }

        /// <summary>
        /// Gets or sets the radius for spawning objects in circular distributions.
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Gets or sets the spawning area for objects in a square.
        /// </summary>
        public Box Boundaries { get; set; }

        /// <summary>
        /// Gets or sets the initial velocity for spawned objects.
        /// </summary>
        public float Velocity { get; set; }

        /// <summary>
        /// Gets or sets the direction of the velocity.
        /// </summary>
        public Vector3 Direction { get; set; }

        /// <summary>
        /// Get or sets if the velocity should be away from the center.
        /// </summary>
        public bool MoveAwayFromCenter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpawnPolicy"/> class with default values.
        /// </summary>
        public SpawnPolicy()
        {
            this.Random = new UniformRandom();
            this.Is2D = false;
            this.Plane2D = EPlane.XY;
            this.InSphere = false;
            this.SphereLimits = new Tuple<float, float>[] { new Tuple<float, float>(0f, 2f * Mathf.PI) };
            this.Center = Vector3.zero;
            this.Radius = 0f;
            this.Boundaries = new Box(Vector3.zero, Vector3.zero);
            this.Velocity = 0f;
            this.Direction = Vector3.zero;
        }

        /// <summary>
        /// Apply the policy on the passed GameObject.
        /// </summary>
        /// <param name="_Object">The GameObject to apply the policy on.</param>
        public void Apply(GameObject _Object)
        {
            // Get position and velocity.
            Vector3 var_Position = this.GetSpawnPosition();
            Vector3 var_Velocity = this.GetSpawnVelocity(var_Position);

            // Apply position on transform.
            _Object.transform.position = var_Position;

            // Apply velocity on rigidbody.
            if (this.Is2D)
            {
                Rigidbody2D rigidbody2D = _Object.GetComponent<Rigidbody2D>();
                Rigidbody rigidbody3D = _Object.GetComponent<Rigidbody>();

                if (rigidbody2D != null)
                {
                    switch (this.Plane2D)
                    {
#if UNITY_6000_0_OR_NEWER
                        case EPlane.XY:
                            rigidbody2D.linearVelocity = new Vector2(var_Velocity.x, var_Velocity.y);
                            break;
                        case EPlane.XZ:
                            rigidbody2D.linearVelocity = new Vector2(var_Velocity.x, var_Velocity.z);
                            break;
                        case EPlane.YZ:
                            rigidbody2D.linearVelocity = new Vector2(var_Velocity.y, var_Velocity.z);
                            break;
#else
                        case EPlane.XY:
                            rigidbody2D.velocity = new Vector2(var_Velocity.x, var_Velocity.y);
                            break;
                        case EPlane.XZ:
                            rigidbody2D.velocity = new Vector2(var_Velocity.x, var_Velocity.z);
                            break;
                        case EPlane.YZ:
                            rigidbody2D.velocity = new Vector2(var_Velocity.y, var_Velocity.z);
                            break;
#endif
                    }
                } 
                else if(rigidbody3D != null)
                {
                    switch (this.Plane2D)
                    {
#if UNITY_6000_0_OR_NEWER
                        case EPlane.XY:
                            rigidbody3D.linearVelocity = new Vector3(var_Velocity.x, var_Velocity.y, 0f);
                            break;
                        case EPlane.XZ:
                            rigidbody3D.linearVelocity = new Vector3(var_Velocity.x, 0f, var_Velocity.z);
                            break;
                        case EPlane.YZ:
                            rigidbody3D.linearVelocity = new Vector3(0f, var_Velocity.y, var_Velocity.z);
                            break;
#else
                        case EPlane.XY:
                            rigidbody3D.velocity = new Vector3(var_Velocity.x, var_Velocity.y, 0f);
                            break;
                        case EPlane.XZ:
                            rigidbody3D.velocity = new Vector3(var_Velocity.x, 0f, var_Velocity.z);
                            break;
                        case EPlane.YZ:
                            rigidbody3D.velocity = new Vector3(0f, var_Velocity.y, var_Velocity.z);
                            break;
#endif
                    }
                }
            }
            else
            {
                Rigidbody rigidbody3D = _Object.GetComponent<Rigidbody>();

                if (rigidbody3D != null)
                {
#if UNITY_6000_0_OR_NEWER
                    rigidbody3D.linearVelocity = var_Velocity;
#else
                    rigidbody3D.velocity = var_Velocity;
#endif
                }
            }
        }

        /// <summary>
        /// Gets the spawn position based on the defined policy.
        /// </summary>
        /// <returns>The spawn position.</returns>
        private Vector3 GetSpawnPosition()
        {
            Vector3 var_Position = Vector3.zero;

            if (this.InSphere)
            {
                if (this.Is2D)
                {
                    // Get random sphere limit index.
                    int var_Index = UnityEngine.Random.Range(0, this.SphereLimits.Length);

                    // Get random angle.
                    float var_Theta = UnityEngine.Random.Range(this.SphereLimits[var_Index].Item1, this.SphereLimits[var_Index].Item2);

                    // Get random distance.
                    float var_Distance = this.Random.GetRandom(0f, this.Radius);

                    // Get random vector in angle direction.
                    switch (this.Plane2D)
                    {
                        case EPlane.XY:
                            var_Position = new Vector3(Mathf.Cos(var_Theta), Mathf.Sin(var_Theta), 0f) * var_Distance;
                            break;
                        case EPlane.XZ:
                            var_Position = new Vector3(Mathf.Cos(var_Theta), 0f, Mathf.Sin(var_Theta)) * var_Distance;
                            break;
                        case EPlane.YZ:
                            var_Position = new Vector3(0f, Mathf.Cos(var_Theta), Mathf.Sin(var_Theta)) * var_Distance;
                            break;
                    }

                }
                else
                {
                    // Get a random point on the sphere.
                    float var_X = this.Random.GetRandom();
                    float var_Y = this.Random.GetRandom();
                    float var_Z = this.Random.GetRandom();

                    // Get the normalized position on the sphere.
                    var_Position = new Vector3(var_X, var_Y, var_Z).normalized * this.Radius;
                }
            }
            else
            {
                if (this.Is2D)
                {
                    // Get random position in boundaries.
                    float var_X = this.Random.GetRandom(this.Boundaries.X, this.Boundaries.X + this.Boundaries.Width);
                    float var_Y = this.Random.GetRandom(this.Boundaries.Y, this.Boundaries.Y + this.Boundaries.Height);

                    // Get random vector on plane.
                    switch (this.Plane2D)
                    {
                        case EPlane.XY:
                            var_Position = new Vector3(var_X, var_Y, 0f);
                            break;
                        case EPlane.XZ:
                            var_Position = new Vector3(var_X, 0f, var_Y);
                            break;
                        case EPlane.YZ:
                            var_Position = new Vector3(0f, var_X, var_Y);
                            break;
                    }
                }
                else
                {
                    // Get random position in boundaries.
                    float var_X = this.Random.GetRandom(this.Boundaries.X, this.Boundaries.X + this.Boundaries.Width);
                    float var_Y = this.Random.GetRandom(this.Boundaries.Y, this.Boundaries.Y + this.Boundaries.Height);
                    float var_Z = this.Random.GetRandom(this.Boundaries.Z, this.Boundaries.Z + this.Boundaries.Depth);

                    var_Position = new Vector3(var_X, var_Y, var_Z);
                }
            }

            // Add the center position.
            var_Position += this.Center;

            // Return the spawn position.
            return var_Position;
        }

        /// <summary>
        /// Gets the spawn velocity based on the defined policy.
        /// </summary>
        /// <param name="_Position">The spawn position.</param>
        /// <returns>The spawn velocity.</returns>
        private Vector3 GetSpawnVelocity(Vector3 _Position)
        {
            // Get the direction.
            Vector3 var_Direction = this.Direction;

            if (this.MoveAwayFromCenter)
            {
                var_Direction = (_Position - this.Center).normalized;
            }

            // Get the direction on the plane.
            if(this.Is2D)
            {
                switch (this.Plane2D)
                {
                    case EPlane.XY:
                        var_Direction = new Vector3(var_Direction.x, var_Direction.y, 0f);
                        break;
                    case EPlane.XZ:
                        var_Direction = new Vector3(var_Direction.x, 0f, var_Direction.z);
                        break;
                    case EPlane.YZ:
                        var_Direction = new Vector3(0f, var_Direction.y, var_Direction.z);
                        break;
                }
            }

            // Return the spawn velocity.
            return var_Direction * this.Velocity;
        }
    }
}
