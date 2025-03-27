using System.IO;
using UnityEditor;
using UnityEngine;

public static class OnlyEditorSavve
{
    [MenuItem("Tools/存档/打开存档文件夹",priority = 20)]
    public static void OpenSaveFolder() => Application.OpenURL("file:///" + Application.persistentDataPath);
        
    [MenuItem("Tools/存档/清除本地存档",priority = 31)]
    public static void ClearLocalSave()
    {
        Directory.Delete(Application.persistentDataPath, true);
    }
        
    /*[MenuItem("Tools/存档/清除云端存档",priority = 32)]
        public static async void ClearRemoteSave()
        {
            if (EditorUtility.DisplayDialog("危险操作", $"你正在请求删除Id为:{"EDITOR_" + SystemInfo.deviceUniqueIdentifier}的云端存档数据,确认ID无误之后点击确认", "确认", "取消"))
            {
                if (Application.isPlaying)
                {
                    await NetworkManager.Inst.UploadSaves("", 0);
                }
                else
                {
                    var login = await NetworkManager.Inst.Login("EDITOR_" + SystemInfo.deviceUniqueIdentifier);
                    GameSettings.PlayerId = login.PlayerId;
                    await NetworkManager.Inst.UploadSaves("", 0);
                    GameSettings.PlayerId = 0;
                }
            }
        }
        
        [MenuItem("Tools/存档/清除全部存档",priority = 33)]
        public static void ClearAllSave()
        {
            ClearLocalSave();
            ClearRemoteSave();
        }*/
}