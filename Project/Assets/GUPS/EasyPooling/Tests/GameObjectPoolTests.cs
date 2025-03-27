// System
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

// Test
using NUnit.Framework;

// Unity
using UnityEngine;
using UnityEngine.TestTools;

// GUPS
using GUPS.EasyPooling.Pooling;

namespace GUPS.EasyPooling.Tests
{
    public class GameObjectPoolTests
    {
        #region Setup

        [SetUp]
        public void Setup()
        {
        }

        #endregion

        #region Create

        [Test]
        public void GameObjectPool_Create_Empty()
        {
            // Create a pool.
            var var_Pool = GameObjectPool.Create("Empty", Strategy.EPoolingStrategy.DEFAULT, 25);

            // Check if the pool is not null.
            Assert.IsNotNull(var_Pool);

            // Check blueprint - Has the default pooling behaviour.
            Assert.AreEqual(1, var_Pool.Blueprint.Behaviours.Count);

            // Check if the pool is empty.
            Assert.AreEqual(0, var_Pool.Count);
        }

        [Test]
        public void GameObjectPool_Create_Empty_Fill()
        {
            // Create a pool.
            var var_Pool = GameObjectPool.Create("Empty", Strategy.EPoolingStrategy.FILL, 25);

            // Check if the pool is not null.
            Assert.IsNotNull(var_Pool);

            // Check if the pool is empty.
            Assert.AreEqual(25, var_Pool.Count);
        }

        [Test]
        public void GameObjectPool_Create_Cube()
        {
            // Get the cube prefab.
            var var_Cube = Resources.Load<GameObject>("EP/Test/Prefabs/Cube");

            // Create a pool.
            var var_Pool = GameObjectPool.Create(var_Cube, Strategy.EPoolingStrategy.DEFAULT, 25);

            // Check if the pool is not null.
            Assert.IsNotNull(var_Pool);

            // Check blueprint - Has the default pooling behaviour and the "MyBehaviour" behaviour.
            Assert.AreEqual(2, var_Pool.Blueprint.Behaviours.Count);

            // Check if the pool is empty.
            Assert.AreEqual(0, var_Pool.Count);
        }

        #endregion

        #region Spawn

        [Test]
        public void GameObjectPool_Spawn_Empty()
        {
            // Create a pool.
            var var_Pool = GameObjectPool.Create("Empty", Strategy.EPoolingStrategy.DEFAULT, 25);

            // Spawn a GameObject.
            var var_GameObject = var_Pool.Spawn();

            // Check if the GameObject is not null.
            Assert.IsNotNull(var_GameObject);

            // Check if the name is correct.
            Assert.AreEqual("Empty", var_GameObject.name);

            // Check if the pool is empty.
            Assert.AreEqual(0, var_Pool.Count);
        }

        #endregion

        #region Despawn

        [Test]
        public void GameObjectPool_Despawn_Empty()
        {
            // Create a pool.
            var var_Pool = GameObjectPool.Create("Empty", Strategy.EPoolingStrategy.DEFAULT, 25);

            // Spawn a GameObject.
            var var_GameObject = var_Pool.Spawn();

            // Check if the GameObject is not null.
            Assert.IsNotNull(var_GameObject);

            // Check if the name is correct.
            Assert.AreEqual("Empty", var_GameObject.name);

            // Check if the pool is empty.
            Assert.AreEqual(0, var_Pool.Count);

            // Despawn the GameObject.
            var_Pool.Despawn(var_GameObject);

            // Check if the pool is empty.
            Assert.AreEqual(1, var_Pool.Count);
        }

        #endregion
    }
}
