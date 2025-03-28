﻿using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ScratchCardAsset.Editor
{
    public class ScratchCardMenu : UnityEditor.Editor
    {
        private static string ScratchCardPrefabGUID = "bfd21db4576fb4dac871b93fdc37924b";

        private static ScratchCardManager CreateScratchCard()
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(ScratchCardPrefabGUID);
            var asset = AssetDatabase.LoadAssetAtPath<ScratchCardManager>(prefabPath);
            var scratchCardManager = PrefabUtility.InstantiatePrefab(asset) as ScratchCardManager;
            if (scratchCardManager != null)
            {
                return scratchCardManager;
            }
            return null;
        }

        private static void MarkAsDirty(Component component)
        {
            Selection.activeObject = component.gameObject;
            EditorUtility.SetDirty(component);
            EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
        }

        [MenuItem("GameObject/Scratch Card/Image", false, 34)]
        private static void CreateImageRendererScratchCard()
        {
            var scratchCardManager = CreateScratchCard();
            if (scratchCardManager != null)
            {
                scratchCardManager.TrySelectCard();
                MarkAsDirty(scratchCardManager);
            }
        }
    }
}