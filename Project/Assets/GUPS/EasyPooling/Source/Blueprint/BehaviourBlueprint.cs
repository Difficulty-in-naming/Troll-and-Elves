// System
using System;
using System.Collections.Generic;
using System.Reflection;

// Unity
using UnityEngine;

namespace GUPS.EasyPooling.Blueprint
{
    /// <summary>
    /// Represents a blueprint for managing and creating instances of behaviors, providing information about their structure and default values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="BehaviourBlueprint"/> class implements the <see cref="IBlueprint"/> interface and serves as a blueprint for behavior
    /// instances. It encapsulates details such as the behavior's name, type, default values, and field information.
    /// </para>
    /// <para>
    /// The public properties include <see cref="Name"/> to retrieve the behavior's name, <see cref="Type"/> for its type,
    /// <see cref="DefaultValues"/> for default property values, and <see cref="Fields"/> to access information about the behavior's fields.
    /// </para>
    /// <para>
    /// The <see cref="Create"/> method allows the creation of a new <see cref="BehaviourBlueprint"/> based on an existing behavior instance,
    /// extracting default values from its fields. This method is useful for generating blueprints dynamically.
    /// </para>
    /// </remarks>
    public class BehaviourBlueprint : IBlueprint
    {
        /// <summary>
        /// Gets the name of the behavior.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Gets the type of the behavior.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets the default values for the behavior. 
        /// Returns a dictionary with the name of the property as the key and the default value as the value.
        /// </summary>
        public Dictionary<String, object> DefaultValues { get; private set; }

        /// <summary>
        /// Stores information about the fields of the behavior.
        /// </summary>
        public FieldInfo[] Fields { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviourBlueprint"/> class.
        /// </summary>
        /// <param name="_Name">The name of the behavior.</param>
        /// <param name="_Type">The type of the behavior.</param>
        public BehaviourBlueprint(String _Name, Type _Type)
            : this(_Name, _Type, new Dictionary<String, object>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviourBlueprint"/> class.
        /// </summary>
        /// <param name="_Name">The name of the behavior.</param>
        /// <param name="_Type">The type of the behavior.</param>
        /// <param name="_DefaultValues">The default values for the behavior.</param>
        public BehaviourBlueprint(String _Name, Type _Type, Dictionary<String, object> _DefaultValues)
        {
            // Assign the parameters.
            Name = _Name;
            Type = _Type;
            DefaultValues = _DefaultValues;

            // Find all field infos.
            this.Fields = this.GetFields(_Type).ToArray();
        }

        /// <summary>
        /// Find recursively all fields of the type. Stop if the type is null or a MonoBehaviour / Behaviour.
        /// </summary>
        /// <param name="_Type">The type to get the fields from.</param>
        /// <returns>A list of all fields of the type.</returns>
        private List<FieldInfo> GetFields(Type _Type)
        {
            // Return an empty list if the type is null or a MonoBehaviour / Behaviour.
            if (_Type == null || _Type.Equals(typeof(MonoBehaviour)) || _Type.Equals(typeof(Behaviour)))
            {
                return new List<FieldInfo>();
            }

            // Create a result list.
            List<FieldInfo> var_Result = new List<FieldInfo>();

            // Get the fields of the type.
            FieldInfo[] var_Fields = _Type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // Filter only primitives and serializeable fields.
            var_Fields = Array.FindAll(var_Fields, (FieldInfo _Field) => (IsPrimitive(_Field.FieldType) || IsUnityPrimitive(_Field.FieldType)));

            // Add the fields to the result list.
            var_Result.AddRange(var_Fields);

            // Get the base type fields.
            var_Result.AddRange(GetFields(_Type.BaseType));

            // Return the result.
            return var_Result;
        }

        /// <summary>
        /// Creates a new <see cref="BehaviourBlueprint"/> based on an existing <see cref="Behaviour"/>.
        /// </summary>
        /// <param name="_Behaviour">The behavior instance to create the blueprint from.</param>
        /// <returns>A new <see cref="BehaviourBlueprint"/> instance.</returns>
        public static BehaviourBlueprint Create(Behaviour _Behaviour)
        {
            try
            {
                // Create a new behavior blueprint.
                BehaviourBlueprint var_Blueprint = new BehaviourBlueprint(_Behaviour.name, _Behaviour.GetType());

                // Store the default values.
                Dictionary<String, object> _DefaultValues = new Dictionary<String, object>();

                // Iterate over all fields and store the default values of public fields or private fields with the SerializeField attribute.
                foreach (FieldInfo var_Field in var_Blueprint.Fields)
                {
                    _DefaultValues[var_Field.Name] = var_Field.GetValue(_Behaviour);
                }

                // Assign the default values.
                var_Blueprint.DefaultValues = _DefaultValues;

                // Return the blueprint.
                return var_Blueprint;
            }
            catch(Exception _Exception)
            {
                Debug.LogError("Failed to create blueprint for behaviour '" + _Behaviour.name + "' of type '" + _Behaviour.GetType().Name + "'.");
                Debug.LogException(_Exception);

                return null;
            }
        }

        /// <summary>
        /// Check if the type is a primitive.
        /// </summary>
        /// <param name="_FieldType">The fiel type to check.</param>
        /// <returns>True, if the type is a primitive.</returns>
        private static bool IsPrimitive(Type _FieldType)
        {
            if (_FieldType == typeof(int))
            {
                return true;
            }
            else if (_FieldType == typeof(uint))
            {
                return true;
            }
            else if (_FieldType == typeof(short))
            {
                return true;
            }
            else if (_FieldType == typeof(ushort))
            {
                return true;
            }
            else if (_FieldType == typeof(long))
            {
                return true;
            }
            else if (_FieldType == typeof(ulong))
            {
                return true;
            }
            else if (_FieldType == typeof(byte))
            {
                return true;
            }
            else if (_FieldType == typeof(sbyte))
            {
                return true;
            }
            else if (_FieldType == typeof(char))
            {
                return true;
            }
            else if (_FieldType == typeof(float))
            {
                return true;
            }
            else if (_FieldType == typeof(double))
            {
                return true;
            }
            else if (_FieldType == typeof(bool))
            {
                return true;
            }
            else if (_FieldType == typeof(decimal))
            {
                return true;
            }
            else if (_FieldType == typeof(string))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the type is a unity primitive.
        /// </summary>
        /// <param name="_FieldType">The fiel type to check.</param>
        /// <returns>True, if the type is a unity primitive.</returns>
        private static bool IsUnityPrimitive(Type _FieldType)
        {
            if (_FieldType == typeof(Color))
            {
                return true;
            }
            else if (_FieldType == typeof(Color32))
            {
                return true;
            }
            else if (_FieldType == typeof(Vector2))
            {
                return true;
            }
            else if (_FieldType == typeof(Vector2Int))
            {
                return true;
            }
            else if (_FieldType == typeof(Vector3))
            {
                return true;
            }
            else if (_FieldType == typeof(Vector3Int))
            {
                return true;
            }
            else if (_FieldType == typeof(Vector4))
            {
                return true;
            }
            else if (_FieldType == typeof(Quaternion))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the field is serializable.
        /// </summary>
        /// <param name="_Field">The field to check.</param>
        /// <returns>True, if the field is serializable.</returns>
        private static bool IsSerializable(FieldInfo _Field)
        {
            bool var_HasSerializeFieldAttribute = Attribute.IsDefined(_Field, typeof(SerializeField));

            return _Field.IsPublic || _Field.IsPrivate && var_HasSerializeFieldAttribute;
        }
    }
}
