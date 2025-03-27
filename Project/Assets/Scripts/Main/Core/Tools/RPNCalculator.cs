using System;
using System.Collections.Generic;
using UnityEngine;

namespace EdgeStudio.Tools
{
    public class RPNCalculator
    {
        // 计算逆波兰表达式
        public static bool CalculateRPN(List<string> rpn, Dictionary<string, Func<float>> variables)
        {
            Stack<float> stack = new Stack<float>();

            foreach (string token in rpn)
            {
                if (float.TryParse(token, out float num))
                {
                    stack.Push(num);
                }
                else if (variables.ContainsKey(token))
                {
                    // 如果是变量，调用函数获取值
                    stack.Push(variables[token]());
                }
                else if (IsOperator(token))
                {
                    float b = stack.Pop();
                    float a = stack.Pop();
                    float result = ApplyOperator(token, a, b);
                    stack.Push(result);
                }
                else if (IsComparisonOperator(token))
                {
                    float b = stack.Pop();
                    float a = stack.Pop();
                    bool result = ApplyComparisonOperator(token, a, b);
                    stack.Push(result ? 1 : 0); // 将 bool 转换为 1 或 0
                }
                else if (IsLogicalOperator(token))
                {
                    float b = stack.Pop();
                    float a = stack.Pop();
                    bool result = ApplyLogicalOperator(token, a != 0, b != 0); // 将 1 或 0 转换为 bool
                    stack.Push(result ? 1 : 0); // 将 bool 转换为 1 或 0
                }
                else
                {
                    throw new Exception($"未知标记: {token}");
                }
            }

            return stack.Pop() != 0;
        }

        // 将中缀表达式转换为逆波兰表达式（RPN）
        public static List<string> ConvertToRPN(string expression)
        {
            List<string> output = new List<string>();
            Stack<string> operators = new Stack<string>();

            int i = 0;
            while (i < expression.Length)
            {
                char c = expression[i];

                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                // 处理数字
                if (char.IsDigit(c) || c == '.')
                {
                    string num = "";
                    while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                    {
                        num += expression[i];
                        i++;
                    }
                    output.Add(num);
                    continue;
                }

                // 处理变量
                if (char.IsLetter(c))
                {
                    string varName = "";
                    while (i < expression.Length && char.IsLetterOrDigit(expression[i]))
                    {
                        varName += expression[i];
                        i++;
                    }
                    output.Add(varName);
                    continue;
                }

                // 处理多字符运算符（如 &&, ||）
                if (i + 1 < expression.Length && IsMultiCharOperator(c, expression[i + 1]))
                {
                    string op = c.ToString() + expression[i + 1];
                    i += 2; // 跳过两个字符

                    while (operators.Count > 0 && operators.Peek() != "(" &&
                           GetPrecedence(operators.Peek()) >= GetPrecedence(op))
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Push(op);
                    continue;
                }

                // 处理单字符运算符
                if (IsOperator(c.ToString()) || IsComparisonOperator(c.ToString()))
                {
                    string op = c.ToString();
                    i++;

                    while (operators.Count > 0 && operators.Peek() != "(" &&
                           GetPrecedence(operators.Peek()) >= GetPrecedence(op))
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Push(op);
                    continue;
                }

                // 处理括号
                if (c == '(')
                {
                    operators.Push(c.ToString());
                    i++;
                }
                else if (c == ')')
                {
                    while (operators.Count > 0 && operators.Peek() != "(")
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Pop(); // 弹出 '('
                    i++;
                }
                else
                {
                    throw new Exception($"未知字符: {c}");
                }
            }

            // 将剩余的运算符弹出
            while (operators.Count > 0)
            {
                output.Add(operators.Pop());
            }

            return output;
        }

        private static bool IsOperator(string op) => op is "+" or "-" or "*" or "/";

        private static bool IsComparisonOperator(string op) => op is ">" or "<" or ">=" or "<=" or "==" or "!=";

        private static bool IsLogicalOperator(string op) => op is "&&" or "||";

        private static bool IsMultiCharOperator(char c1, char c2)
        {
            var op = c1.ToString() + c2;
            return op is "&&" or "||" or "==" or "!=" or ">=" or "<=";
        }

        private static int GetPrecedence(string op)
        {
            return op switch
            {
                "&&" or "||" => 1,
                ">" or "<" or ">=" or "<=" or "==" or "!=" => 2,
                "+" or "-" => 3,
                "*" or "/" => 4,
                _ => throw new Exception($"未知运算符: {op}")
            };
        }

        private static float ApplyOperator(string op, float a, float b)
        {
            return op switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => a / b,
                _ => throw new Exception($"未知运算符: {op}")
            };
        }

        private static bool ApplyComparisonOperator(string op, float a, float b)
        {
            return op switch
            {
                ">" => a > b,
                "<" => a < b,
                ">=" => a >= b,
                "<=" => a <= b,
                "==" => Mathf.Approximately(a, b),
                "!=" => !Mathf.Approximately(a, b),
                _ => throw new Exception($"未知比较运算符: {op}")
            };
        }

        private static bool ApplyLogicalOperator(string op, bool a, bool b)
        {
            return op switch
            {
                "&&" => a && b,
                "||" => a || b,
                _ => throw new Exception($"未知逻辑运算符: {op}")
            };
        }
    }
}