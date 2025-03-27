using UnityEngine;
using UnityEngine.UI;

namespace ScratchCardAsset.Tools
{
    /// <summary>
    /// A class for migration from version 1.x to 2.x
    /// </summary>
    public class MigrationHelper
    {
        private Object migratedObject;
        
        public void StartMigrate(ScratchCardManager scratchCardManager)
        {
            if (scratchCardManager == null)
                return;

            var result = false;
            
            if (scratchCardManager.CanvasRendererCard == null)
            {
                var field = scratchCardManager.GetType().GetField("ImageCard");
                var imageCardValue = field.GetValue(scratchCardManager);
                if (imageCardValue != null)
                {
                    var imageCardGameObject = (GameObject)imageCardValue;
                    if (imageCardGameObject != null)
                    {
                        if (imageCardGameObject.TryGetComponent<Image>(out var image))
                        {
                            scratchCardManager.CanvasRendererCard = image;
                            field.SetValue(scratchCardManager, null);
                            result = true;
                        }
                    }
                }
            }

            if (result)
            {
                migratedObject = scratchCardManager;
                Debug.Log($"The migration for {scratchCardManager} was successful!", scratchCardManager);
            }
        }

        public void FinishMigrate()
        {
            if (migratedObject != null)
            {
                MarkAsDirty(migratedObject);
                migratedObject = null;
            }
        }

        private void MarkAsDirty(Object unityObject)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(unityObject);
                var component = unityObject as Component;
                if (component != null)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                }
            }
#endif
        }
    }
}