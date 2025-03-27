using System;

namespace EdgeStudio.UI
{
    [Flags]
    public enum AnimType 
    {
        None = 1 << 0,
        Custom = 1 << 1, //自定义动画,标记为这个类型的动画.可以重写DoHideAnimation/DoShowAnimation来做自定义的动画
        FadeIn = 1 << 2, //淡入
        FadeOut = 1 << 3, //淡出
        ScaleToZero = 1 << 4, //缩放至0
        ScaleToNormal = 1 << 5, //缩放至原始大小
        Fall = 1 << 6, //从上往下(显示：从屏幕上端移动到屏幕中间，隐藏：从屏幕中间移动到屏幕下端)
        Rise = 1 << 7, //从下往上(显示：从屏幕下端移动到屏幕中间，隐藏：从屏幕中间移动到屏幕上端)
        Jelly = 1 << 8, //果冻动画
    }
}