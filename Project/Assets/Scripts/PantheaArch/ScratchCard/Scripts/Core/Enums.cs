using System;

namespace ScratchCardAsset.Core
{
    public enum Quality
    {
        Low = 4,
        Medium = 2,
        High = 1
    }

    public enum ScratchMode
    {
        Erase,
        Restore
    }

    public enum ScratchAnimationSpace
    {
        UV,
        Texture
    }

    public enum ProgressAccuracy
    {
        Default,
        High
    }
    
    [Flags]
    public enum InputMethods
    {
        None = 0,
        Pen = 1,
        Touch = 2,
        Mouse = 4,
        Custom = 8,
    }
}