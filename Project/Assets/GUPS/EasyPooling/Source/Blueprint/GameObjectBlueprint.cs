// System
using System;
using System.Collections.Generic;
using System.Reflection;

// Unity
using UnityEngine;

// GUPS
using GUPS.EasyPooling.Decorator;

namespace GUPS.EasyPooling.Blueprint
{
    /// <summary>
    /// Represents a blueprint for a GameObject, specifying its name and associated behavior blueprints.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="GameObjectBlueprint"/> class allows the definition of a blueprint for a GameObject,
    /// encapsulating information such as its name and a list of associated <see cref="BehaviourBlueprint"/> instances.
    /// </para>
    /// <para>
    /// The class implements the <see cref="IBlueprint"/> and <see cref="IDecorator"/> interfaces, providing a name
    /// for a GameObject and enabling the decoration of a poolable GameObject by adding and configuring behaviors based 
    /// on the behaviour blueprints.
    /// </para>
    /// </remarks>
    public class GameObjectBlueprint : IBlueprint, IDecorator
    {
        /// <summary>
        /// Gets the name of the GameObject.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Gets a list of behavior blueprints associated with this GameObject.
        /// </summary>
        public List<BehaviourBlueprint> Behaviours { get; private set; }

        /// <summary>
        /// Apply this decorator when a new GameObject is created or respawned.
        /// </summary>
        public bool OnCreateOnly => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameObjectBlueprint"/> class.
        /// </summary>
        /// <param name="_Name">The name of the GameObject.</param>
        public GameObjectBlueprint(String _Name)
            : this(_Name, new List<BehaviourBlueprint>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameObjectBlueprint"/> class.
        /// </summary>
        /// <param name="_Name">The name of the GameObject.</param>
        /// <param name="_Behaviours">The list of behavior blueprints associated with this GameObject.</param>
        public GameObjectBlueprint(String _Name, List<BehaviourBlueprint> _Behaviours)
        {
            this.Name = _Name;
            this.Behaviours = _Behaviours;
        }

        /// <summary>
        /// Decorates a GameObject by adding and configuring its behaviors based on the blueprint.
        /// </summary>
        /// <param name="_GameObject">The GameObject to decorate.</param>
        public void OnDecorate(GameObject _GameObject)
        {
            // Loop through all the behaviors.
            foreach (BehaviourBlueprint var_Behaviour in Behaviours)
            {
                try
                {
                    // Get the behavior.
                    MonoBehaviour var_MonoBehaviour = _GameObject.GetComponent(var_Behaviour.Type) as MonoBehaviour;

                    // If the behavior is null, create it.
                    if (var_MonoBehaviour == null)
                    {
                        // Create the behavior.
                        var_MonoBehaviour = _GameObject.AddComponent(var_Behaviour.Type) as MonoBehaviour;
                    }

                    // Loop through all the fields.
                    foreach (FieldInfo var_FieldInfo in var_Behaviour.Fields)
                    {
                        // Try to get the default value and assign it.
                        if (var_Behaviour.DefaultValues.TryGetValue(var_FieldInfo.Name, out object var_DefaultValue))
                        {
                            // Set the value.
                            var_FieldInfo.SetValue(var_MonoBehaviour, var_DefaultValue);
                        }
                    }
                }
                catch (Exception _Exception)
                {
                    Debug.LogError($"Failed to decorate GameObject '{_GameObject.name}' with behavior '{var_Behaviour.Name}' of type '{var_Behaviour.Type.Name}'.");
                    Debug.LogException(_Exception);
                }
            }
        }

        /// <summary>
        /// Creates a GameObject blueprint based on an existing GameObject and its behaviors.
        /// </summary>
        /// <param name="_GameObject">The GameObject instance to create the blueprint from.</param>
        /// <returns>A new <see cref="GameObjectBlueprint"/> instance.</returns>
        public static GameObjectBlueprint Create(GameObject _GameObject)
        {
            try
            {
                // Create a new blueprint.
                GameObjectBlueprint var_Blueprint = new GameObjectBlueprint(_GameObject.name);

                // Get all the behaviors.
                MonoBehaviour[] var_Behaviours = _GameObject.GetComponents<MonoBehaviour>();

                // Loop through all the behaviors.
                foreach (MonoBehaviour var_Behaviour in var_Behaviours)
                {
                    // Create a new behavior blueprint.
                    BehaviourBlueprint behaviourBlueprint = BehaviourBlueprint.Create(var_Behaviour);

                    // If something went wrong, continue.
                    if (behaviourBlueprint == null)
                    {
                        continue;
                    }

                    // Add the behavior blueprint to the GameObject blueprint.
                    var_Blueprint.Behaviours.Add(behaviourBlueprint);
                }

                // Return the blueprint.
                return var_Blueprint;
            }
            catch (Exception _Exception)
            {
                Debug.LogError($"Failed to create blueprint for GameObject '{_GameObject.name}' of type '{_GameObject.GetType().Name}'.");
                Debug.LogException(_Exception);

                return null;
            }
        }
    }
}
