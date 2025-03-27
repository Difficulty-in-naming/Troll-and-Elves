/********************************
  该脚本是自动生成的请勿手动修改
*********************************/
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Panthea.Asset;

namespace EdgeStudio.Config
{
    [UnityEngine.Scripting.Preserve]
    public partial class LocalizationProperty : IConfig
    {
        private string mId;
        private string mChinese;
        /// <summary>
        /// Id
        /// 
        /// </summary>
        public string Id { get => mId; set => mId = value; }

        /// <summary>
        /// 中文
        /// </summary>
        public string Chinese { get => mChinese; set => mChinese = value; }

        public static string Read(string id)
        {
            if (string.IsNullOrEmpty(id))
                return "";
            var property = Get(id);
            if (property == null)
                return id;
            var language = Language.CurrentLanguage;
            if (language.Equals(Language.Chinese))
            {
                return property.Chinese ?? property.Id;
            }

            /*           else if (language.Equals(Language.Chinese_tw))
            {
                return property.Chinese_tw ?? property.Id;
            }
            else if (language.Equals(Language.English))
            {
                return property.English ?? property.Id;
            }*/
            return property.Chinese ?? property.Id;
        }

        public static LocalizationProperty Get(string id)
        {
            return ConfigAssetManager<LocalizationProperty>.Read(id);
        }

        public static Dictionary<string, LocalizationProperty> ReadDict()
        {
            return ConfigAssetManager<LocalizationProperty>.ReadstringDict();
        }

        /// <summary>
        /// 警告:该方法会重新开辟一块内存用于存放Property列表.
        /// </summary>
        public static List<LocalizationProperty> ReadList()
        {
            return ConfigAssetManager<LocalizationProperty>.ReadList();
        }

        public static async UniTask Load()
        {
            ConfigAssetManager<LocalizationProperty>.Load((await AssetsKit.Inst.Load<TextAsset>("Config/Localization".ToLower())).text);
        }
    }
}