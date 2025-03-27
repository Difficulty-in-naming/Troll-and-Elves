// System
using GUPS.EasyPooling.Strategy;
using System;
using System.Reflection;

// Unity
using UnityEngine;

namespace GUPS.EasyPooling.Blueprint
{
    /// <summary>
    /// Represents a definition for a GameObject pool, specifying its name, GameObject blueprint, pooling strategy, and initial size 
    /// (the number of storeable pooled GameObjects).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="BlueprintPoolDefinition"/> class is a container for defining the properties of a GameObject pool. It includes 
    /// details such as the pool's name (and so the GameObjects), the associated blueprint GameObject, the pooling strategy to be 
    /// employed, and the initial size of the pool.
    /// </para>
    /// </remarks>
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class BlueprintPoolDefinition
    {
        /// <summary>
        /// Gets or sets the name of the GameObject pool.
        /// </summary>
        [SerializeField]
        public string PoolName;

        /// <summary>
        /// Gets or sets the blueprint GameObject associated with the pool.
        /// </summary>
        [SerializeField]
        public GameObject Blueprint;

        /// <summary>
        /// Gets or sets the pooling strategy employed by the GameObject pool.
        /// </summary>
        [SerializeField]
        public EPoolingStrategy Strategy;

        /// <summary>
        /// Gets or sets the initial size of the GameObject pool.
        /// </summary>
        [SerializeField]
        public int InitialSize;
    }

}
