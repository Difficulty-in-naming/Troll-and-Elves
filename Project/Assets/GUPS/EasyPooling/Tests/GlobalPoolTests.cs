// System
using System.Collections;

// Test
using NUnit.Framework;

// Unity
using UnityEngine;
using UnityEngine.TestTools;

// GUPS
using GUPS.EasyPooling.Policy;
using GUPS.EasyPooling.Policy.Random;

namespace GUPS.EasyPooling.Tests
{
    public class GlobalPoolTests
    {
        #region Setup

        /// <summary>
        /// The GlobalPool instance.
        /// </summary>
        private GlobalPool globalPool;

        [SetUp]
        public void Setup()
        {
            // Get / Create the GlobalPool instance.
            this.globalPool = GlobalPool.Instance;

            // Register the cube prefab.
            this.globalPool.Register("Cube", Resources.Load<GameObject>("EP/Test/Prefabs/Cube"));

            // Register the sphere prefab.
            this.globalPool.Register("Sphere", Resources.Load<GameObject>("EP/Test/Prefabs/Sphere"));
        }

        #endregion

        #region TearDown

        [TearDown]
        public void TearDown()
        {
            // Destroy the GlobalPool instance.
            GameObject.DestroyImmediate(this.globalPool.gameObject);
        }

        #endregion

        #region Has

        [Test]
        public void GlobalPool_Has()
        {
            // Check if the GlobalPool has the cube prefab.
            Assert.IsTrue(this.globalPool.HasBlueprint("Cube"));

            // Check if the GlobalPool has the sphere prefab.
            Assert.IsTrue(this.globalPool.HasBlueprint("Sphere"));

            // Check if the GlobalPool has not the capsule prefab.
            Assert.IsFalse(this.globalPool.HasBlueprint("Capsule"));
        }

        #endregion

        #region Spawn

        [Test]
        public void GlobalPool_Spawn_Empty()
        {
            // Spawn an empty GameObject.
            var var_GameObject = this.globalPool.Spawn();

            // Check if the GameObject is not null.
            Assert.IsNotNull(var_GameObject);

            // Check if the GameObject has the name "Empty".
            Assert.AreEqual("Empty", var_GameObject.name);
        }

        [Test]
        public void GlobalPool_Spawn_Cube()
        {
            // Spawn a cube GameObject.
            var var_GameObject = this.globalPool.Spawn("Cube");
            
            // Check if the GameObject is not null.
            Assert.IsNotNull(var_GameObject);

            // Check if the GameObject has the name "Cube".
            Assert.AreEqual("Cube", var_GameObject.name);
        }

        [Test]
        public void GlobalPool_Spawn_Sphere()
        {
            // Spawn a sphere GameObject.
            var var_GameObject = this.globalPool.Spawn("Sphere");

            // Check if the GameObject is not null.
            Assert.IsNotNull(var_GameObject);

            // Check if the GameObject has the name "Sphere".
            Assert.AreEqual("Sphere", var_GameObject.name);
        }

        [Test]
        public void GlobalPool_Spawn_Capsule()
        {
            // Try to spawn a capsule GameObject.
            var var_GameObject = this.globalPool.Spawn("Capsule");

            // Check if the GameObject is null.
            Assert.IsNull(var_GameObject);
        }

        [Test]
        public void GlobalPool_Spawn_Empty_WithDecorator()
        {
            // Create a decorator.
            var var_Decorator = new Mock.MyDecorator();

            // Spawn an empty GameObject.
            var var_GameObject = this.globalPool.Spawn(var_Decorator);

            // Check if the GameObject is not null.
            Assert.IsNotNull(var_GameObject);

            // Check if the GameObject has the MyBehaviourC component.
            Assert.IsNotNull(var_GameObject.GetComponent<Mock.MyBehaviourC>());

            // Check if the GameObject has the MyBehaviourC component with the correct values.
            Assert.AreEqual(123, var_GameObject.GetComponent<Mock.MyBehaviourC>().valueDouble);
            Assert.AreEqual("Nice!", var_GameObject.GetComponent<Mock.MyBehaviourC>().valueString);
        }

        #endregion

        #region Spawn Many

        [Test]
        public void GlobalPool_SpawnMany_Empty()
        {
            // Spawn 10 empty GameObjects.
            var var_GameObjects = this.globalPool.SpawnMany(10);

            // Check if the GameObjects are not null.
            Assert.IsNotNull(var_GameObjects);

            // Check if the GameObjects length is 10.
            Assert.AreEqual(10, var_GameObjects.Length);

            // Check if the GameObjects have the name "Empty".
            foreach (var var_GameObject in var_GameObjects)
            {
                Assert.AreEqual("Empty", var_GameObject.name);
            }
        }

        #endregion

        #region Spawn with policy

        [Test]
        public void GlobalPool_Spawn_Cube_WithPolicy()
        {
            // Create a policy.
            SpawnPolicy var_Policy = new SpawnPolicyBuilder(new GaussianRandom(10)).Is3D().SetCenter(Vector3.zero).SpawnInSquare().Build();

            // Spawn multiple cube GameObjects with the policy.
            var var_GameObjects = this.globalPool.SpawnMany("Cube", 10, var_Policy);

            // Check if the GameObjects are not null.
            Assert.IsNotNull(var_GameObjects);

            // Check if the GameObjects length is 10.
            Assert.AreEqual(10, var_GameObjects.Length);

            // Check if the GameObjects have the name "Cube".
            foreach (var var_GameObject in var_GameObjects)
            {
                Assert.AreEqual("Cube", var_GameObject.name);
            }
        }

        #endregion

        #region Despawn

        [Test]
        public void GlobalPool_Despawn_Empty()
        {
            // Spawn an empty GameObject.
            var var_GameObject = this.globalPool.Spawn();

            // Check if the GameObject is not null.
            Assert.IsNotNull(var_GameObject);

            // Check if the GameObject has the name "Empty".
            Assert.AreEqual("Empty", var_GameObject.name);

            // Despawn the GameObject.
            this.globalPool.Despawn(var_GameObject);

            // Check if the GameObject is inactive.
            Assert.IsFalse(var_GameObject.activeSelf);
        }

        [Test]
        public void GlobalPool_Despawn_Cube()
        {
            // Spawn a cube GameObject.
            var var_GameObject = this.globalPool.Spawn("Cube");

            // Check if the GameObject is not null.
            Assert.IsNotNull(var_GameObject);

            // Check if the GameObject has the name "Cube".
            Assert.AreEqual("Cube", var_GameObject.name);

            // Despawn the GameObject.
            this.globalPool.Despawn(var_GameObject);

            // Check if the GameObject is inactive.
            Assert.IsFalse(var_GameObject.activeSelf);
        }

        [Test]
        public void GlobalPool_Despawn_Sphere()
        {
            // Spawn a sphere GameObject.
            var var_GameObject = this.globalPool.Spawn("Sphere");

            // Check if the GameObject is not null.
            Assert.IsNotNull(var_GameObject);

            // Check if the GameObject has the name "Sphere".
            Assert.AreEqual("Sphere", var_GameObject.name);

            // Despawn the GameObject.
            this.globalPool.Despawn(var_GameObject);

            // Check if the GameObject is inactive.
            Assert.IsFalse(var_GameObject.activeSelf);
        }

        #endregion

        #region Despawn - Delayed

        [UnityTest]
        public IEnumerator GlobalPool_Despawn_Delayed_Empty()
        {
            // Spawn an empty GameObject.
            var var_GameObject = this.globalPool.Spawn();

            // Check if the GameObject is not null.
            Assert.IsNotNull(var_GameObject);

            // Check if the GameObject has the name "Empty".
            Assert.AreEqual("Empty", var_GameObject.name);

            // Despawn the GameObject in 2 seconds.
            this.globalPool.Despawn(var_GameObject, 2);

            // Check if the GameObject is active.
            Assert.IsTrue(var_GameObject.activeSelf);

            // Wait for 2 + 1 (to be sure) seconds.
            yield return new WaitForSeconds(3);

            // Check if the GameObject is inactive.
            Assert.IsFalse(var_GameObject.activeSelf);
        }

        #endregion
    }
}
