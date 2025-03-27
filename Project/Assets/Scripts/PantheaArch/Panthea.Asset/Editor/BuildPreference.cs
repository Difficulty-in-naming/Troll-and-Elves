using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Panthea.Asset
{
    [Serializable]
    public class SetInfo
    {
        public string Path;
        public bool Ignore = false;
        public bool Include = true;
        public BuildObject.BundlePackingMode PackingMode;
        public CompressionType CompressionType = CompressionType.Lz4;
        public Object Obj;
        public bool IsOverride;
        public string Address;
        public SetInfo(Object o)
        {
            Path = AssetDatabase.GetAssetPath(o);
            Obj = o;
        }
        public SetInfo(){}
        private static readonly SetInfo DefaultValue = new SetInfo();
        public bool IsDefaultValue()
        {
            return Ignore == DefaultValue.Ignore && 
                   Include == DefaultValue.Include && 
                   PackingMode == DefaultValue.PackingMode && 
                   CompressionType == DefaultValue.CompressionType &&
                   IsOverride == DefaultValue.IsOverride && 
                   Address == DefaultValue.Address;
        }
    }
    [Serializable]
    public class InfoList
    {
        public List<SetInfo> List = new List<SetInfo>();
    }
    public class BuildPreference : ScriptableObject
    {
        public string OutputPath = Application.streamingAssetsPath;
        public bool AppendHash = true;
        public List<BuildObject> Groups = new List<BuildObject>();
        private Dictionary<Object,SetInfo> mFileAttribute = new Dictionary<Object, SetInfo>();
        private Dictionary<string,SetInfo> mFilePathMapping = new Dictionary<string, SetInfo>();
        
        [FormerlySerializedAs("list")] [SerializeField] private InfoList fileList = new InfoList();
        private static BuildPreference mInstance;
        public static BuildPreference Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = AssetDatabase.LoadAssetAtPath<BuildPreference>("Assets/XAssetsData/Preference.asset");
                    if(mInstance != null)
                        Init();
                }
                if (mInstance == null)
                {
                    mInstance = CreateInstance<BuildPreference>();
                    Directory.CreateDirectory(Application.dataPath + "/XAssetsData/");
                    AssetDatabase.CreateAsset(mInstance, "Assets/XAssetsData/Preference.asset");
                    
                    Init();
                }
                return mInstance;
            }
        }

        private static void Init()
        {
            mInstance.mFileAttribute = mInstance.fileList.List.ToDictionary(t1 => t1.Obj, t2 => t2);
            mInstance.mFilePathMapping = new Dictionary<string, SetInfo>(mInstance.fileList.List.ToDictionary(t1 => t1.Path, t2 => t2), StringComparer.OrdinalIgnoreCase);
        }

        public BuildObject AddGroup(BuildObject obj)
        {
            Groups.Add(obj);
            var path = obj.name.Replace("/", "-").Replace("\\","-");
            Directory.CreateDirectory(Application.dataPath + "/XAssetsData/Files");
            AssetDatabase.CreateAsset(obj, "Assets/XAssetsData/Files/" + path + ".asset");
            return obj;
        }

        public void RemoveGroup(BuildObject obj)
        {
            if (obj == null)
                return;
            Groups.Remove(obj);
            try
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(obj));
            }
            catch(System.Exception e)
            {
                Debug.LogError("删除[Group]" + obj.name + "失败" + "\n" + e);
            }
        }

        public BuildObject FindGroup(string path)
        {
            var findOut = Groups.Find(t1 => t1.RealAddress == path);
            return findOut;
        }
        public SetInfo GetInfo(Object o)
        {
            if (mFileAttribute.TryGetValue(o, out var value))
            {
                return value;
            }
            else
            {
                var info = new SetInfo(o);
                return info;
            }
        }
        public SetInfo GetInfo(string str)
        {
            if (mFilePathMapping.TryGetValue("Assets/Res/" + str, out var value))
            {
                return value;
            }
            return null;
        }
        public InfoList GetAllInfo()
        {
            return fileList;
        }
        public void WriteInfo(SetInfo info)
        {
            if (!mFileAttribute.TryGetValue(info.Obj, out _))
            {
                if (info.IsDefaultValue())
                    return;
                mFileAttribute.Add(info.Obj, info);
                fileList.List.Add(info);
                mFilePathMapping.Add(info.Path, info);
            }
            else
            {
                if (info.IsDefaultValue())
                {
                    mFileAttribute.Remove(info.Obj);
                    fileList.List.Remove(info);
                    mFilePathMapping.Remove(info.Path);
                }
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }
}
