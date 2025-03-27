using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Panthea.Common;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using System.Collections;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace EdgeStudio
{
    [Conditional("UNITY_EDITOR")]
    public class BindFieldAttribute : PropertyGroupAttribute
    {
        public string Path;
        public bool IsAsset;
        public bool SearchInParent;
        public BindFieldAttribute(string path = "", bool isAsset = false,bool searchInParent = false) : base("绑定对象")
        {
            Path = path;
            IsAsset = isAsset;
            SearchInParent = searchInParent;
        }
    }
#if UNITY_EDITOR

    public class BindFieldAttributeDrawer : OdinGroupDrawer<BindFieldAttribute>
    {
        private ValueResolver<string> labelGetter;
        protected override void Initialize() => this.labelGetter = ValueResolver.GetForString(this.Property, this.Attribute.GroupName);
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.labelGetter.DrawError();
        
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            this.Property.State.Expanded = SirenixEditorGUI.Foldout(this.Property.State.Expanded, "View");
            SirenixEditorGUI.EndBoxHeader();
            if (SirenixEditorGUI.BeginFadeGroup((object) this, this.Property.State.Expanded))
            {
                for (int index = 0; index < this.Property.Children.Count; ++index)
                {
                    InspectorProperty child = this.Property.Children[index];
                    child.Draw(child.Label);
                }
                DrawButton();
            }
            SirenixEditorGUI.EndFadeGroup();
            SirenixEditorGUI.EndBox();
        }

        void DrawButton()
        {
            if (GUILayout.Button("自动绑定"))
            {
                // 获取所有选中的对象
                var selectedObjects = Selection.gameObjects;
                if (selectedObjects == null || selectedObjects.Length == 0)
                    return;

                // 对每个选中的对象执行绑定操作
                foreach (var selectedObject in selectedObjects)
                {
                    Bind(selectedObject, Property);
                }
            }
        }

        public class WrapField
        {
            private FieldInfo mFieldInfo;
            private MonoBehaviour mInstance;
            private InspectorProperty mChildren;
            public WrapField(FieldInfo node, MonoBehaviour instance)
            {
                mFieldInfo = node;
                mInstance = instance;
            }

            public WrapField(InspectorProperty node)
            {
                mChildren = node;
            }

            public T GetAttribute<T>() where T : Attribute
            {
                return mFieldInfo != null ? mFieldInfo.GetCustomAttribute<T>() : mChildren.GetAttribute<T>();
            }
        
            public string Name => mFieldInfo != null ? mFieldInfo.Name : mChildren.Name;
            public Type Type => mFieldInfo != null ? mFieldInfo.FieldType : mChildren.BaseValueEntry.BaseValueType;

            public object Value
            {
                get => mFieldInfo != null ? mFieldInfo.GetValue(mInstance) : mChildren.BaseValueEntry.WeakSmartValue;
                set
                {
                    if (mFieldInfo != null)
                    {
                        mFieldInfo.SetValue(mInstance, value);
                        EditorUtility.SetDirty(mInstance);
                    }
                    else
                        mChildren.BaseValueEntry.WeakSmartValue = value;
                }
            }

            public void ToArray(List<object> list, Type type)
            {
                Array array = Array.CreateInstance(type, list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    array.SetValue(ConvertToType(list[i], type), i);
                }
                Value = array;
            }

            private object ConvertToType(object obj, Type type)
            {
                if (obj == null)
                {
                    return null;
                }
                Type objType = obj.GetType();
                if (objType == type)
                {
                    return obj;
                }
                else if (type.IsAssignableFrom(objType))
                {
                    return obj;
                }
                else if (objType.IsAssignableFrom(type))
                {
                    return Convert.ChangeType(obj, type);
                }
                else
                {
                    return null;
                }
            }
            
            public void SetCollection(List<object> list, Type elementType)
            {
                if (Type.IsArray)
                {
                    // 处理数组类型
                    Array array = Array.CreateInstance(elementType, list.Count);
                    for (int i = 0; i < list.Count; i++)
                    {
                        array.SetValue(ConvertToType(list[i], elementType), i);
                    }
                    Value = array;
                }
                else if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    // 处理List类型
                    Type listType = typeof(List<>).MakeGenericType(elementType);
                    IList collection = (IList)Activator.CreateInstance(listType);
                    foreach (var item in list)
                    {
                        object converted = ConvertToType(item, elementType);
                        if (converted != null)
                        {
                            collection.Add(converted);
                        }
                    }
                    Value = collection;
                }
            }
        }

        public class WrapObj
        {
            private MonoBehaviour[] mController;
            private InspectorProperty mProperty;
            public WrapObj(GameObject go, InspectorProperty property = null)
            {
                //这里我们不能使用GetComponent,如果是这样的组件结构
                //---Image
                //---Canvas
                //---MyItem
                //我们只能获得Image组件.这会导致绑定组件失效
                mController = go.GetComponents<MonoBehaviour>();
                mProperty = property;
            }

            public List<WrapField> GetProperties()
            {
                if (mProperty != null)
                {
                    return mProperty.Children.Select(node => new WrapField(node)).ToList();
                }
                if (mController != null)
                {
                    List<WrapField> fieldInfos = new List<WrapField>();
                    foreach (var node in mController)
                    {
                        if(node)
                            fieldInfos.AddRange(DirectGetProperties(node));
                    }
                    return fieldInfos;
                }
                return new List<WrapField>();
            }

            private List<WrapField> DirectGetProperties(MonoBehaviour behaviour)
            {
                var type = behaviour.GetType();
                var flag = BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
                var fields = GetAllFields(type,flag);
                return fields.Select(node => new WrapField(node, behaviour)).ToList();
            }
        
            public static List<FieldInfo> GetAllFields(Type type, BindingFlags flags)
            {
                List<FieldInfo> fields = new List<FieldInfo>();

                if (type.BaseType != null)
                {
                    fields.AddRange(GetAllFields(type.BaseType, flags));
                }

                fields.AddRange(type.GetFields(flags));
                return fields;
            }
        }

        public static void Bind(GameObject go, InspectorProperty property)
        {
            var wrapObj = new WrapObj(go, property);
            if (wrapObj == null)
            {
                return;
            }
            var properties = wrapObj.GetProperties();
            for (int i = 0; i < properties.Count; i++)
            {
                var child = properties[i];
                var type = child.Type;
                var bindAttr = child.GetAttribute<BindFieldAttribute>();
                if (bindAttr == null)
                    continue;
                if (string.IsNullOrEmpty(bindAttr.Path))
                {
                    bindAttr.Path = child.Name;
                }
                var parent = go.transform;
                if (!bindAttr.IsAsset)
                {
                    BindObject(go, bindAttr, parent, type, child);
                }
                else
                {
                    //性能改进.使用FindAssets可以利用Unity内部的缓存系统快速定位到大致匹配的文件上.
                    var allPaths = new List<string>(AssetDatabase
                        .FindAssets("t:" + type.Name + " " + Path.GetFileName(bindAttr.Path), new[] { "Assets/Res" })
                        .Select(AssetDatabase.GUIDToAssetPath));
                    //这里增加一个句号的原因是防止如下情况
                    //比如我在查找对象叫star
                    //那么我可能会查找到star_1,star_2
                    //增加句号可以标志着我们文件的结尾就是star.xxx
                    var searchPath = bindAttr.Path + ".";
                    var list = allPaths.FindAll(v => v.IndexOf(searchPath, StringComparison.OrdinalIgnoreCase) != -1);
                    if (list.Count == 1)
                    {
                        type = child.Type;
                        var result = AssetDatabase.LoadAssetAtPath(list[0], type);
                        if (result != null)
                        {
                            child.Value = result;
                        }
                    }
                    else if (list.Count == 0)
                    {
                        Log.Error($"无法定位资源 {bindAttr.Path}");
                    }
                    else
                    {
                        Log.Error($"定位资源大于1个 {bindAttr.Path}");
                    }
                }
            }
        }

        private static void BindObject(GameObject go, BindFieldAttribute bindAttr, Transform parent, Type type,
            WrapField value)
        {
            var targetName = bindAttr.Path;

            if (bindAttr.Path != null)
            {
                if (bindAttr.Path.Contains("/"))
                {
                    var names = new List<string>(bindAttr.Path.Split('/'));
                    targetName = names[^1];
                    names.RemoveAt(names.Count - 1);
                    parent = FindNode(go.transform, string.Join("/", names), bindAttr.SearchInParent);
                    if (parent == null)
                    {
                        Debug.LogErrorFormat("找不到绑定父物体：{0} {1}", type, bindAttr.Path);
                        return;
                    }
                }
                else if (bindAttr.Path.Equals("_This", StringComparison.OrdinalIgnoreCase))
                {
                    SetObjectToProperty(bindAttr, type, value, parent);
                    return;
                }
            }

            Type elementType = null;
            if (type.IsArray)
            {
                elementType = type.GetElementType();
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                elementType = type.GetGenericArguments()[0];
            }

            if (elementType != null)
            {
                // 处理集合类型
                List<Transform> array = new List<Transform>();
                array.AddRange(FindNodeList(parent.transform, targetName, new List<Transform>(), bindAttr.SearchInParent));

                List<object> list = new List<object>();
                foreach (var transform in array)
                {
                    if (elementType == typeof(GameObject))
                    {
                        list.Add(transform.gameObject);
                    }
                    else
                    {
                        var component = transform.GetComponent(elementType);
                        if (component != null)
                        {
                            list.Add(component);
                        }
                    }
                }

                value.SetCollection(list, elementType);
            }
            else
            {
                // 处理单对象类型
                Transform target = FindNode(parent.transform, targetName, bindAttr.SearchInParent);
                if (target == null)
                {
                    Debug.LogErrorFormat("找不到绑定物体：{0} {1}", type, bindAttr.Path);
                    value.Value = null;
                    return;
                }

                SetObjectToProperty(bindAttr, type, value, target);
            }
        }

        private static void SetObjectToProperty(BindFieldAttribute bindAttr, Type type, WrapField value, Transform target)
        {
            if (type == typeof(GameObject))
            {
                value.Value = target.gameObject;
            }
            else
            {
                var comp = target.GetComponent(type);
                if (comp == null)
                {
                    Debug.LogErrorFormat("找不到物体组件：{0} {1}", type, bindAttr.Path);
                }

                value.Value = comp;
            }
        }

        private static Transform FindNode(Transform parent, string name, bool searchInParent = false)
        {
            if (name.Contains("/"))
            {
                var names = name.Split('/');
                name = names[names.Length - 1];
                for (var i = 0; i < names.Length; i++)
                {
                    parent = FindNode(parent, names[i], searchInParent);
                    if (parent == null)
                    {
                        return null;
                    }
                }
            }

            // 如果 SearchInParent 为 true，则只在父节点中查找
            if (searchInParent)
            {
                var currentParent = parent;
                while (currentParent != null)
                {
                    if (currentParent.name == name)
                    {
                        return currentParent;
                    }
                    currentParent = currentParent.parent;
                }
            }
            else
            {
                // 否则在子节点中查找
                var list = new List<Transform>
                {
                    parent
                };

                while (list.Count > 0)
                {
                    var node = list[0];
                    list.RemoveAt(0);
                    if (node.name == name)
                    {
                        return node;
                    }
                    for (var i = 0; i < node.childCount; i++)
                    {
                        list.Add(node.GetChild(i));
                    }
                }
            }

            return null;
        }

        private static List<Transform> FindNodeList(Transform transform, string name, List<Transform> list, bool searchInParent = false)
        {
            if (searchInParent)
            {
                // 如果 SearchInParent 为 true，则只在父节点中查找
                var currentParent = transform;
                while (currentParent != null)
                {
                    if (currentParent.name == name)
                    {
                        list.Add(currentParent);
                    }
                    currentParent = currentParent.parent;
                }
            }
            else
            {
                // 否则在子节点中查找
                var queue = new List<Transform>
                {
                    transform
                };
                while (queue.Count > 0)
                {
                    var node = queue[0];
                    queue.RemoveAt(0);
                    if (node.name == name)
                    {
                        list.Add(node);
                    }
                    for (var i = 0; i < node.childCount; i++)
                    {
                        queue.Add(node.GetChild(i));
                    }
                }
            }

            return list;
        }
    }
#endif
}