using System.IO;
using System.Threading.Tasks;
using Panthea.Asset.Define;
using Panthea.Common;
using Panthea.Utils;
using UnityEditor;
using UnityEngine;

namespace Panthea.Asset.OneHeart
{
    public class CompressFont : AResPipeline
    {
        private string[] files;
        public override Task Do()
        {
            var fontToolsPath = Path.Combine(PathUtils.RootDir, "FontTools");
            var fontPath = Path.Combine(PathUtils.AssetDir, "Res/Fonts/JinNanMaiYuanTi-Regular.ttf");
            if (Directory.Exists(Application.dataPath + "/Res/Config"))
            {
                files = Directory.GetFiles(Application.dataPath + "/Res/Config","*.json");
                foreach (var node in files)
                {
                    File.Copy(node, fontToolsPath + "/" + Path.GetFileNameWithoutExtension(node) + ".txt", true);
                }
            }
        
            EditorUtility.DisplayProgressBar("压缩字体", "正在压缩字体文件 JinNanMaiYuanTi-Regular.ttf", 0);
            ProcessUtil.Execute($"-c {fontToolsPath} -s {fontPath} {fontToolsPath}/compress.ttf", $"{fontToolsPath}/fontsubset.exe", "");
            File.Copy(fontPath, Path.Combine(fontToolsPath, "font_Backup.ttf"), true);
            File.Copy($"{fontToolsPath}/compress.ttf", $"{fontPath}", true);
            AssetDatabase.Refresh(ImportAssetOptions.Default);
            EditorUtility.ClearProgressBar();
            return Task.CompletedTask;
        }

        public override Task Rollback()
        {
            OnComplete();
            return base.Rollback();
        }

        public override Task OnComplete()
        {
            var fontToolsPath = Path.Combine(PathUtils.RootDir, "FontTools");
            var fontPath = Path.Combine(PathUtils.AssetDir, "Res/Fonts/JinNanMaiYuanTi-Regular.ttf");
            File.Copy(Path.Combine(fontToolsPath, "font_Backup.ttf"), fontPath, true);
            if (files != null)
            {
                foreach (var node in files)
                {
                    File.Delete(fontToolsPath + "/" + Path.GetFileNameWithoutExtension(node) + ".txt");
                }
            }

            return base.OnComplete();
        }
    }
}
