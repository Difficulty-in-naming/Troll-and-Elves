using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Panthea.Asset
{
    public class BuildObject : ScriptableObject
    {
        public enum BundlePackingMode
        {
            PackTogether,
            PackSeparately,
        }

        [Serializable]
        public class BuildFileKeyValue
        {
            public string Key;
            public string GUID;
            public Object Object;
            public string Address;
            
            public BuildFileKeyValue(string key, string guid,string address = null)
            {
                Key = key;
                GUID = guid;
                if (!string.IsNullOrEmpty(address))
                    Address = address;
            }
        }

        public CompressionType CompressionMode = CompressionType.Lzma;
        public BundlePackingMode PackingMode = BundlePackingMode.PackTogether;
        public List<BuildFileKeyValue> Files = new List<BuildFileKeyValue>();
        public bool Include = true;
        public string RealAddress;
    }

    [CustomEditor(typeof(BuildObject))]
    [CanEditMultipleObjects]
    public class BuildObject_EDITOR: UnityEditor.Editor
    {
        public void OnEnable()
        {
            var t = (BuildObject) target;
            foreach (var node in t.Files) node.Object = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(node.GUID), typeof(Object));
        }

        public void OnDisable()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}