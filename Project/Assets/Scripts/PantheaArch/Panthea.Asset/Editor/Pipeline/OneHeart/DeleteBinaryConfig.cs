using System.IO;
using System.Threading.Tasks;
using Panthea.Asset.Define;
using UnityEditor;
using UnityEngine;

namespace Panthea.Asset.OneHeart
{
    public class DeleteBinaryConfig : AResPipeline
    {
        public override Task Do()
        {
            var files = Directory.GetFiles(Application.dataPath + "/Res/Config/", "*.*");
            foreach (var file in files)
            {
                if (file.EndsWith(".bytes") || file.EndsWith(".bytes.meta"))
                {
                    File.Delete(file);
                }
            }
            AssetDatabase.Refresh();
            //重命名Hotfix文件
            foreach (var node in Directory.GetFiles(BuildPreference.Instance.OutputPath,"hotfix_*.bundle",SearchOption.TopDirectoryOnly))
            {
                File.Copy(node, BuildPreference.Instance.OutputPath + "/hotfix.bundle", true);
            }
            return Task.CompletedTask;
        }
    }
}