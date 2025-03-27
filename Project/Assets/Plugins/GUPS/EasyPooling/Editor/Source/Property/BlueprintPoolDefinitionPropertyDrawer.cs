// Unity
using UnityEngine;
using UnityEditor;

// GUPS
using GUPS.EasyPooling.Blueprint;
using GUPS.EasyPooling.Strategy;

namespace GUPS.EasyPooling.Editor.Property
{
    [CustomPropertyDrawer(typeof(BlueprintPoolDefinition))]
    public class BlueprintPoolDefinitionPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty _Property, GUIContent _Label)
        {
            return EditorGUIUtility.singleLineHeight * 6f;
        }

        public override void OnGUI(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            EditorGUI.BeginProperty(_Position, _Label, _Property);

            // Get the properties.
            var poolNameProperty = _Property.FindPropertyRelative("PoolName");
            var blueprintProperty = _Property.FindPropertyRelative("Blueprint");
            var strategyProperty = _Property.FindPropertyRelative("Strategy");
            var initialSizeProperty = _Property.FindPropertyRelative("InitialSize");

            // Draw the blueprint property.
            EditorGUI.PropertyField(new Rect(_Position.x, _Position.y, _Position.width, EditorGUIUtility.singleLineHeight), blueprintProperty, new GUIContent("Blueprint", "Assign a GameObject that you want to use as blueprint. For each blueprint a custom 'sub'-pool will be created. From this 'sub'-pool you can spawn performance optimized clones of the blueprint and also pool them."));

            // Assign the game object name to the pool name.
            if (blueprintProperty.objectReferenceValue != null)
            {
                poolNameProperty.stringValue = blueprintProperty.objectReferenceValue.name;
            }

            // Draw the pool name property as not editable.
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(new Rect(_Position.x, _Position.y + EditorGUIUtility.singleLineHeight, _Position.width, EditorGUIUtility.singleLineHeight), poolNameProperty, new GUIContent("Name (Identifier)", "Either use the referenced GameObject or this name as identifier to spawn new clones from the 'sub'-pool."));
            EditorGUI.EndDisabledGroup();

            // Draw a label for the strategy.
            EditorGUI.LabelField(new Rect(_Position.x, _Position.y + EditorGUIUtility.singleLineHeight * 2f, _Position.width, EditorGUIUtility.singleLineHeight), new GUIContent("Strategy", "The strategy defines how the 'sub'-pool will behave."));

            // Increase the indent.
            EditorGUI.indentLevel++;

            // Draw a checkbox for the fill strategy.
            var fillStrategy = (strategyProperty.intValue & (int)EPoolingStrategy.FILL) != 0;

            fillStrategy = EditorGUI.Toggle(new Rect(_Position.x, _Position.y + EditorGUIUtility.singleLineHeight * 3f, _Position.width, EditorGUIUtility.singleLineHeight), new GUIContent("- Fill", "Fill the blueprint 'sub'-pool with 'initial size' clones. Useful when using loading scenes/screens."), fillStrategy);

            // Draw a checkbox for the grow strategy.
            var growStrategy = (strategyProperty.intValue & (int)EPoolingStrategy.GROW) != 0;

            growStrategy = EditorGUI.Toggle(new Rect(_Position.x, _Position.y + EditorGUIUtility.singleLineHeight * 4f, _Position.width, EditorGUIUtility.singleLineHeight), new GUIContent("- Grow", "The capacity of the blueprint 'sub'-pool will smart grow or degrow."), growStrategy);

            // Assign the strategy.
            strategyProperty.intValue = (fillStrategy ? (int)EPoolingStrategy.FILL : 0) | (growStrategy ? (int)EPoolingStrategy.GROW : 0);

            // Decrease the indent.
            EditorGUI.indentLevel--;

            // Draw number box for the initial size.
            EditorGUI.PropertyField(new Rect(_Position.x, _Position.y + EditorGUIUtility.singleLineHeight * 5f, _Position.width, EditorGUIUtility.singleLineHeight), initialSizeProperty);

            EditorGUI.EndProperty();
        }
    }
}
