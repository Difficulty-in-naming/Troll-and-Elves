using System;
using System.Collections.Generic;
using EdgeStudio.Tools;

namespace EdgeStudio.DataStruct
{
    public readonly struct ConditionValue
    {
        private readonly string Condition;
        public ConditionValue(string condition) => Condition = condition;
        public bool Check() => string.IsNullOrEmpty(Condition) || CalculateRPN(Condition);
        
        private static readonly Dictionary<string,List<string>> Cached = new();

        private static Dictionary<string, Func<float>> ConditionFunc;

        public static List<string> CacheRPN(string condition)
        {
            if (!Cached.TryGetValue(condition, out var conditionList))
                Cached[condition] = conditionList = RPNCalculator.ConvertToRPN(condition);
            return conditionList;
        }

        public static bool CalculateRPN(List<string> rpn) => RPNCalculator.CalculateRPN(rpn, ConditionFunc);

        public static bool CalculateRPN(string condition)
        {
            var list = CacheRPN(condition);
            return RPNCalculator.CalculateRPN(list, ConditionFunc);
        }
    }
}