using System;

namespace EdgeStudio.UI
{


    [AttributeUsage(AttributeTargets.Class)]
    public class UIWidgetAttribute : Attribute
    {
        /// 自定义层级 (默认为0)
        public UISortingOrder SortingOrder;

        /// 切换场景的时候不销毁该界面
        /// 回退键也不会销毁
        public bool DontDestroyOnLoad;

        /// 不要自动全屏进行适配.
        /// 用于一些特殊情况
        public bool DontFullScreen;

        /// 设置了该标记的UI不会被放入堆栈当中
        public bool GetControl;

        /// 如果为True回退键不会关闭界面
        public bool IgnoreBack;

        /// 当物体被销毁时第一时间压入池中(默认不开启池模式)
        public bool Pool;

        /// 是否可以重复创建(该选项默认关闭)
        public bool Repeat;

        /// 是否开启FairyBatch优化(该选项默认开启)
        public bool Optimize = true;

        /// 是否显示黑底界面
        public bool Background;

        /// 窗口打开时播放的动画
        public AnimType Enter = AnimType.None;

        /// 窗口关闭时播放的动画
        public AnimType Exit = AnimType.None;

        public bool Dynamic = false;

        public static implicit operator UIWidget(UIWidgetAttribute attribute)
        {
            return new UIWidget
            {
                SortingOrder = attribute.SortingOrder,
                Background = attribute.Background,
                Enter = attribute.Enter,
                Exit = attribute.Exit,
                Optimize = attribute.Optimize,
                Pool = attribute.Pool,
                Repeat = attribute.Repeat,
                GetControl = attribute.GetControl,
                IgnoreBack = attribute.IgnoreBack,
                DontFullScreen = attribute.DontFullScreen,
                DontDestroyOnLoad = attribute.DontDestroyOnLoad,
                Dynamic = attribute.Dynamic
            };
        }
    }

    ///运行时Builder使用,避免使用Class产生GC,注意.一个Struct不宜超过64Byte,否则会很影响性能(内存拷贝)
    public struct UIWidget
    {
        public UISortingOrder SortingOrder;
        public bool DontDestroyOnLoad;
        public bool DontFullScreen;
        public bool GetControl;
        public bool IgnoreBack;
        public bool Pool;
        public bool Repeat;
        public bool Optimize;
        public bool Background;
        public AnimType Enter;
        public AnimType Exit;
        public bool Dynamic;

        public static readonly UIWidget Default = new()
        {
            SortingOrder = UISortingOrder.Default, Background = false, Enter = AnimType.None, Exit = AnimType.None, Optimize = true, Pool = false,
            Repeat = false, GetControl = false, IgnoreBack = false, DontFullScreen = true, DontDestroyOnLoad = false,Dynamic = false
        };

    }
}