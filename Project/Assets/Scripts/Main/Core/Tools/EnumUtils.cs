using System;
using Sirenix.OdinInspector;

namespace EdgeStudio
{
    public static class EnumUtils
    {
        public static ValueDropdownList<int> ConvertEnumToDropdownList<TEnum>() where TEnum : Enum
        {
            var dropdownList = new ValueDropdownList<int>();

            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                string key = value.ToString();
                int enumValue = Convert.ToInt32(value);
                dropdownList.Add(key, enumValue);
            }

            return dropdownList;
        }
    }
}