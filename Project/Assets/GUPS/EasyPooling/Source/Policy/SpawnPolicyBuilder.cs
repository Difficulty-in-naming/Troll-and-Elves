// Unity
using UnityEngine;

// GUPS
using GUPS.EasyPooling.Policy.Random;
using GUPS.EasyPooling.Shapes;
using System;

namespace GUPS.EasyPooling.Policy
{
    /// <summary>
    /// Builder class for creating <see cref="SpawnPolicy"/> instances with specific parameters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="SpawnPolicyBuilder"/> class provides a fluent interface for constructing <see cref="SpawnPolicy"/> instances
    /// with customizable spawning parameters. It encapsulates the configuration of randomization methods, dimensionality (2D or 3D),
    /// spawning shape (circle, sphere, or square), center position, radius, boundaries, initial velocity, and velocity direction.
    /// </para>
    /// <para>
    /// The builder pattern allows the step-by-step definition of a spawning policy, providing flexibility and ease of use.
    /// </para>
    /// </remarks>
    public class SpawnPolicyBuilder
    {
        /// <summary>
        /// Gets or sets the randomization method used for spawning.
        /// </summary>
        private IRandom random;

        /// <summary>
        /// Gets or sets a value indicating whether the spawning is in 2D space.
        /// </summary>
        private bool is2D;

        /// <summary>
        /// Gets or sets the 2D plane. Default is the XY plane.
        /// </summary>
        private EPlane plane2D;

        /// <summary>
        /// Gets or sets a value indicating whether objects should be spawned in a sphere (true) or a square (false).
        /// </summary>
        private bool inSphere;

        /// <summary>
        /// Gets or sets the spawn limits for objects in a sphere in radian [0, 2pi].
        /// </summary>
        private Tuple<float, float>[] sphereLimits;

        /// <summary>
        /// Gets or sets the center position for spawning objects.
        /// </summary>
        private Vector3 center;

        /// <summary>
        /// Gets or sets the radius for spawning objects in circular distributions.
        /// </summary>
        private float radius;

        /// <summary>
        /// Gets or sets the spawning area for objects in a square.
        /// </summary>
        private Box boundaries;

        /// <summary>
        /// Gets or sets the initial velocity for spawned objects.
        /// </summary>
        private float velocity;

        /// <summary>
        /// Gets or sets the direction of the velocity.
        /// </summary>
        private Vector3 direction;

        /// <summary>
        /// Get or sets if the velocity should be away from the center.
        /// </summary>
        private bool moveAwayFromCenter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpawnPolicyBuilder"/> class with the <see cref="UniformRandom"/> randomization method.
        /// </summary>
        public SpawnPolicyBuilder()
            :this(new UniformRandom())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpawnPolicyBuilder"/> class with the specified randomization method.
        /// </summary>
        /// <param name="_Random">The randomization method to use for spawning.</param>
        public SpawnPolicyBuilder(IRandom _Random)
        {
            this.random = _Random;
            this.is2D = false;
            this.plane2D = EPlane.XY;
            this.inSphere = false;
            this.sphereLimits = new Tuple<float, float>[] { new Tuple<float, float>(0f, 360f) };
            this.center = Vector3.zero;
            this.radius = 0f;
            this.boundaries = new Box(Vector3.zero, Vector3.zero);
            this.velocity = 0f;
            this.direction = Vector3.zero;
        }

        /// <summary>
        /// Sets the spawning to 2D mode.
        /// </summary>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder Is2D()
        {
            this.is2D = true;
            return this;
        }

        /// <summary>
        /// Sets the spawning to 2D mode.
        /// </summary>
        /// <param name="_Plane">The 2D plane to spawn on.</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder Is2D(EPlane _Plane)
        {
            this.is2D = true;
            this.plane2D = _Plane;
            return this;
        }

        /// <summary>
        /// Sets the spawning to 3D mode.
        /// </summary>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder Is3D()
        {
            this.is2D = false;
            return this;
        }

        /// <summary>
        /// Sets the center position for spawning objects.
        /// </summary>
        /// <param name="_Center">The center position.</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SetCenter(Vector3 _Center)
        {
            this.center = _Center;
            return this;
        }

        /// <summary>
        /// Sets the spawning area to a 2D circle (default is the XY plane) with the specified radius. Every GameObject will be spawned in a circle around the center within the specified radius.
        /// </summary>
        /// <param name="_Radius">The radius of the circle.</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SpawnInCircle(float _Radius)
        {
            this.is2D = true;
            this.inSphere = true;
            this.radius = _Radius;
            this.sphereLimits = new Tuple<float, float>[] { new Tuple<float, float>(0f, 2f * Mathf.PI) };
            return this;
        }

        /// <summary>
        /// Sets the spawning area to a 2D circle (default is the XY plane) with the specified radius. Every GameObject will be spawned in a circle around the center within the specified radius.
        /// </summary>
        /// <param name="_Radius">The radius of the circle.</param>
        /// <param name="_Plane">The 2D plane to spawn on.</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SpawnInCircle(float _Radius, EPlane _Plane)
        {
            this.is2D = true;
            this.plane2D = _Plane;
            this.inSphere = true;
            this.radius = _Radius;
            this.sphereLimits = new Tuple<float, float>[] { new Tuple<float, float>(0f, 2f * Mathf.PI) };
            return this;
        }

        /// <summary>
        /// Sets the spawning area to a 2D circle (default is the XY plane) with the specified radius. Every GameObject will be spawned in a circle around the center within the specified radius.
        /// </summary>
        /// <param name="_Radius">The radius of the circle.</param>
        /// <param name="_Plane">The 2D plane to spawn on.</param>
        /// <param name="_Limits">The spawn limit of the circle in radian [0, 2pi].</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SpawnInCircle(float _Radius, EPlane _Plane, Tuple<float, float> _Limit)
        {
            this.is2D = true;
            this.plane2D = _Plane;
            this.inSphere = true;
            this.radius = _Radius;
            this.sphereLimits = new Tuple<float, float>[] { _Limit };
            return this;
        }

        /// <summary>
        /// Sets the spawning area to a 2D circle (default is the XY plane) with the specified radius. Every GameObject will be spawned in a circle around the center within the specified radius.
        /// </summary>
        /// <param name="_Radius">The radius of the circle.</param>
        /// <param name="_Plane">The 2D plane to spawn on.</param>
        /// <param name="_Limits">The spawn limits of the circle in radian [0, 2pi].</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SpawnInCircle(float _Radius, EPlane _Plane, Tuple<float, float>[] _Limits)
        {
            this.is2D = true;
            this.plane2D = _Plane;
            this.inSphere = true;
            this.radius = _Radius;
            this.sphereLimits = _Limits;
            return this;
        }

        /// <summary>
        /// Sets the spawning area to a 3D sphere with the specified radius. Every GameObject will be spawned in a sphere around the center within the specified radius.
        /// </summary>
        /// <param name="_Radius">The radius of the circle.</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SpawnInSphere(float _Radius)
        {
            this.is2D = false;
            this.inSphere = true;
            this.radius = _Radius;
            return this;
        }

        /// <summary>
        /// Sets the spawning area to a 2D square (the default is the XY plane) with boundaries of size one. Every GameObject will be spawned in a square around the center with the boundaries [-1, 1] in each direction.
        /// </summary>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SpawnInSquare()
        {
            return this.SpawnInSquare(Vector2.one);
        }

        /// <summary>
        /// Sets the spawning area to a 2D square (the default is the XY plane) with the passed boundaries. Every GameObject will be spawned in a square around the center within the passed boundaries.
        /// </summary>
        /// <param name="_Boundary">The size of the square.</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SpawnInSquare(Vector2 _Boundary)
        {
            return this.SpawnInSquare(new Rect(-_Boundary, _Boundary));
        }

        /// <summary>
        /// Sets the spawning area to a 2D square (the default is the XY plane) with the passed boundaries. Every GameObject will be spawned in a square around the center within the passed boundaries.
        /// </summary>
        /// <param name="_Boundary">The size of the square.</param>
        /// <param name="_Plane">The 2D plane to spawn on.</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SpawnInSquare(Vector2 _Boundary, EPlane _Plane)
        {
            return this.SpawnInSquare(new Rect(-_Boundary, _Boundary), _Plane);
        }

        /// <summary>
        /// Sets the spawning area to a 2D square (the default is the XY plane) with the passed boundaries. Every GameObject will be spawned in a square around the center within the passed boundaries.
        /// </summary>
        /// <param name="_Boundaries">The size of the square.</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SpawnInSquare(Rect _Boundaries)
        {
            this.is2D = true;
            this.inSphere = false;
            this.boundaries = new Box(_Boundaries.min, _Boundaries.max);
            return this;
        }

        /// <summary>
        /// Sets the spawning area to a 2D square (the default is the XY plane) with the passed boundaries. Every GameObject will be spawned in a square around the center within the passed boundaries.
        /// </summary>
        /// <param name="_Boundaries">The size of the square.</param>
        /// <param name="_Plane">The 2D plane to spawn on.</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SpawnInSquare(Rect _Boundaries, EPlane _Plane)
        {
            this.is2D = true;
            this.inSphere = false;
            this.boundaries = new Box(_Boundaries.min, _Boundaries.max);
            this.plane2D = _Plane;
            return this;
        }

        /// <summary>
        /// Sets the spawning shape to a square with boundaries of size one. Every GameObject will be spawned in a square around the center with the boundaries [-1, 1] in each direction.
        /// </summary>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SpawnInBox()
        {
            return this.SpawnInBox(Vector3.one);
        }

        /// <summary>
        /// Sets the spawning shape to a square with the passed boundaries. Every GameObject will be spawned in a square around the center within the passed boundaries.
        /// </summary>
        /// <param name="_MinMaxSpawnPosition">The size of the square.</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SpawnInBox(Vector3 _Boundary)
        {
            return this.SpawnInBox(new Box(-_Boundary, _Boundary));
        }

        /// <summary>
        /// Sets the spawning shape to a square with the specified minimum and maximum positions. Every GameObject will be spawned in a square around the center within the passed boundaries.
        /// </summary>
        /// <param name="_MinSpawnPosition">The minimum position of the square.</param>
        /// <param name="_MaxSpawnPosition">The maximum position of the square.</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SpawnInBox(Box _Boundaries)
        {
            this.is2D = false;
            this.inSphere = false;
            this.boundaries = _Boundaries;
            return this;
        }

        /// <summary>
        /// Set the rigidbody velocity of the spawned objects to the specified direction and velocity.
        /// </summary>
        /// <param name="_Direction">The direction of the velocity.</param>
        /// <param name="_Velocity">The velocity of the spawned objects.</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SetVelocity(Vector3 _Direction, float _Velocity)
        {
            this.direction = _Direction;
            this.velocity = _Velocity;
            return this;
        }

        /// <summary>
        /// Set the rigidbody velocity of the spawned objects to move aways from the center with the passed velocity.
        /// </summary>
        /// <param name="_Velocity">The velocity of the spawned objects.</param>
        /// <returns>The updated <see cref="SpawnPolicyBuilder"/> instance.</returns>
        public SpawnPolicyBuilder SetVelocity(float _Velocity)
        {
            this.velocity = _Velocity;
            this.moveAwayFromCenter = true;
            return this;
        }

        /// <summary>
        /// Builds a <see cref="SpawnPolicy"/> instance with the configured parameters.
        /// </summary>
        /// <returns>The constructed <see cref="SpawnPolicy"/> instance.</returns>
        public SpawnPolicy Build()
        {
            SpawnPolicy policy = new SpawnPolicy();
            policy.Random = this.random;
            policy.Is2D = this.is2D;
            policy.Plane2D = this.plane2D;
            policy.InSphere = this.inSphere;
            policy.SphereLimits = this.sphereLimits;
            policy.Center = this.center;
            policy.Radius = this.radius;
            policy.Boundaries = this.boundaries;
            policy.Velocity = this.velocity;
            policy.Direction = this.direction;
            policy.MoveAwayFromCenter = this.moveAwayFromCenter;
            return policy;
        }

        /// <summary>
        /// Creates a 2D (on the XY plane) explosion spawn policy with the specified parameters.
        /// </summary>
        /// <param name="_Center">The center position of the explosion.</param>
        /// <param name="_Radius">The radius of the explosion.</param>
        /// <param name="_Velocity">The initial velocity of spawned objects.</param>
        /// <returns>The generated <see cref="SpawnPolicy"/>.</returns>
        public static SpawnPolicy Explosion2D(Vector2 _Center, float _Radius, float _Velocity)
        {
            return new SpawnPolicyBuilder(new UniformRandom())
                .SetCenter(_Center)
                .SpawnInCircle(_Radius)
                .SetVelocity(_Velocity)
                .Build();
        }

        /// <summary>
        /// Creates a 3D explosion spawn policy with the specified parameters.
        /// </summary>
        /// <param name="_Center">The center position of the explosion.</param>
        /// <param name="_Radius">The radius of the explosion.</param>
        /// <param name="_Velocity">The initial velocity of spawned objects.</param>
        /// <returns>The generated <see cref="SpawnPolicy"/>.</returns>
        public static SpawnPolicy Explosion3D(Vector3 _Center, float _Radius, float _Velocity)
        {
            return new SpawnPolicyBuilder(new UniformRandom())
                .SetCenter(_Center)
                .SpawnInSphere(_Radius)
                .SetVelocity(_Velocity)
                .Build();
        }
    }
}
