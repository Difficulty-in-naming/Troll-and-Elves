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
    public partial class ShopProperty : IConfig
    {
        private string mId;
        private ItemQuantity[] mContent;
        /// <summary>
        /// Id
        /// 
        /// </summary>
        public string Id { get => mId; set => mId = value; }

        /// <summary>
        /// 出售内容
        /// </summary>
        public ItemQuantity[] Content { get => mContent; set => mContent = value; }

        public static ShopProperty Read(string id)
        {
            return ConfigAssetManager<ShopProperty>.Read(id);
        }

        public static Dictionary<string, ShopProperty> ReadDict()
        {
            return ConfigAssetManager<ShopProperty>.ReadstringDict();
        }

        /// <summary>
        /// 警告:该方法会重新开辟一块内存用于存放Property列表.
        /// </summary>
        public static List<ShopProperty> ReadList()
        {
            return ConfigAssetManager<ShopProperty>.ReadList();
        }

        public static async UniTask Load()
        {
            ConfigAssetManager<ShopProperty>.Load((await AssetsKit.Inst.Load<TextAsset>("Config/Shop".ToLower())).text);
        }
    }
}