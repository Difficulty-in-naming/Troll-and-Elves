using System.Diagnostics;
using PantheaArch.Panthea.Utils;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace EdgeStudio.Odin
{
    [Conditional("UNITY_EDITOR")]
    public class ColorFoldoutAttribute : PropertyGroupAttribute
    {
        public string Path;
        public int ColorIndex;
        public ColorFoldoutAttribute(string path,int colorIndex = -1) : base(path)
        {
            Path = path;
            ColorIndex = colorIndex < 0 ? Colors.RandomColor(path) : colorIndex;
        }
    }
    
#if UNITY_EDITOR
    public class ColorFoldoutAttributeDrawer : OdinGroupDrawer<ColorFoldoutAttribute>
    {
        private Color groupColor;
        private GUIStyle headerStyle;

        protected override void Initialize()
        {
            groupColor = Colors.GetColorAt(Attribute.ColorIndex);
            headerStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14,
            };
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUILayout.Space(10f);
            Rect groupRect = EditorGUILayout.BeginVertical();

            Rect headerRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - 28, 28f);
            EditorGUI.DrawRect(headerRect, new Color32(43, 43, 43, 255));

            Rect foldoutRect = new Rect(headerRect.x + 26f, headerRect.y, headerRect.width - 12f, headerRect.height);
            var type = Property.SerializationRoot.ParentType.ToString();
            string key = type + "-" + Attribute.GroupName;
            this.Property.State.Expanded = EditorGUI.Foldout(foldoutRect, EditorPrefs.GetBool(key, true), Attribute.GroupName, true, headerStyle);
            EditorPrefs.SetBool(key, this.Property.State.Expanded);

            if (this.Property.State.Expanded)
            {
                var contentRect = EditorGUILayout.BeginVertical();
                EditorGUI.DrawRect(contentRect, new Color32(49, 49, 49, 255));
                EditorGUI.indentLevel++;
                for (int index = 0; index < this.Property.Children.Count; ++index)
                {
                    InspectorProperty property = this.Property.Children[index];
                    property.Draw();
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();

            Rect finalGroupRect = groupRect;
            finalGroupRect.height = GUILayoutUtility.GetLastRect().yMax - groupRect.y;

            Rect colorLineRect = new Rect(finalGroupRect.x, finalGroupRect.y, 3f, finalGroupRect.height);
            EditorGUI.DrawRect(colorLineRect, groupColor);
        }
    }
#endif
}