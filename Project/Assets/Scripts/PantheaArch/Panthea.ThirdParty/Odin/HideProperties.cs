using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace EdgeStudio.Odin
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class HideProperties : Attribute
    {
        public string[] PropertiesNames;

        public HideProperties(params string[] propertiesNames)
        {
            PropertiesNames = propertiesNames;
        }
    }

    [DrawerPriority(int.MaxValue)]
    public class HidePropertiesDrawer<T> : OdinAttributeDrawer<HideProperties,T> where T : class
    {
        private HashSet<string> hiddenProperties;

        protected override void Initialize()
        {
            // 初始化时将需要隐藏的属性名称存储到HashSet中以提高查找效率
            hiddenProperties = new HashSet<string>(Attribute.PropertiesNames);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            foreach (var property in this.Property.Children)
            {
                if (hiddenProperties.Contains(property.Name))
                {
                    continue;
                }
                property.Draw();
            }
        }
    }
}