using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class SpriteConverter : Editor
{
    [MenuItem("Assets/Edge Studio/Convert Sprite To Texture2D", true)]
    private static bool ValidateConvert()
    {
        // 仅当选中Sprite时显示菜单项
        foreach (Object obj in Selection.objects)
        {
            if (obj is Sprite) return true;
        }
        return false;
    }

    [MenuItem("Assets/Edge Studio/Convert Sprite To Texture2D")]
    private static void ConvertSelectedSprites()
    {
        List<string> successPaths = new List<string>();
        List<string> failPaths = new List<string>();

        try
        {
            EditorUtility.DisplayProgressBar("Processing", "Converting sprites...", 0);

            // 获取所有选中的Sprite
            Object[] selectedObjects = Selection.objects;
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                Sprite sprite = selectedObjects[i] as Sprite;
                if (sprite == null) continue;

                EditorUtility.DisplayProgressBar("Processing", 
                    $"Converting {sprite.name} ({i+1}/{selectedObjects.Length})", 
                    (float)i / selectedObjects.Length);

                // 执行转换
                Texture2D newTexture = ConvertSingleSprite(sprite);
                if (newTexture == null)
                {
                    failPaths.Add(AssetDatabase.GetAssetPath(sprite));
                    continue;
                }

                // 保存文件
                string path = SaveTexture(newTexture, sprite);
                if (!string.IsNullOrEmpty(path))
                {
                    successPaths.Add(path);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();

            // 显示处理结果
            ShowResultReport(successPaths, failPaths);
        }
    }

    private static Texture2D ConvertSingleSprite(Sprite sprite)
    {
        if (sprite == null) return null;

        Texture2D originalTexture = sprite.texture;
        Rect spriteRect = sprite.rect;

        if (originalTexture == null || !originalTexture.isReadable)
        {
            return null;
        }

        try
        {
            Texture2D newTexture = new Texture2D(
                (int)spriteRect.width,
                (int)spriteRect.height,
                originalTexture.format,
                originalTexture.mipmapCount > 1
            );

            Color[] pixels = originalTexture.GetPixels(
                (int)spriteRect.x,
                (int)spriteRect.y,
                (int)spriteRect.width,
                (int)spriteRect.height
            );

            newTexture.SetPixels(pixels);
            newTexture.Apply();
            return newTexture;
        }
        catch
        {
            return null;
        }
    }

    private static string SaveTexture(Texture2D texture, Sprite originalSprite)
    {
        string originalPath = AssetDatabase.GetAssetPath(originalSprite);
        string directory = Path.GetDirectoryName(originalPath);
        string fileName = Path.GetFileNameWithoutExtension(originalPath);
        string newFileName = $"{originalSprite.name}.png";
        string fullPath = Path.Combine(directory, newFileName);

        try
        {
            // 确保目录存在
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 保存为PNG文件
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);

            // 销毁临时纹理
            Object.DestroyImmediate(texture);

            return fullPath;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
            return null;
        }
    }

    private static void ShowResultReport(List<string> success, List<string> fails)
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("Conversion Report:");
        report.AppendLine($"Success: {success.Count}");
        report.AppendLine($"Failed: {fails.Count}");

        if (fails.Count > 0)
        {
            report.AppendLine("\nFailed Assets:");
            foreach (string path in fails)
            {
                report.AppendLine($"- {path}");
            }
        }

        EditorUtility.DisplayDialog("Conversion Complete", report.ToString(), "OK");
    }
}