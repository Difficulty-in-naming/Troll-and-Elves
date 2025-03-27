/********************************
  该脚本是自动生成的请勿手动修改
*********************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using EdgeStudio.DataStruct;
using Panthea.Asset;

namespace EdgeStudio.Config
{
    [UnityEngine.Scripting.Preserve]
    public partial class GlobalProperty : IConfig
    {
        private string mId;
        private DynamicParameters mValue;
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get => mId; set => mId = value; }

        /// <summary>
        /// 参数
        /// </summary>
        public DynamicParameters Value { get => mValue; set => mValue = value; }

        public static GlobalProperty Read(string id)
        {
            return ConfigAssetManager<GlobalProperty>.Read(id);
        }

        public static Dictionary<string, GlobalProperty> ReadDict()
        {
            return ConfigAssetManager<GlobalProperty>.ReadstringDict();
        }

        /// <summary>
        /// 警告:该方法会重新开辟一块内存用于存放Property列表.
        /// </summary>
        public static List<GlobalProperty> ReadList()
        {
            return ConfigAssetManager<GlobalProperty>.ReadList();
        }

        public static async UniTask Load()
        {
            ConfigAssetManager<GlobalProperty>.Load((await AssetsKit.Inst.Load<TextAsset>("Config/Global".ToLower())).text);
        }
    }
}