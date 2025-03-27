using System;

namespace EdgeStudio.Config
{
    [Flags]
    public enum ItemType
    {
        Weapon = 1 << 0,//武器
        Head = 1 << 1,//头盔
        Clothes = 1 << 2,//衣服
        Gloves = 1 << 3,//裤子
        Belt = 1 << 4,//腰带
        Bag = 1 << 5,//背包
        Shoes = 1 << 6,//鞋子
        Prop = 1 << 7,//道具
        Attachment = 1 << 8,//插槽
        Ammo = 1<<9,//弹药
        Food = 1 << 10,//食物
        Drink = 1 << 11,//饮料
        Medical = 1 << 12,//药品
    }
}