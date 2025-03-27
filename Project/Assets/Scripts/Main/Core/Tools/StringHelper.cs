using System;
using Cysharp.Text;

namespace EdgeStudio.Tools
{
    public class StringHelper
    {
        private static readonly string[] CHINESE_SUFFIXES = { "", "万", "亿","万亿", "兆","万兆","亿兆", "京", "垓", "秭", "穰", "沟", "涧", "正", "载", "极", "恒河沙", "阿僧祇", "那由他", "不可思议", "无量大数" };

        public static void ToCultivationLevel(uint num,ref Utf16ValueStringBuilder builder){
            var level = num % 15;
            var str = level switch
            {
                2 => "二",
                3 => "三",
                4 => "四",
                5 => "五",
                6 => "六",
                7 => "七",
                8 => "八",
                9 => "九",
                10 => "十",
                11 => "十一",
                12 => "十二",
                13 => "十三",
                14 => "十四",
                15 => "十五",
                _ => "一"
            };
            switch (num)
            {
                case <= 15:
                    builder.Append("练气");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 30:
                    builder.Append("筑基");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 45:
                    builder.Append("结丹");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 60:
                    builder.Append("元婴");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 75:
                    builder.Append("出窍");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 90:
                    builder.Append("分神");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 105:
                    builder.Append("合体");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 120:
                    builder.Append("渡劫");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 135:
                    builder.Append("散仙");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 150:
                    builder.Append("大乘");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 165:
                    builder.Append("天仙");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 180:
                    builder.Append("金仙");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 195:
                    builder.Append("大罗金仙");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 210:
                    builder.Append("仙君");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 225:
                    builder.Append("仙帝");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                case <= 240:
                    builder.Append("仙尊");
                    builder.Append(str);
                    builder.Append("期");
                    break;
                default:
                    builder.Append("未知境界");
                    break;
            }
        }
        
        public static void FormatLargeNumber(double number,ref Utf16ValueStringBuilder builder)
        {
            int suffixIndex = 0;
            for (; suffixIndex < CHINESE_SUFFIXES.Length - 1 && number >= 10000; suffixIndex++)
            {
                number /= 10000;
            }

            // 四舍五入到三位小数
            number = Math.Round(number, 2);

            // 如果数字是整数，去掉小数部分
            if (number == (int)number)
            {
                builder.Append((int)number);
                builder.Append(CHINESE_SUFFIXES[suffixIndex]);
            }
            else
            {
                builder.Append($"{number:0.##}");
                builder.Append(CHINESE_SUFFIXES[suffixIndex]);
            }
        }

        public static void ConvertTimeToString(long time,ref Utf16ValueStringBuilder builder)
        {
            var timeSpan = TimeSpan.FromSeconds(time);
            if(timeSpan.Hours < 10)
                builder.Append("0");
            builder.Append(timeSpan.Hours);
            builder.Append(":");
            if(timeSpan.Minutes < 10)
                builder.Append("0");
            builder.Append(timeSpan.Minutes);
            builder.Append(":");
            if(timeSpan.Seconds < 10)
                builder.Append("0");
            builder.Append(timeSpan.Seconds); 
        }
        
        public static void ConvertSecondsToDHM(long seconds,ref Utf16ValueStringBuilder builder) 
        {
            int totalMinutes = (int)Math.Ceiling(seconds / 60.0);

            if (totalMinutes == 0) {
                builder.Append("1分钟");
                return;
            }

            int days = totalMinutes / (24 * 60);
            totalMinutes %= 24 * 60;
            int hours = totalMinutes / 60;
            totalMinutes %= 60;
            int minutes = totalMinutes;

            if (days > 0) {
                builder.Append(days);
                builder.Append("天");
            }
            if (hours > 0) {
                if (builder.Length > 0) {
                    builder.Append(" ");
                }
                builder.Append(hours);
                builder.Append("小时");
            }
            if (minutes > 0) {
                if (builder.Length > 0) {
                    builder.Append(" ");
                }
                builder.Append(minutes);
                builder.Append("分钟");
            }
        }
    }
}