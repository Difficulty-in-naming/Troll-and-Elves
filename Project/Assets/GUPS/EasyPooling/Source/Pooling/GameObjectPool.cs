// System
using System;
using System.Collections.Generic;

// Unity
using UnityEngine;

// GUPS
using GUPS.EasyPooling.Blueprint;
using GUPS.EasyPooling.Decorator;
using GUPS.EasyPooling.Strategy;
using GUPS.EasyPooling.Policy;

namespace GUPS.EasyPooling.Pooling
{
    /// <summary>
    /// A pool for a single type (blueprint) of GameObjects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="GameObjectPool"/> class provides a pool for managing and reusing instances of a single type (blueprint) of GameObject.
    /// It supports various spawning and despawning options, including applying decorators, spawn policies, and managing the pool size dynamically.
    /// </para>
    /// <para>
    /// The pool is thread-safe, ensuring proper synchronization during spawn and despawn operations.
    /// </para>
    /// </remarks>
    public class GameObjectPool : MonoBehaviour, IPool
    {
        /// <summary>
        /// Gets the name of the GameObject.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// The original GameObject used to spawn new pooled instances.
        /// </summary>
        public GameObject Original { get; private set; }

        /// <summary>
        /// The blueprint of the GameObject is used to reset despawned instances.
        /// </summary>
        public GameObjectBlueprint Blueprint { get; private set; }

        /// <summary>
        /// Gets the pooling strategy of the pool.
        /// </summary>
        public EPoolingStrategy Strategy { get; private set; }

        /// <summary>
        /// The minimum capacity of the pool.
        /// </summary>
        private int minCapacity;

        /// <summary>
        /// Gets the capacity of the pool.
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// Gets the number of GameObjects in the pool.
        /// </summary>
        public int Count { get => this.pool.Count; }

        /// <summary>
        /// The pool of GameObjects.
        /// </summary>
        private Queue<GameObject> pool = new Queue<GameObject>();

        /// <summary>
        /// The lock used to synchronize access to the pool.
        /// </summary>
        private object poolLock = new object();

        /// <summary>
        /// Either get a pooled instance or create a new one and apply the blueprint.
        /// </summary>
        public GameObject Spawn()
        {
            return this.Spawn(this.Blueprint, Vector3.zero, Quaternion.identity, null);
        }

        /// <summary>
        /// Either get a pooled instance or create a new one and apply the blueprint.
        /// </summary>
        /// <param name="_SpawnPolicy">The spawn policy to apply to the spawned GameObject. Applied before OnSpawn.</param>
        public GameObject Spawn(SpawnPolicy _SpawnPolicy)
        {
            return this.Spawn(this.Blueprint, Vector3.zero, Quaternion.identity, _SpawnPolicy);
        }

        /// <summary>
        /// Spawns a GameObject at the specified position and rotation.
        /// </summary>
        /// <param name="_Position">The position at which to spawn the GameObject.</param>
        /// <param name="_Rotation">The rotation at which to spawn the GameObject.</param>
        public GameObject Spawn(Vector3 _Position, Quaternion _Rotation)
        {
            return this.Spawn(this.Blueprint, _Position, _Rotation, null);
        }

        /// <summary>
        /// Spawns a GameObject at the specified position and rotation.
        /// </summary>
        /// <param name="_Position">The position at which to spawn the GameObject.</param>
        /// <param name="_Rotation">The rotation at which to spawn the GameObject.</param>
        /// <param name="_SpawnPolicy">The spawn policy to apply to the spawned GameObject. Applied before OnSpawn.</param>
        public GameObject Spawn(Vector3 _Position, Quaternion _Rotation, SpawnPolicy _SpawnPolicy)
        {
            return this.Spawn(this.Blueprint, _Position, _Rotation, _SpawnPolicy);
        }

        /// <summary>
        /// Spawns a GameObject with a provided decorator. If you pass null, the GameObject will be spawned as it is.
        /// </summary>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        public GameObject Spawn(IDecorator _Decorator)
        {
            return this.Spawn(_Decorator, Vector3.zero, Quaternion.identity, null);
        }

        /// <summary>
        /// Spawns a GameObject with a provided decorator. If you pass null, the GameObject will be spawned as it is.
        /// </summary>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <param name="_SpawnPolicy">The spawn policy to apply to the spawned GameObject. Applied before OnSpawn.</param>
        public GameObject Spawn(IDecorator _Decorator, SpawnPolicy _SpawnPolicy)
        {
            return this.Spawn(_Decorator, Vector3.zero, Quaternion.identity, _SpawnPolicy);
        }

        /// <summary>
        /// Spawns a GameObject with a provided decorator at the specified position and rotation. If you pass null to the decorator, the GameObject will be spawned as it is.
        /// </summary>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <param name="_Position">The position at which to spawn the GameObject.</param>
        /// <param name="_Rotation">The rotation at which to spawn the GameObject.</param>
        public GameObject Spawn(IDecorator _Decorator, Vector3 _Position, Quaternion _Rotation)
        {
            return this.Spawn(_Decorator, _Position, _Rotation, null);
        }

        /// <summary>
        /// Spawns a GameObject with a provided decorator at the specified position and rotation. If you pass null to the decorator, the GameObject will be spawned as it is.
        /// </summary>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <param name="_Position">The position at which to spawn the GameObject.</param>
        /// <param name="_Rotation">The rotation at which to spawn the GameObject.</param>
        /// <param name="_SpawnPolicy">The spawn policy to apply to the spawned GameObject. Applied before OnSpawn.</param>
        public GameObject Spawn(IDecorator _Decorator, Vector3 _Position, Quaternion _Rotation, SpawnPolicy _SpawnPolicy)
        {
            // Make sure the pool is thread safe.
            lock (this.poolLock)
            {
                // The instance to return.
                GameObject var_GameObject = null;

                // The poolable components of the instance.
                IPoolAble[] var_Poolables = null;

                if (this.pool.Count > 0)
                {
                    // Get a pooled instance.
                    var_GameObject = this.GetPooled(_Decorator, _Position, _Rotation, out var_Poolables);
                }
                else
                {
                    // Create a new instance.
                    var_GameObject = this.GetCreated(_Decorator, _Position, _Rotation, out var_Poolables);

                    // If the grow flag is set, check if the pool needs to grow.
                    if ((this.Strategy & EPoolingStrategy.GROW) == EPoolingStrategy.GROW)
                    {
                        // Check if the pool is empty and less than the maximum capacity.
                        if (this.pool.Count == 0 && this.Capacity < this.minCapacity * 5)
                        {
                            // Increase the capacity.
                            this.Capacity += 5;
                        }
                    }
                }

                // Assign the owner for each poolable component and set is out of the pool.
                foreach (IPoolAble var_Poolable in var_Poolables)
                {
                    var_Poolable.IsPooled = false;
                }

                // Apply the spawn policy.
                if(_SpawnPolicy != null)
                {
                    _SpawnPolicy.Apply(var_GameObject);
                }

                // Activate the instance.
                var_GameObject.SetActive(true);

                // Call the OnSpawn method for each poolable component.
                foreach (IPoolAble var_Poolable in var_Poolables)
                {
                    var_Poolable.OnSpawn();
                }

                // Return the instance.
                return var_GameObject;
            }
        }

        /// <summary>
        /// Dequeues a pooled instance and applies the decoration, position and rotation. Does not check if the pool is empty.
        /// </summary>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <param name="_Position">The position at which to spawn the GameObject.</param>
        /// <param name="_Rotation">The rotation at which to spawn the GameObject.</param>
        /// <param name="_PoolAbles">Returns the poolable components of the instance.</param>
        /// <returns></returns>
        private GameObject GetPooled(IDecorator _Decorator, Vector3 _Position, Quaternion _Rotation, out IPoolAble[] _PoolAbles)
        {
            // Get a pooled instance.
            GameObject var_GameObject = this.pool.Dequeue();

            // Reset the parent.
            var_GameObject.transform.parent = null;

            // Reset the position.
            var_GameObject.transform.position = _Position;
            var_GameObject.transform.rotation = _Rotation;

            // Reset the instance.
            if (_Decorator != null && !_Decorator.OnCreateOnly)
            {
                _Decorator.OnDecorate(var_GameObject);
            }

            // Get the poolable components.
            _PoolAbles = var_GameObject.GetComponents<IPoolAble>();

            // Return the pooled instance.
            return var_GameObject;
        }

        /// <summary>
        /// Instantiates a new clone and applies the decoration, position and rotation.
        /// </summary>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <param name="_Position">The position at which to spawn the GameObject.</param>
        /// <param name="_Rotation">The rotation at which to spawn the GameObject.</param>
        /// <param name="_PoolAbles">Returns the poolable components of the instance.</param>
        /// <returns></returns>
        private GameObject GetCreated(IDecorator _Decorator, Vector3 _Position, Quaternion _Rotation, out IPoolAble[] _PoolAbles)
        {
            // Create a new instance.
            GameObject var_GameObject = Instantiate(this.Original, _Position, _Rotation, null);

            // Set the name.
            var_GameObject.name = this.Name;

            // Apply blueprint properties.
            if (_Decorator != null)
            {
                _Decorator.OnDecorate(var_GameObject);
            }

            // Get the poolable components.
            _PoolAbles = var_GameObject.GetComponents<IPoolAble>();

            if (_PoolAbles.Length == 0)
            {
                // Add the default poolable component.
                _PoolAbles = new IPoolAble[] { var_GameObject.AddComponent<DefaultPoolAble>() };
            }

            // Call the OnCreate method for each poolable component.
            foreach (IPoolAble var_Poolable in _PoolAbles)
            {
                var_Poolable.Owner = this;

                var_Poolable.OnCreate();
            }

            // Return the created instance.
            return var_GameObject;
        }

        /// <summary>
        /// Despawns a poolable GameObject back into the pool.
        /// </summary>
        /// <param name="_PoolAble">The poolable GameObject to despawn.</param>
        public void Despawn(GameObject _PoolAble)
        {
            // Despawn the instance without an un-decorator.
            this.Despawn(_PoolAble, null);
        }

        /// <summary>
        /// Despawns a poolable GameObject back into the pool using a provided un-decorator.
        /// </summary>
        /// <param name="_PoolAble">The poolable GameObject to despawn.</param>
        /// <param name="_UnDecorator">The un-decorator to apply to the despawned GameObject.</param>
        public void Despawn(GameObject _PoolAble, IUnDecorator _UnDecorator)
        {
            // Make sure the pool is thread safe.
            lock (this.poolLock)
            {
                // Get the poolable components.
                IPoolAble[] var_Poolables = _PoolAble.GetComponents<IPoolAble>();

                // Call the OnDespawn method for each poolable component and set is in the pool.
                foreach (IPoolAble var_Poolable in var_Poolables)
                {
                    var_Poolable.OnDespawn();

                    var_Poolable.IsPooled = true;
                }

                // If the pool is not full, add the instance back to the pool. Else, destroy it.
                if (this.pool.Count < this.Capacity)
                {
                    // If the un-decorator is not null, call the OnUnDecorate method.
                    if (_UnDecorator != null)
                    {
                        _UnDecorator.OnUnDecorate(_PoolAble);
                    }

                    // Deactivate the instance.
                    _PoolAble.SetActive(false);

                    // Reset the parent.
                    _PoolAble.transform.parent = this.transform;

                    // Add the instance back to the pool.
                    this.pool.Enqueue(_PoolAble);
                }
                else
                {
                    // Destroy the instance.
                    Destroy(_PoolAble);

                    // If the grow flag is set, check if the pool needs to shrink.
                    if ((this.Strategy & EPoolingStrategy.GROW) == EPoolingStrategy.GROW)
                    {
                        // Check if the pool is at maximum capacity and more than the minimum capacity.
                        if (this.pool.Count == this.Capacity && this.pool.Count > this.minCapacity)
                        {
                            // Decrease the capacity.
                            this.Capacity--;

                            // Remove the instance from the pool.
                            GameObject var_GameObject = this.pool.Dequeue();

                            // Destroy the instance.
                            Destroy(var_GameObject);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a new reference pool for a type of GameObject.
        /// </summary>
        /// <param name="_Original">The original GameObject used to spawn new pooled instances.</param>
        /// <param name="_Strategy">The pooling strategy to use (optional, default is <see cref="EPoolingStrategy.DEFAULT"/>).</param>
        /// <param name="_Capacity">The initial capacity of the pool (optional, default is 25).</param>
        public static GameObjectPool Create(GameObject _Original, EPoolingStrategy _Strategy = EPoolingStrategy.DEFAULT, int _Capacity = 25)
        {
            // Create a new GameObject.
            GameObject var_PoolGameObject = new GameObject(_Original.name + " - Pool");

            // Add the pool component.
            GameObjectPool var_Pool = var_PoolGameObject.AddComponent<GameObjectPool>();

            // Check if the original is already a poolable instance.
            IPoolAble[] var_Poolables = _Original.GetComponents<IPoolAble>();

            if (var_Poolables.Length == 0)
            {
                // Add the default poolable component.
                var_Poolables = new IPoolAble[] { _Original.AddComponent<DefaultPoolAble>() };
            }

            // Assign the owner for each poolable component and set is out of the pool.
            foreach (IPoolAble var_Poolable in var_Poolables)
            {
                var_Poolable.Owner = var_Pool;

                var_Poolable.IsPooled = false;
            }

            // Create the original.
            var_Pool.Original = Instantiate(_Original, var_Pool.transform);
            var_Pool.Original.name += " - Blueprint";
            var_Pool.Original.SetActive(false);

            // Assign the rest of the parameters.
            var_Pool.Name = _Original.name;
            var_Pool.Strategy = _Strategy;
            var_Pool.minCapacity = _Capacity;
            var_Pool.Capacity = _Capacity;

            // Create the blueprint.
            var_Pool.Blueprint = GameObjectBlueprint.Create(var_Pool.Original);

            // If the blueprint is null, destroy the pool and return null.
            if(var_Pool.Blueprint == null)
            {
                Destroy(var_PoolGameObject);

                return null;
            }

            // Check if the strategy has the fill flag.
            if ((var_Pool.Strategy & EPoolingStrategy.FILL) == EPoolingStrategy.FILL)
            {
                // Fill the pool.
                for (int i = 0; i < var_Pool.Capacity; i++)
                {
                    // Create a new instance.
                    GameObject var_NewInstance = var_Pool.GetCreated(var_Pool.Blueprint, Vector3.zero, Quaternion.identity, out _);

                    // Attach to the pool.
                    var_NewInstance.transform.parent = var_Pool.transform;

                    // Deactivate the instance.
                    var_NewInstance.SetActive(false);

                    // Add the instance to the pool.
                    var_Pool.pool.Enqueue(var_NewInstance);
                }
            }

            // Return the pool.
            return var_Pool;
        }

        /// <summary>
        /// Create a new pool for an empty GameObject.
        /// </summary>
        /// <param name="_Name">The name to use.</param>
        /// <param name="_Strategy">The pooling strategy to use (optional, default is <see cref="EPoolingStrategy.DEFAULT"/>).</param>
        /// <param name="_Capacity">The initial capacity of the pool (optional, default is 25).</param>
        public static GameObjectPool Create(String _Name, EPoolingStrategy _Strategy = EPoolingStrategy.DEFAULT, int _Capacity = 25)
        {
            // Create a new GameObject.
            GameObject var_PoolGameObject = new GameObject(_Name + " - Pool");

            // Add the pool component.
            GameObjectPool var_Pool = var_PoolGameObject.AddComponent<GameObjectPool>();

            // Create an empty GameObject.
            GameObject var_Original = new GameObject(_Name + " - Blueprint");

            // Check if the original is already a poolable instance.
            IPoolAble[] var_Poolables = var_Original.GetComponents<IPoolAble>();

            if (var_Poolables.Length == 0)
            {
                // Add the default poolable component.
                var_Poolables = new IPoolAble[] { var_Original.AddComponent<DefaultPoolAble>() };
            }

            // Assign the owner for each poolable component and set is out of the pool.
            foreach (IPoolAble var_Poolable in var_Poolables)
            {
                var_Poolable.Owner = var_Pool;

                var_Poolable.IsPooled = false;
            }

            // Create the original.
            var_Pool.Original = var_Original;
            var_Pool.Original.transform.parent = var_Pool.transform;
            var_Pool.Original.SetActive(false);

            // Assign the rest of the parameters.
            var_Pool.Name = _Name;
            var_Pool.Strategy = _Strategy;
            var_Pool.minCapacity = _Capacity;
            var_Pool.Capacity = _Capacity;

            // Create the blueprint.
            var_Pool.Blueprint = GameObjectBlueprint.Create(var_Pool.Original);

            // If the blueprint is null, destroy the pool and return null.
            if (var_Pool.Blueprint == null)
            {
                Destroy(var_PoolGameObject);

                return null;
            }

            // Check if the strategy has the fill flag.
            if ((var_Pool.Strategy & EPoolingStrategy.FILL) == EPoolingStrategy.FILL)
            {
                // Fill the pool.
                for (int i = 0; i < var_Pool.Capacity; i++)
                {
                    // Create a new instance.
                    GameObject var_NewInstance = var_Pool.GetCreated(var_Pool.Blueprint, Vector3.zero, Quaternion.identity, out _);

                    // Attach to the pool.
                    var_NewInstance.transform.parent = var_Pool.transform;

                    // Deactivate the instance.
                    var_NewInstance.SetActive(false);

                    // Add the instance to the pool.
                    var_Pool.pool.Enqueue(var_NewInstance);
                }
            }

            // Return the pool.
            return var_Pool;
        }
    }
}
