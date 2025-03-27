// System
using System;
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;

// GUPS
using GUPS.EasyPooling.Blueprint;
using GUPS.EasyPooling.Decorator;
using GUPS.EasyPooling.Policy;
using GUPS.EasyPooling.Pooling;
using GUPS.EasyPooling.Singleton;
using GUPS.EasyPooling.Strategy;

namespace GUPS.EasyPooling
{
    /// <summary>
    /// Abstract base class for implementing a generic game object pooling system.
    /// </summary>
    /// <typeparam name="T">The type of the concrete pool class inheriting from this base class.</typeparam>
    /// <remarks>
    /// <para>
    /// The <see cref="AGamePool{T}"/> class serves as a foundation for creating specialized game object pooling systems in Unity.
    /// It extends the functionality of the <see cref="Singleton{T}"/> class and implements the <see cref="IPool"/> interface,
    /// providing a robust and thread-safe mechanism for managing and reusing game objects efficiently.
    /// </para>
    /// <para>
    /// The pooling system maintains an empty pool for spawning and despawning empty game objects, and blueprint 'sub'-pools for
    /// managing different types of game objects. It supports various pooling strategies and allows for dynamic registration of
    /// blueprints during initialization. Blueprints can be registered with specified pooling strategies and initial capacities.
    /// </para>
    /// <para>
    /// The class provides a comprehensive set of methods for spawning and despawning game objects based on blueprint names,
    /// applying decorators and spawn policies as needed.
    /// </para>
    /// </remarks>
    public abstract class AGamePool<T> : Singleton<T>, IPool
        where T : AGamePool<T>
    {
        /// <summary>
        /// Reference to the empty pool used for spawning empty GameObjects.
        /// </summary>
        private GameObjectPool emptyPool = null;

        /// <summary>
        /// Lock object for thread safety.
        /// </summary>
        private object blueprintLock = new object();

        /// <summary>
        /// A dictionary mapping a blueprint identifier to their respective GameObjectPool.
        /// </summary>
        private Dictionary<String, GameObjectPool> blueprintPools = new Dictionary<string, GameObjectPool>();

        /// <summary>
        /// Array of GameObject blueprints to register on pool awake, useful if you want to assing protoypes in the inspector.
        /// </summary>
        public BlueprintPoolDefinition[] StartupBlueprints;

        /// <summary>
        /// Gets the pooling strategy of the empty pool.
        /// </summary>
        public EPoolingStrategy Strategy { get => this.emptyPool.Strategy; }

        /// <summary>
        /// Gets the capacity of the empty pool.
        /// </summary>
        public int Capacity { get => this.emptyPool.Capacity; }

        /// <summary>
        /// Gets the number of GameObjects in the empty pool.
        /// </summary>
        public int Count { get => this.emptyPool.Count; }

        /// <summary>
        /// Called during Awake to initialize the LocalPool instance.
        /// </summary>
        protected override void Awake()
        {
            // Call the base method.
            base.Awake();

            // Create the empty pool.
            this.emptyPool = GameObjectPool.Create("Empty", EPoolingStrategy.DEFAULT, 25);

            // Set the parent of the empty pool to this GameObject.
            this.emptyPool.transform.parent = this.transform;

            // Register the startup blueprints.
            if (this.StartupBlueprints != null)
            {
                foreach (var var_Blueprint in this.StartupBlueprints)
                {
                    this.Register(var_Blueprint);
                }
            }
        }

        #region Pool

        private int GetBlueprintIdentifier(GameObject _Blueprint)
        {
            // Get the identifier of the blueprint.
            int var_Identifier = 42;

            // Calculate the identifier of the blueprint by the blueprint and children component type names.
            Component[] var_Components = _Blueprint.GetComponentsInChildren<Component>(true);

            foreach (Component var_Component in var_Components)
            {
                var_Identifier = var_Identifier ^ var_Component.GetType().Name.GetHashCode();
            }

            return var_Identifier;
        }

        /// <summary>
        /// Creates a new blueprint pool and registers it with the specified name, blueprint, pooling strategy, and initial capacity.
        /// </summary>
        /// <param name="_Name">The name or identifier of the blueprint.</param>
        /// <param name="_Blueprint">The GameObject blueprint to register.</param>
        /// <param name="_PoolingStrategy">The pooling strategy to use (optional, default is <see cref="EPoolingStrategy.DEFAULT"/>).</param>
        /// <param name="_Capacity">The initial capacity of the pool (optional, default is 25).</param>
        private void CreateBlueprint(String _Name, GameObject _Blueprint, EPoolingStrategy _PoolingStrategy, int _Capacity)
        {
            // Assign an empty object to the blueprint pool list as placeholder. Making sure registering the prototyep from Awake does not create any issues.
            this.blueprintPools.Add(_Name, null);

            // Create a new pool.
            GameObjectPool var_Pool = GameObjectPool.Create(_Blueprint, _PoolingStrategy, _Capacity);

            // If the pool is null, show a warning and return.
            if (var_Pool == null)
            {
                Debug.LogWarning("Failed to create blueprint pool for '" + _Name + "'.");
                return;
            }

            // Set the parent of the pool to this GameObject.
            var_Pool.transform.parent = this.transform;

            // Replace the placeholder with the new created pool.
            this.blueprintPools[_Name] = var_Pool;
        }

        /// <summary>
        /// Checks if a blueprint pool with the specified name already exists.
        /// </summary>
        /// <param name="_Name">The name or identifier of the blueprint.</param>
        /// <returns>True if a pool with the specified name exists; otherwise, false.</returns>
        public bool HasBlueprint(String _Name)
        {
            lock (this.blueprintLock)
            {
                return this.blueprintPools.ContainsKey(_Name);
            }
        }

        /// <summary>
        /// Gets the blueprint pool with the specified name.
        /// </summary>
        /// <param name="_Name">The name or identifier of the blueprint.</param>
        /// <returns>The GameObjectPool with the specified name, or null if not found.</returns>
        private GameObjectPool GetBlueprint(String _Name)
        {
            if (this.HasBlueprint(_Name))
            {
                return this.blueprintPools[_Name];
            }

            return null;
        }

        /// <summary>
        /// Destroys the pool with the specified name.
        /// </summary>
        /// <param name="_Name">The name or identifier of the blueprint.</param>
        private void DestroyBlueprint(String _Name)
        {
            // Get the pool.
            GameObjectPool var_Pool = this.blueprintPools[_Name];

            // Destroy the pool.
            GameObject.Destroy(var_Pool.gameObject);

            // Remove the pool from the dictionary.
            this.blueprintPools.Remove(_Name);
        }

        #endregion

        #region Register

        /// <summary>
        /// Registers a GameObject as poolable blueprint to the pool. This process creates a new local 'sub'-pool, clones the passed GameObject 
        /// and attaches it to the new created 'sub'-pool as blueprint reference. If the GameObject has no <see cref="IPoolAble"/> component attached, 
        /// the <see cref="DefaultPoolAble"/> component will be added.
        /// </summary>
        /// <param name="_BlueprintPoolDefinition">The blueprint definition to register.</param></param>
        public void Register(BlueprintPoolDefinition _BlueprintPoolDefinition)
        {
            this.Register(_BlueprintPoolDefinition.PoolName, _BlueprintPoolDefinition.Blueprint, _BlueprintPoolDefinition.Strategy, _BlueprintPoolDefinition.InitialSize);
        }

        /// <summary>
        /// Registers a GameObject as poolable blueprint to the pool. This process creates a new local 'sub'-pool, clones the passed GameObject 
        /// and attaches it to the new created 'sub'-pool as blueprint reference. If the GameObject has no <see cref="IPoolAble"/> component attached, 
        /// the <see cref="DefaultPoolAble"/> component will be added.
        /// </summary>
        /// <param name="_Name">The name or identifier of the blueprint.</param>
        /// <param name="_Blueprint">The GameObject blueprint to register.</param>
        /// <param name="_PoolingStrategy">The pooling strategy to use (optional, default is <see cref="EPoolingStrategy.DEFAULT"/>).</param>
        /// <param name="_Capacity">The initial capacity of the pool (optional, default is 25).</param>
        public void Register(String _Name, GameObject _Blueprint, EPoolingStrategy _PoolingStrategy = EPoolingStrategy.DEFAULT, int _Capacity = 25)
        {
            lock (this.blueprintLock)
            {
                // Check if the pool already exists...
                if(this.blueprintPools.ContainsKey(_Name))
                {
                    // Show a warning.
                    Debug.LogWarning("A blueprint with the name '" + _Name + "' is already registered.");
                    return;
                }

                // ... else create the pool.
                this.CreateBlueprint(_Name, _Blueprint, _PoolingStrategy, _Capacity);
            }
        }

        #endregion

        #region Spawn - Empty

        /// <summary>
        /// Spawns an empty GameObject from the pool or creates a new one.
        /// </summary>
        /// <returns>The spawned GameObject.</returns>
        public GameObject Spawn()
        {
            return this.emptyPool.Spawn();
        }

        /// <summary>
        /// Spawns an empty GameObject from the pool or creates a new one at the specified position and rotation.
        /// </summary>
        /// <param name="_Position">The position at which to spawn the GameObject.</param>
        /// <param name="_Rotation">The rotation at which to spawn the GameObject.</param>
        public GameObject Spawn(Vector3 _Position, Quaternion _Rotation)
        {
            return this.emptyPool.Spawn(_Position, _Rotation);
        }

        /// <summary>
        /// Spawns an empty GameObject from the pool or creates a new one.
        /// </summary>
        /// <param name="_SpawnPolicy">The spawn policy to apply. Applied before OnSpawn.</param>
        /// <returns>The spawned GameObject.</returns>
        public GameObject Spawn(SpawnPolicy _SpawnPolicy)
        {
            return this.emptyPool.Spawn(_SpawnPolicy);
        }

        /// <summary>
        /// Spawns multiple empty GameObjects from the pool or creates new ones.
        /// </summary>
        /// <param name="_Count">The number of GameObjects to spawn.</param>
        /// <returns>An array of spawned GameObjects.</returns>
        public GameObject[] SpawnMany(int _Count)
        {
            GameObject[] var_Result = new GameObject[_Count];

            for (int i = 0; i < _Count; i++)
            {
                var_Result[i] = this.Spawn();
            }

            return var_Result;
        }

        /// <summary>
        /// Spawns multiple empty GameObjects from the pool or creates new ones, applying a spawn policy to each.
        /// </summary>
        /// <param name="_Count">The number of GameObjects to spawn.</param>
        /// <param name="_SpawnPolicy">The spawn policy to apply. Applied before OnSpawn.</param>
        /// <returns>An array of spawned GameObjects.</returns>
        public GameObject[] SpawnMany(int _Count, SpawnPolicy _SpawnPolicy)
        {
            GameObject[] var_Result = new GameObject[_Count];

            for (int i = 0; i < _Count; i++)
            {
                var_Result[i] = this.Spawn(_SpawnPolicy);
            }

            return var_Result;
        }

        /// <summary>
        /// Spawns an empty GameObject from the pool or creates a new one and decorates it with the provided decorator.
        /// </summary>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <returns>The spawned GameObject.</returns>
        public GameObject Spawn(IDecorator _Decorator)
        {
            return this.emptyPool.Spawn(_Decorator);
        }

        /// <summary>
        /// Spawns an empty GameObject from the pool or creates a new one and decorates it with the provided decorator at the specified position and rotation.
        /// </summary>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <param name="_Position">The position at which to spawn the GameObject.</param>
        /// <param name="_Rotation">The rotation at which to spawn the GameObject.</param>
        public GameObject Spawn(IDecorator _Decorator, Vector3 _Position, Quaternion _Rotation)
        {
            return this.emptyPool.Spawn(_Decorator, _Position, _Rotation);
        }

        /// <summary>
        /// Spawns an empty GameObject from the pool or creates a new one and decorates it with the provided decorator and applying a spawn policy.
        /// </summary>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <param name="_SpawnPolicy">The spawn policy to apply.</param>
        /// <returns>The spawned GameObject.</returns>
        public GameObject Spawn(IDecorator _Decorator, SpawnPolicy _SpawnPolicy)
        {
            return this.emptyPool.Spawn(_Decorator, _SpawnPolicy);
        }

        /// <summary>
        /// Spawns multiple empty GameObjects from the pool or creates new ones, decorating each with the provided decorator.
        /// </summary>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObjects.</param>
        /// <param name="_Count">The number of GameObjects to spawn.</param>
        /// <returns>An array of spawned GameObjects.</returns>
        public GameObject[] SpawnMany(IDecorator _Decorator, int _Count)
        {
            GameObject[] var_Result = new GameObject[_Count];

            for (int i = 0; i < _Count; i++)
            {
                var_Result[i] = this.Spawn(_Decorator);
            }

            return var_Result;
        }

        /// <summary>
        /// Spawns multiple empty GameObjects from the pool or creates new ones, decorating each with the provided decorator, and applying a spawn policy to each.
        /// </summary>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObjects.</param>
        /// <param name="_Count">The number of GameObjects to spawn.</param>
        /// <param name="_SpawnPolicy">The spawn policy to apply. Applied before OnSpawn.</param>
        /// <returns>An array of spawned GameObjects.</returns>
        public GameObject[] SpawnMany(IDecorator _Decorator, int _Count, SpawnPolicy _SpawnPolicy)
        {
            GameObject[] var_Result = new GameObject[_Count];

            for (int i = 0; i < _Count; i++)
            {
                var_Result[i] = this.Spawn(_Decorator, _SpawnPolicy);
            }

            return var_Result;
        }

        #endregion

        #region Spawn - Blueprint

        /// <summary>
        /// Spawns a GameObject based on its blueprint name.
        /// </summary>
        /// <param name="_Blueprint">The name or identifier of the blueprint to spawn.</param>
        /// <returns>The spawned GameObject.</returns>
        public GameObject Spawn(String _Blueprint)
        {
            // Get the pool.
            GameObjectPool var_Pool = this.GetBlueprint(_Blueprint);

            // If the pool is null, show a warning.
            if (var_Pool == null)
            {
                Debug.LogWarning("A blueprint with the name '" + _Blueprint + "' is not registered.");
                return null;
            }

            // Spawn the GameObject.
            return var_Pool.Spawn();
        }

        /// <summary>
        /// Spawns a GameObject based on its blueprint name at the specified position and rotation.
        /// </summary>
        /// <param name="_Blueprint">The name or identifier of the blueprint to spawn.</param>
        /// <param name="_Position">The position at which to spawn the GameObject.</param>
        /// <param name="_Rotation">The rotation at which to spawn the GameObject.</param>
        public GameObject Spawn(String _Blueprint, Vector3 _Position, Quaternion _Rotation)
        {
            // Get the pool.
            GameObjectPool var_Pool = this.GetBlueprint(_Blueprint);

            // If the pool is null, show a warning.
            if (var_Pool == null)
            {
                Debug.LogWarning("A blueprint with the name '" + _Blueprint + "' is not registered.");
                return null;
            }

            // Spawn the GameObject.
            return var_Pool.Spawn(_Position, _Rotation);
        }

        /// <summary>
        /// Spawn a GameObject based on its blueprint name, applying a spawn policy.
        /// </summary>
        /// <param name="_Blueprint">The name or identifier of the blueprint to spawn.</param>
        /// <param name="_SpawnPolicy">The spawn policy to apply. Applied before OnSpawn.</param>
        /// <returns>The spawned GameObject.</returns>
        public GameObject Spawn(String _Blueprint, SpawnPolicy _SpawnPolicy)
        {
            // Get the pool.
            GameObjectPool var_Pool = this.GetBlueprint(_Blueprint);

            // If the pool is null, show a warning.
            if (var_Pool == null)
            {
                Debug.LogWarning("A blueprint with the name '" + _Blueprint + "' is not registered.");
                return null;
            }

            // Spawn the GameObject.
            return var_Pool.Spawn(_SpawnPolicy);
        }

        /// <summary>
        /// Spawns a GameObject based on its blueprint name with a custom provided decorator.
        /// </summary>
        /// <param name="_Blueprint">The name or identifier of the blueprint to spawn.</param>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <returns>The spawned GameObject.</returns>
        public GameObject Spawn(String _Blueprint, IDecorator _Decorator)
        {
            return this.Spawn(_Blueprint, _Decorator, null);
        }

        /// <summary>
        /// Spawns a GameObject based on its blueprint name with a custom provided decorator at the specified position and rotation.
        /// </summary>
        /// <param name="_Blueprint">The name or identifier of the blueprint to spawn.</param>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <param name="_Position">The position at which to spawn the GameObject.</param>
        /// <param name="_Rotation">The rotation at which to spawn the GameObject.</param>
        public GameObject Spawn(String _Blueprint, IDecorator _Decorator, Vector3 _Position, Quaternion _Rotation)
        {
            // Get the pool.
            GameObjectPool var_Pool = this.GetBlueprint(_Blueprint);

            // If the pool is null, show a warning.
            if (var_Pool == null)
            {
                Debug.LogWarning("A blueprint with the name '" + _Blueprint + "' is not registered.");
                return null;
            }

            // Spawn the GameObject.
            return var_Pool.Spawn(_Decorator, _Position, _Rotation);
        }

        /// <summary>
        /// Spawns a GameObject based on its blueprint name with a custom provided decorator.
        /// </summary>
        /// <param name="_Blueprint">The name or identifier of the blueprint to spawn.</param>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <param name="_SpawnPolicy">The spawn policy to apply. Applied before OnSpawn.</param>
        /// <returns>The spawned GameObject.</returns>
        public GameObject Spawn(String _Blueprint, IDecorator _Decorator, SpawnPolicy _SpawnPolicy)
        {
            // Get the pool.
            GameObjectPool var_Pool = this.GetBlueprint(_Blueprint);

            // If the pool is null, show a warning.
            if (var_Pool == null)
            {
                Debug.LogWarning("A blueprint with the name '" + _Blueprint + "' is not registered.");
                return null;
            }

            // Spawn the GameObject.
            return var_Pool.Spawn(_Decorator, _SpawnPolicy);
        }

        /// <summary>
        /// Spawns multiple GameObjects based on the blueprint name.
        /// </summary>
        /// <param name="_Blueprint">The name or identifier of the blueprint to spawn.</param>
        /// <param name="_Count">The number of GameObjects to spawn.</param>
        /// <returns>An array of spawned GameObjects.</returns>
        public GameObject[] SpawnMany(String _Blueprint, int _Count)
        {
            return this.SpawnMany(_Blueprint, _Count, null);
        }

        /// <summary>
        /// Spawns multiple GameObjects based on the blueprint name, applying a spawn policy to each.
        /// </summary>
        /// <param name="_Blueprint">The name or identifier of the blueprint to spawn.</param>
        /// <param name="_Count">The number of GameObjects to spawn.</param>
        /// <param name="_SpawnPolicy">The spawn policy to apply. Applied before OnSpawn.</param>
        /// <returns>An array of spawned GameObjects.</returns>
        public GameObject[] SpawnMany(String _Blueprint, int _Count, SpawnPolicy _SpawnPolicy)
        {
            GameObject[] var_Result = new GameObject[_Count];

            for (int i = 0; i < _Count; i++)
            {
                var_Result[i] = this.Spawn(_Blueprint, _SpawnPolicy);
            }

            return var_Result;
        }

        /// <summary>
        /// Spawns multiple GameObjects based on the blueprint name with a custom provided decorator.
        /// </summary>
        /// <param name="_Blueprint">The name or identifier of the blueprint to spawn.</param>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <param name="_Count">The number of GameObjects to spawn.</param>
        /// <returns>An array of spawned GameObjects.</returns>
        public GameObject[] SpawnMany(String _Blueprint, IDecorator _Decorator, int _Count)
        {
            return this.SpawnMany(_Blueprint, _Decorator, _Count, null);
        }

        /// <summary>
        /// Spawns multiple GameObjects based on the blueprint name with a custom provided decorator, applying a spawn policy to each.
        /// </summary>
        /// <param name="_Blueprint">The name or identifier of the blueprint to spawn.</param>
        /// <param name="_Decorator">The decorator to apply to the spawned GameObject.</param>
        /// <param name="_Count">The number of GameObjects to spawn.</param>
        /// <param name="_SpawnPolicy">The spawn policy to apply.</param>
        /// <returns>An array of spawned GameObjects.</returns>
        public GameObject[] SpawnMany(String _Blueprint, IDecorator _Decorator, int _Count, SpawnPolicy _SpawnPolicy)
        {
            GameObject[] var_Result = new GameObject[_Count];

            for (int i = 0; i < _Count; i++)
            {
                var_Result[i] = this.Spawn(_Blueprint, _Decorator, _SpawnPolicy);
            }

            return var_Result;
        }

        #endregion

        #region Despawn

        /// <summary>
        /// Despawns a poolable GameObject back into the pool.
        /// </summary>
        /// <param name="_PoolAble">The poolable GameObject to despawn.</param>
        public void Despawn(GameObject _PoolAble)
        {
            // Despawn the GameObject.
            this.Despawn(_PoolAble, null);
        }

        /// <summary>
        /// Starts a coroutine to despawn the GameObject back into the pool, after a delay (seconds).
        /// </summary>
        /// <param name="_PoolAble">The poolable GameObject to despawn.</param>
        /// <param name="_Delay">The delay in seconds before despawning the GameObject.</param>
        public void Despawn(GameObject _PoolAble, float _Delay)
        {
            // Start a coroutine to despawn the GameObject after a delay.
            this.StartCoroutine(this.DespawnDelayed(_PoolAble, null, _Delay));
        }

        /// <summary>
        /// Despawns a poolable GameObject back into the pool using a provided un-decorator.
        /// </summary>
        /// <param name="_PoolAble">The poolable GameObject to despawn.</param>
        /// <param name="_UnDecorator">The un-decorator to apply to the despawned GameObject.</param>
        public void Despawn(GameObject _PoolAble, IUnDecorator _UnDecorator)
        {
            // Check if the GameObject is a poolable GameObject.
            IPoolAble var_PoolAble = _PoolAble.GetComponent<IPoolAble>();

            if (var_PoolAble == null)
            {
                // Show a warning.
                Debug.LogWarning("The GameObject '" + _PoolAble.name + "' is not a poolable GameObject.");
                return;
            }

            // Get the pool.
            IPool var_Pool = var_PoolAble.Owner;

            // If the pool is null, destroy the GameObject...
            if (var_Pool == null)
            {
                GameObject.Destroy(_PoolAble);
                return;
            }

            // ... else despawn the GameObject.

#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
            if (var_Pool == this.emptyPool)
            {
                // If the pool is the empty pool, use the full un-decorator.
                var_Pool.Despawn(_PoolAble, new FullUnDecorator());
            }
            else
            {
                // Else use the provided un-decorator.
                var_Pool.Despawn(_PoolAble, _UnDecorator);
            }
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
        }

        /// <summary>
        /// Starts a coroutine to despawn the GameObject back into the pool using a provided un-decorator, after a delay (seconds).
        /// </summary>
        /// <param name="_PoolAble">The poolable GameObject to despawn.</param>
        /// <param name="_UnDecorator">The un-decorator to apply to the despawned GameObject.</param>
        /// <param name="_Delay">The delay in seconds before despawning the GameObject.</param>
        public void Despawn(GameObject _PoolAble, IUnDecorator _UnDecorator, float _Delay)
        {
            this.StartCoroutine(this.DespawnDelayed(_PoolAble, _UnDecorator, _Delay));
        }

        /// <summary>
        /// The coroutine enumerator to despawn the GameObject back into the pool after a delay.
        /// </summary>
        /// <param name="_PoolAble">The poolable GameObject to despawn.</param>
        /// <param name="_UnDecorator">The un-decorator to apply to the despawned GameObject.</param>
        /// <param name="_Delay">The delay in seconds before despawning the GameObject.</param>
        /// <returns></returns>
        private IEnumerator DespawnDelayed(GameObject _PoolAble, IUnDecorator _UnDecorator, float _Delay)
        {
            // Wait for the delay.
            yield return new WaitForSeconds(_Delay);

            // Despawn the GameObject.
            this.Despawn(_PoolAble, _UnDecorator);
        }

        #endregion
    }
}
