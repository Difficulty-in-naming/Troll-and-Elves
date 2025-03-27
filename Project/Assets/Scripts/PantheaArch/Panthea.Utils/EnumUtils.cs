using System;

namespace PantheaArch.Panthea.Utils
{
    public static class EnumUtils
    {
        public static T GetFirstFlag<T>(this T flags) where T: Enum
        {
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                if (flags.HasFlag(value))
                    return value;
            }

            return default;
        }
    }
}