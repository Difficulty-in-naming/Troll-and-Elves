using System;
using System.Collections.Generic;
using System.Reflection;
using EdgeStudio.UI.Component;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(AsyncImage),true),CanEditMultipleObjects]
public class ConvertImageToAsyncImage : ImageEditor
{
    protected PropertyTree tree;

    protected override void OnEnable()
    {
        base.OnEnable();

        EnsureTree();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        KillTree();
    }

    protected void EnsureTree()
    {
        if ( tree == null )
            tree = PropertyTree.Create( serializedObject );
    }

    protected void KillTree()
    {
        if ( tree != null )
            tree.Dispose();
        tree = null;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EnsureTree();

        tree.Draw( true );
    }
}

public class AsyncImageAttributeProcessor<T> : OdinAttributeProcessor<T> where T : AsyncImage
{
    public override bool CanProcessChildMemberAttributes( InspectorProperty parentProperty, MemberInfo member )
    {
        return !typeof( AsyncImage ).IsAssignableFrom( member.DeclaringType );
    }

    public override void ProcessChildMemberAttributes( InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes )
    {
        attributes.Add( new HideInInspector() );
    }
}

[CustomEditor(typeof(Image), true),CanEditMultipleObjects]
public class ConvertImageToAsyncImage2 : ImageEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("转换为AsyncImage"))
        {
            foreach (var node in targets)
            {
                var image = (Image)node;
                var go = image.gameObject;
        
                var sprite = image.sprite;
                var color = image.color;
                var raycastTarget = image.raycastTarget;
                var raycastPadding = image.raycastPadding;
                var maskable = image.maskable;
                var type = image.type;
                var fillAmount = image.fillAmount;
                var fillCenter = image.fillCenter;
                var fillClockwise = image.fillClockwise;
                var fillMethod = image.fillMethod;
                var fillOrigin = image.fillOrigin;
                var useSpriteMesh = image.useSpriteMesh;
                var preserveAspect = image.preserveAspect;

                DestroyImmediate(image);
                image = go.AddComponent<AsyncImage>();
                image.sprite = sprite;
                image.color = color;
                image.material = null;
                image.raycastTarget = raycastTarget;
                image.raycastPadding = raycastPadding;
                image.maskable = maskable;
                image.type = type;
                image.fillAmount = fillAmount;
                image.fillCenter = fillCenter;
                image.fillClockwise = fillClockwise;
                image.fillMethod = fillMethod;
                image.fillOrigin = fillOrigin;
                image.useSpriteMesh = useSpriteMesh;
                image.preserveAspect = preserveAspect;
                image.enabled = true;
            }
        }
    }
}