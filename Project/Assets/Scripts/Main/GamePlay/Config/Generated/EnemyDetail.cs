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
    public partial class EnemyDetailProperty : IConfig
    {
        private string mId;
        private string mName;
        private string mWeapon;
        private string mArmor;
        private int mHp;
        private int mDefense;
        private int mAttack;
        private string mSpriteLibrary;
        /// <summary>
        /// Id
        /// 
        /// </summary>
        public string Id { get => mId; set => mId = value; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get => mName; set => mName = value; }

        /// <summary>
        /// 携带武器
        /// </summary>
        public string Weapon { get => mWeapon; set => mWeapon = value; }

        /// <summary>
        /// 携带装甲
        /// </summary>
        public string Armor { get => mArmor; set => mArmor = value; }

        /// <summary>
        /// 基础生命值
        /// </summary>
        public int Hp { get => mHp; set => mHp = value; }

        /// <summary>
        /// 基础防御力
        /// </summary>
        public int Defense { get => mDefense; set => mDefense = value; }

        /// <summary>
        /// 基础攻击力
        /// </summary>
        public int Attack { get => mAttack; set => mAttack = value; }

        /// <summary>
        /// SpriteLibrary
        /// </summary>
        public string SpriteLibrary { get => mSpriteLibrary; set => mSpriteLibrary = value; }

        public static EnemyDetailProperty Read(string id)
        {
            return ConfigAssetManager<EnemyDetailProperty>.Read(id);
        }

        public static Dictionary<string, EnemyDetailProperty> ReadDict()
        {
            return ConfigAssetManager<EnemyDetailProperty>.ReadstringDict();
        }

        /// <summary>
        /// 警告:该方法会重新开辟一块内存用于存放Property列表.
        /// </summary>
        public static List<EnemyDetailProperty> ReadList()
        {
            return ConfigAssetManager<EnemyDetailProperty>.ReadList();
        }

        public static async UniTask Load()
        {
            ConfigAssetManager<EnemyDetailProperty>.Load((await AssetsKit.Inst.Load<TextAsset>("Config/EnemyDetail".ToLower())).text);
        }
    }
}