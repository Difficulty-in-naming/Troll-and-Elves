#if USE_MEMORYPACK
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MemoryPack;
using Newtonsoft.Json;
// using Newtonsoft.Json;
using Panthea.Asset.Runtime;
using UnityEditor;
using UnityEngine;

namespace Panthea.Asset.Editor
{
    public class ConvertJsonConfigToBinaryConfig : AResPipeline
    {
        public override Task Do()
        {
            var assetsManager = new EDITOR_AssetsManager();
            AssetsKit.Inst = assetsManager;
            var configType = typeof(IConfig);
            foreach (var type in typeof(IAPProperty).Assembly.GetTypes())
            {
                if (configType.IsAssignableFrom(type))
                {
                    var idType = type.GetProperty("Id")?.PropertyType;
                    if (idType == null)
                        continue;
                    var targetType = typeof(Dictionary<,>).MakeGenericType(idType == typeof(int) ? typeof(int) : typeof(string), type);
                    var fileName = type.Name.Replace("Property", "");
                    var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Res/Config/" + fileName + ".json");
                    if (!textAsset)
                    {
                        Debug.LogError(type + "未生成二进制配置表因为找不到文件");
                        continue;
                    }
                    var property = JsonConvert.DeserializeObject(textAsset.text,targetType,EdgeStudioJsonConverter.JsonConverters);
                    var binary = MemoryPackSerializer.Serialize(targetType, property);
                    File.WriteAllBytes(Application.dataPath + "/Res/Config/" + fileName.ToLower() + ".bytes",binary);
                }
            }
            AssetDatabase.Refresh();
            return Task.CompletedTask;
        }

        public override Task Rollback()
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
            return base.Rollback();
        }
    }
}
#endif