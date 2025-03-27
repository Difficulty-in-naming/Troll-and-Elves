using System;
using System.Collections.Generic;
using System.Reflection;
using EdgeStudio.UI.Component;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(BetterToggle), true), CanEditMultipleObjects]
public class ConvertToggleToBetterToggle : SelectableEditor
{
    SerializedProperty m_OnValueChangedProperty;
    SerializedProperty m_TransitionProperty;
    SerializedProperty m_GraphicProperty;
    SerializedProperty m_GroupProperty;
    SerializedProperty m_IsOnProperty;

    protected PropertyTree tree;
    protected override void OnEnable()
    {
        base.OnEnable();

        m_TransitionProperty = serializedObject.FindProperty("toggleTransition");
        m_GraphicProperty = serializedObject.FindProperty("graphic");
        m_GroupProperty = serializedObject.FindProperty("m_Group");
        m_IsOnProperty = serializedObject.FindProperty("m_IsOn");
        m_OnValueChangedProperty = serializedObject.FindProperty("onValueChanged");
        
        EnsureTree();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        KillTree();
    }
    
    protected void EnsureTree() => tree ??= PropertyTree.Create(serializedObject);

    protected void KillTree()
    {
        tree?.Dispose();
        tree = null;
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        serializedObject.Update();
        BetterToggle toggle = serializedObject.targetObject as BetterToggle;
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_IsOnProperty);
        if (EditorGUI.EndChangeCheck())
        {
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(toggle.gameObject.scene);

            BetterToggleGroup group = m_GroupProperty.objectReferenceValue as BetterToggleGroup;

            toggle.isOn = m_IsOnProperty.boolValue;

            if (group != null && group.isActiveAndEnabled && toggle.IsActive())
            {
                if (toggle.isOn || (!group.AnyTogglesOn()))
                {
                    toggle.isOn = true;
                }
            }
        }
        EditorGUILayout.PropertyField(m_TransitionProperty);
        EditorGUILayout.PropertyField(m_GraphicProperty);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_GroupProperty);
        if (EditorGUI.EndChangeCheck())
        {
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(toggle.gameObject.scene);

            BetterToggleGroup group = m_GroupProperty.objectReferenceValue as BetterToggleGroup;
            toggle.group = group;
        }

        EditorGUILayout.Space();

        // Draw the event notification options
        EditorGUILayout.PropertyField(m_OnValueChangedProperty);

        serializedObject.ApplyModifiedProperties();
        EnsureTree();
        tree.Draw();
    }
}

public class BetterToggleAttributeProcessor<T> : OdinAttributeProcessor<T> where T : BetterToggle
{
    public override bool CanProcessChildMemberAttributes( InspectorProperty parentProperty, MemberInfo member )
    {
        return !typeof( BetterToggle ).IsAssignableFrom( member.DeclaringType );
    }

    public override void ProcessChildMemberAttributes( InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes )
    {
        attributes.Add( new HideInInspector() );
    }
}

[CustomEditor(typeof(Toggle), true), CanEditMultipleObjects]
public class ConvertToggleToBetterToggle2 : ToggleEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.HelpBox("请勿使用Toggle,而是改用BetterToggle来进行业务开发", MessageType.Error);
        if (GUILayout.Button("转换为 BetterToggle")) ConvertToBetterToggle();
    }

private void ConvertToBetterToggle()
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Convert Toggle to BetterToggle");

        foreach (var target in targets)
        {
            var toggle = (Toggle)target;
            var go = toggle.gameObject;

            var isOn = toggle.isOn;
            var toggleTransition = toggle.toggleTransition;
            var graphic = toggle.graphic;
            var group = toggle.group;
            var colors = toggle.colors;
            var spriteState = toggle.spriteState;
            var animationTriggers = toggle.animationTriggers;
            var navigation = toggle.navigation;
            var transition = toggle.transition;
            var interactable = toggle.interactable;

            /*var displayHandler = go.GetComponent<ToggleDisplayStateHandler>();
            Graphic[] enableDisplays = null;
            Graphic[] disableDisplays = null;

            if (displayHandler != null)
            {
                enableDisplays = displayHandler.EnableDisplays;
                disableDisplays = displayHandler.DisableDisplays;
                
                Undo.DestroyObjectImmediate(displayHandler);
            }*/

            Undo.DestroyObjectImmediate(toggle);
            
            var betterToggle = go.AddComponent<BetterToggle>();
            Undo.RegisterCreatedObjectUndo(betterToggle, "Create BetterToggle");

            betterToggle.isOn = isOn;
            betterToggle.toggleTransition = toggleTransition;
            betterToggle.graphic = graphic;
            betterToggle.colors = colors;
            betterToggle.spriteState = spriteState;
            betterToggle.animationTriggers = animationTriggers;
            betterToggle.navigation = navigation;
            betterToggle.transition = transition;
            betterToggle.interactable = interactable;

            /*if (enableDisplays != null && disableDisplays != null)
            {
                betterToggle.EnableDisplays = enableDisplays;
                betterToggle.DisableDisplays = disableDisplays;
            }*/
        }
        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
    }
}