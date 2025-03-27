using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

[AttributeUsage(AttributeTargets.Field)]
public class EditableRectAttribute : Attribute
{
    public Color OutlineColor { get; set; } = new Color(0f, 0.75f, 1f, 1f);
    public Color FillColor { get; set; } = new Color(0f, 0.5f, 1f, 0.3f);
    
    public EditableRectAttribute() { }
    
    public EditableRectAttribute(float r, float g, float b)
    {
        OutlineColor = new Color(r, g, b, 1f);
        FillColor = new Color(r, g, b, 0.3f);
    }
}

#if UNITY_EDITOR
public static class RectEditManager
{
    private static Dictionary<UnityEngine.Object, Dictionary<string, RectData>> targetRects = new();
    
    private static Rect? copiedRect = null;
    
    private struct RectData
    {
        public EditableRectAttribute Attribute;
        public Func<Rect> GetRect;
        public Action<Rect> SetRect;
        public bool IsFlashing;
        public double FlashStartTime;
    }
    
    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        Selection.selectionChanged += OnSelectionChanged;
        EditorApplication.update += OnEditorUpdate;
    }
    
    private static void OnSelectionChanged()
    {
        SceneView.RepaintAll();
    }
    
    private static void OnEditorUpdate()
    {
        bool needsRepaint = false;
        
        UnityEngine.Object target = Selection.activeObject;
        if (target != null && targetRects.TryGetValue(target, out var rectDataDict))
        {
            foreach (var kvp in rectDataDict)
            {
                if (kvp.Value.IsFlashing)
                {
                    double elapsed = EditorApplication.timeSinceStartup - kvp.Value.FlashStartTime;
                    if (elapsed > 1.0)
                    {
                        var rectData = kvp.Value;
                        rectData.IsFlashing = false;
                        rectDataDict[kvp.Key] = rectData;
                    }
                    needsRepaint = true;
                }
            }
        }
        
        if (needsRepaint)
        {
            SceneView.RepaintAll();
        }
    }
    
    public static void RegisterRect(UnityEngine.Object target, string propertyName, Func<Rect> getter, Action<Rect> setter, EditableRectAttribute attribute)
    {
        if (target == null) return;
        var go = ((Component)target).gameObject;
        if (!targetRects.ContainsKey(go))
        {
            targetRects[go] = new Dictionary<string, RectData>();
        }
        
        targetRects[go][propertyName] = new RectData
        {
            Attribute = attribute,
            GetRect = getter,
            SetRect = setter,
            IsFlashing = false,
            FlashStartTime = 0
        };
    }
    
    public static void FocusAndFlash(UnityEngine.Object target, string propertyName)
    {
        if (target == null || !targetRects.ContainsKey(target) || 
            !targetRects[target].ContainsKey(propertyName)) return;
            
        var rectData = targetRects[target][propertyName];
        Rect rect = rectData.GetRect();
        
        FocusOnRect(rect);
        
        rectData.IsFlashing = true;
        rectData.FlashStartTime = EditorApplication.timeSinceStartup;
        targetRects[target][propertyName] = rectData;
        
        SceneView.RepaintAll();
    }
    
    public static void CopyRect(Rect rect) => copiedRect = rect;

    public static bool CanPaste() => copiedRect.HasValue;

    public static Rect? PasteRect() => copiedRect;

    private static void OnSceneGUI(SceneView sceneView)
    {
        var target = Selection.activeObject;
        if (target == null || !targetRects.ContainsKey(target)) return;
        
        if (!sceneView.in2DMode)
        {
            sceneView.in2DMode = true;
        }
        
        Event e = Event.current;
        
        var rectDataDict = targetRects[target];
        foreach (var kvp in rectDataDict)
        {
            string propertyName = kvp.Key;
            var rectData = kvp.Value;
            
            Rect rect = rectData.GetRect();
            
            // 计算控制点 - 9个点(4个角点，4个边中点，1个中心点)
            Vector3[] controlPoints = new Vector3[9];
            controlPoints[0] = new Vector3(rect.xMin, rect.yMin, 0); // 左下
            controlPoints[1] = new Vector3(rect.center.x, rect.yMin, 0); // 下中
            controlPoints[2] = new Vector3(rect.xMax, rect.yMin, 0); // 右下
            controlPoints[3] = new Vector3(rect.xMax, rect.center.y, 0); // 右中
            controlPoints[4] = new Vector3(rect.xMax, rect.yMax, 0); // 右上
            controlPoints[5] = new Vector3(rect.center.x, rect.yMax, 0); // 上中
            controlPoints[6] = new Vector3(rect.xMin, rect.yMax, 0); // 左上
            controlPoints[7] = new Vector3(rect.xMin, rect.center.y, 0); // 左中
            controlPoints[8] = new Vector3(rect.center.x, rect.center.y, 0); // 中心
            
            // 绘制矩形填充和轮廓
            Vector3[] rectCorners = new[] {
                controlPoints[0], // 左下
                controlPoints[2], // 右下
                controlPoints[4], // 右上
                controlPoints[6]  // 左上
            };
            
            Color fillColor = rectData.Attribute.FillColor;
            Color outlineColor = rectData.Attribute.OutlineColor;
            
            if (rectData.IsFlashing)
            {
                double elapsed = EditorApplication.timeSinceStartup - rectData.FlashStartTime;
                float flash = Mathf.PingPong((float)elapsed * 5f, 1f);
                
                outlineColor = Color.Lerp(outlineColor, Color.white, flash);
                fillColor = new Color(fillColor.r, fillColor.g, fillColor.b, 
                    Mathf.Lerp(fillColor.a, 0.7f, flash));
            }
            
            Handles.DrawSolidRectangleWithOutline(rectCorners, fillColor, outlineColor);
            
            // 处理中心点移动 - 移动整个矩形
            EditorGUI.BeginChangeCheck();
            
            float size = HandleUtility.GetHandleSize(controlPoints[8]) * 0.05f;
            
            // 绘制中心控制点
            Handles.color = Color.yellow;
            Vector3 centerPosition = Handles.FreeMoveHandle(
                controlPoints[8],
                size * 2,
                Vector3.zero,
                Handles.CircleHandleCap
            );
            
            if (EditorGUI.EndChangeCheck())
            {
                Vector2 delta = new Vector2(
                    centerPosition.x - controlPoints[8].x,
                    centerPosition.y - controlPoints[8].y
                );
                
                // 移动整个矩形
                Rect newRect = new Rect(
                    rect.x + delta.x,
                    rect.y + delta.y,
                    rect.width,
                    rect.height
                );
                
                rectData.SetRect(newRect);
                
                e.Use();
            }
            
            for (int i = 0; i < 8; i++) // 不包括中心点
            {
                Color pointColor = i % 2 == 0 ? Color.white : new Color(0.8f, 0.8f, 0.8f, 1f);
                Handles.color = pointColor;
                
                EditorGUI.BeginChangeCheck();
                
                float pointSize = i % 2 == 0 ? size : size * 0.8f; // 角点稍大，边中点稍小

                Vector3 newPosition = Handles.FreeMoveHandle(
                    controlPoints[i],
                    pointSize,
                    Vector3.zero,
                    i % 2 == 0 ? Handles.RectangleHandleCap : Handles.DotHandleCap
                );
                
                if (EditorGUI.EndChangeCheck())
                {
                    Rect newRect = rect;
                    
                    switch (i)
                    {
                        case 0: // 左下
                            newRect.xMin = newPosition.x;
                            newRect.yMin = newPosition.y;
                            break;
                        case 1: // 下中
                            newRect.yMin = newPosition.y;
                            break;
                        case 2: // 右下
                            newRect.xMax = newPosition.x;
                            newRect.yMin = newPosition.y;
                            break;
                        case 3: // 右中
                            newRect.xMax = newPosition.x;
                            break;
                        case 4: // 右上
                            newRect.xMax = newPosition.x;
                            newRect.yMax = newPosition.y;
                            break;
                        case 5: // 上中
                            newRect.yMax = newPosition.y;
                            break;
                        case 6: // 左上
                            newRect.xMin = newPosition.x;
                            newRect.yMax = newPosition.y;
                            break;
                        case 7: // 左中
                            newRect.xMin = newPosition.x;
                            break;
                    }
                    
                    if (newRect.width < 0)
                    {
                        newRect.x = newRect.x + newRect.width;
                        newRect.width = -newRect.width;
                    }
                    
                    if (newRect.height < 0)
                    {
                        newRect.y = newRect.y + newRect.height;
                        newRect.height = -newRect.height;
                    }
                    
                    rectData.SetRect(newRect);
                    
                    e.Use();
                }
            }
            
            DrawInfoLabel(rect, propertyName);
        }
        
        sceneView.Repaint();
    }
    
    private static void DrawInfoLabel(Rect rect, string propertyName)
    {
        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontSize = 12;
        labelStyle.richText = true;
        labelStyle.padding = new RectOffset(6, 6, 4, 4);
        
        Vector3 infoPos = new Vector3(rect.x, rect.y - 45, 0);
        
        GUIContent content = new GUIContent(
            $"<b>{propertyName}</b>\n宽: {rect.width:F2}, 高: {rect.height:F2}\n坐标: ({rect.x:F2}, {rect.y:F2})");
        
        Handles.Label(infoPos, content, labelStyle);
    }
    
    private static void FocusOnRect(Rect rect)
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            Vector2 center = new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f);
            float size = Mathf.Max(rect.width, rect.height) * 1.5f;
            size = Mathf.Max(size, 5.0f);
            
            sceneView.in2DMode = true;
            sceneView.Frame(new Bounds(new Vector3(center.x, center.y, 0), new Vector3(size, size, 0)), false);
            sceneView.Repaint();
            
            EditorWindow.FocusWindowIfItsOpen<SceneView>();
        }
    }
}

[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
public class EditableRectAttributeDrawer : OdinAttributeDrawer<EditableRectAttribute>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        UnityEngine.Object target = this.Property.SerializationRoot.ValueEntry.WeakSmartValue as UnityEngine.Object;
        if (target == null) return;
        
        string propertyName = this.Property.Name;
        EditableRectAttribute attribute = this.Attribute;
        
        if (this.Property.ValueEntry.TypeOfValue != typeof(Rect))
        {
            EditorGUILayout.HelpBox("EditableRect 属性只能用于 Rect 类型!", MessageType.Error);
            this.CallNextDrawer(label);
            return;
        }
        
        Rect currentRect = (Rect)this.Property.ValueEntry.WeakSmartValue;
        
        Func<Rect> getter = () => (Rect)this.Property.ValueEntry.WeakSmartValue;
        Action<Rect> setter = (newValue) => {
            Undo.RecordObject(target, "修改矩形");
            this.Property.ValueEntry.WeakSmartValue = newValue;
            EditorUtility.SetDirty(target);
        };
        
        RectEditManager.RegisterRect(target, propertyName, getter, setter, attribute);
        
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField(label, GUILayout.Width(100));
        
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("X", GUILayout.Width(30));
        float x = EditorGUILayout.FloatField(currentRect.x, GUILayout.Width(65));
        
        EditorGUILayout.LabelField("Y", GUILayout.Width(30));
        float y = EditorGUILayout.FloatField(currentRect.y, GUILayout.Width(65));
        
        EditorGUILayout.LabelField("W", GUILayout.Width(30));
        float width = EditorGUILayout.FloatField(currentRect.width, GUILayout.Width(65));
        
        EditorGUILayout.LabelField("H", GUILayout.Width(30));
        float height = EditorGUILayout.FloatField(currentRect.height, GUILayout.Width(65));
        EditorGUILayout.EndHorizontal();
        
        if (x != currentRect.x || y != currentRect.y || width != currentRect.width || height != currentRect.height)
        {
            setter(new Rect(x, y, width, height));
        }
        
        EditorGUILayout.EndVertical();
        
        // 按钮区域
        Color originalColor = GUI.color;
        GUI.color = attribute.OutlineColor;
        
        if (GUILayout.Button("跳转", GUILayout.Height(38), GUILayout.Width(60)))
        {
            RectEditManager.FocusAndFlash(target, propertyName);
        }
        
        GUI.color = originalColor;
        
        EditorGUILayout.BeginVertical(GUILayout.Width(60));
        
        if (GUILayout.Button("复制", GUILayout.Height(18)))
        {
            RectEditManager.CopyRect(currentRect);
        }
        
        GUI.enabled = RectEditManager.CanPaste();
        if (GUILayout.Button("粘贴", GUILayout.Height(18)))
        {
            Rect? pastedRect = RectEditManager.PasteRect();
            if (pastedRect.HasValue)
            {
                setter(pastedRect.Value);
            }
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
    }
}
#endif