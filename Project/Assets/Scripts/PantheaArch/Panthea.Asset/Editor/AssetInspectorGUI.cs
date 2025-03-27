using System;
using System.IO;
using Panthea.Utils;
using UnityEditor;
using UnityEngine;

namespace Panthea.Asset
{
    [InitializeOnLoad]
    public class AssetInspectorGUI
    {
        static AssetInspectorGUI()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }

        private static void OnPostHeaderGUI(UnityEditor.Editor editor)
        {
             // 检查是否在指定路径下
            if (!AssetDatabase.GetAssetPath(editor.target).StartsWith("Assets/Res/", StringComparison.OrdinalIgnoreCase))
                return;

            // 获取所有选中的对象
            var targets = editor.targets;

            // 只对第一个对象进行 GUI 绘制
            var firstTarget = targets[0];
            var importer = firstTarget as AssetImporter;
            var info = BuildPreference.Instance.GetInfo(importer ? AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(importer.assetPath) : firstTarget);

            var changed = false;

            EditorGUILayout.BeginHorizontal();
            info.Ignore = EditorGUILayout.Toggle(info.Ignore, GUILayout.Width(16));
            TightLabel("忽略该文件");
            if (!(firstTarget is DefaultAsset))
                GUI.enabled = info.IsOverride;
            info.Include = EditorGUILayout.Toggle(info.Include, GUILayout.Width(16));
            TightLabel("包含该文件");
            if (!(firstTarget is DefaultAsset))
                GUI.enabled = true;
            if (!(firstTarget is DefaultAsset))
            {
                info.IsOverride = EditorGUILayout.Toggle(info.IsOverride, GUILayout.Width(16));
                TightLabel("覆盖文件夹设置");
            }

            EditorGUILayout.EndHorizontal();
            if (!(firstTarget is DefaultAsset))
                GUI.enabled = true;
            changed |= GUI.changed;
            if (!info.Ignore)
            {
                if (!(firstTarget is DefaultAsset) && targets.Length == 1)
                {
                    EditorGUILayout.BeginHorizontal();
                    TightLabel("寻址");
                    info.Address = EditorGUILayout.TextField(info.Address);
                    if (GUILayout.Button("简化"))
                    {
                        info.Address = Path.GetFileNameWithoutExtension(info.Path).ToLower();
                        changed = true;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();

                if (firstTarget is DefaultAsset)
                {
                    TightLabel("拆分打包");
                    info.PackingMode = (BuildObject.BundlePackingMode)EditorGUILayout.EnumPopup(info.PackingMode);
                }

                TightLabel("压缩类型");
                info.CompressionType = (CompressionType)EditorGUILayout.EnumPopup(info.CompressionType);

                changed |= GUI.changed;
                EditorGUILayout.EndHorizontal();
            }
            
            // 如果 GUI 发生更改，将设置应用到所有选中的对象
            if (changed)
            {
                foreach (var target in targets)
                {
                    var targetImporter = target as AssetImporter;
                    var targetAssetPath = targetImporter ? targetImporter.assetPath : AssetDatabase.GetAssetPath(target);
                    var targetAssetObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetAssetPath);
                    var targetInfo = BuildPreference.Instance.GetInfo(targetAssetObject);

                    // 如果是文件夹，使用 BuildGroup 的参数
                    if (AssetDatabase.IsValidFolder(targetAssetPath))
                    {
                        // 获取文件夹的完整路径（去掉 "Assets/Res/" 前缀）
                        string folderPath = targetAssetPath.Substring("Assets/Res/".Length).ToLower();
                        var targetBuildGroup = BuildPreference.Instance.Groups.Find(group => group.RealAddress.ToLower() == folderPath);
                        if (targetBuildGroup != null)
                        {
                            targetBuildGroup.PackingMode = targetInfo.PackingMode;
                            targetBuildGroup.CompressionMode = targetInfo.CompressionType;
                            targetBuildGroup.Include = targetInfo.Include;
                            EditorUtility.SetDirty(targetBuildGroup);
                        }
                    }

                    // 将第一个对象的设置复制到其他对象
                    targetInfo.Ignore = info.Ignore;
                    targetInfo.Include = info.Include;
                    targetInfo.IsOverride = info.IsOverride;
                    targetInfo.Address = info.Address;
                    targetInfo.PackingMode = info.PackingMode;
                    targetInfo.CompressionType = info.CompressionType;

                    BuildPreference.Instance.WriteInfo(targetInfo);
                }
            }
        }

        private static void TightLabel(string labelStr)
        {
            GUIContent label = new GUIContent(labelStr);
            EditorGUILayout.LabelField(label, GUILayout.Width(GUI.skin.label.CalcSize(label).x));
        }
    }
}
