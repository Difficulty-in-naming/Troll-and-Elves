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
    public partial class ItemProperty : IConfig
    {
        private string mId;
        private Vector2Int mOccupiedOffsets;
        private string mTexture;
        private ItemType mType;
        private int mMaxStackNum;
        private int mPrice;
        private bool mCanBeSold;
        private WeaponArgs mWeaponArgs;
        private AttachmentArgs mAttachmentArgs;
        private EquipmentArgs mEquipmentArgs;
        private ConsumableArgs mConsumableArgs;
        /// <summary>
        /// Id
        /// 
        /// </summary>
        public string Id { get => mId; set => mId = value; }

        /// <summary>
        /// 占用格子
        /// </summary>
        public Vector2Int OccupiedOffsets { get => mOccupiedOffsets; set => mOccupiedOffsets = value; }

        /// <summary>
        /// 贴图路径
        /// </summary>
        public string Texture { get => mTexture; set => mTexture = value; }

        /// <summary>
        /// 物品类型
        /// </summary>
        public ItemType Type { get => mType; set => mType = value; }

        /// <summary>
        /// 最大堆叠数量
        /// </summary>
        public int MaxStackNum { get => mMaxStackNum; set => mMaxStackNum = value; }

        /// <summary>
        /// 价格
        /// </summary>
        public int Price { get => mPrice; set => mPrice = value; }

        /// <summary>
        /// 是否可出售
        /// </summary>
        public bool CanBeSold { get => mCanBeSold; set => mCanBeSold = value; }

        /// <summary>
        /// 武器参数
        /// </summary>
        public WeaponArgs WeaponArgs { get => mWeaponArgs; set => mWeaponArgs = value; }

        /// <summary>
        /// 改装部件参数
        /// </summary>
        public AttachmentArgs AttachmentArgs { get => mAttachmentArgs; set => mAttachmentArgs = value; }

        /// <summary>
        /// 装备参数
        /// </summary>
        public EquipmentArgs EquipmentArgs { get => mEquipmentArgs; set => mEquipmentArgs = value; }

        /// <summary>
        /// 其他参数
        /// </summary>
        public ConsumableArgs ConsumableArgs { get => mConsumableArgs; set => mConsumableArgs = value; }

        public static ItemProperty Read(string id)
        {
            return ConfigAssetManager<ItemProperty>.Read(id);
        }

        public static Dictionary<string, ItemProperty> ReadDict()
        {
            return ConfigAssetManager<ItemProperty>.ReadstringDict();
        }

        /// <summary>
        /// 警告:该方法会重新开辟一块内存用于存放Property列表.
        /// </summary>
        public static List<ItemProperty> ReadList()
        {
            return ConfigAssetManager<ItemProperty>.ReadList();
        }

        public static async UniTask Load()
        {
            ConfigAssetManager<ItemProperty>.Load((await AssetsKit.Inst.Load<TextAsset>("Config/Item".ToLower())).text);
        }
    }
}