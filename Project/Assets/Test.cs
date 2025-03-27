using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform Target;
    [Button]
    public void aaa()
    {
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            GameObject prefabRoot = prefabStage.prefabContentsRoot;
            EditorUtility.SetDirty(prefabRoot);
            Debug.Log("=====将预设标记为脏让Unity触发Auto Save: " + prefabStage.assetPath);
        }
    }
}
