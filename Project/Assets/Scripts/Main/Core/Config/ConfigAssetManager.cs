using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Panthea.Common;
using UnityEngine.Scripting;

namespace EdgeStudio.Config
{
    [Preserve]
    public interface IConfig
    {
    }

    /// <summary>
    /// 特殊函数 PostPipeline,PrePipeline
    /// private static void PostPipeline:在配置表被加载之后做一些特殊的处理
    /// (未实现)private static void PrePipeline:在配置表被加载之前做一些特殊的处理
    /// </summary>
    [Preserve]
    public abstract class ConfigAssetManager<T>: IConfig where T : IConfig
    {
        protected static Dictionary<int, T> mIntKeyValue;
        protected static Dictionary<string, T> mStringKeyValue;
        private static Type mType;

        /// <summary>
        /// 用于随机取值使用的
        /// </summary>
        protected static List<T> Values;

        public static void Load(string json)
        {
            mType = typeof(T);
            bool isInt = mType.GetProperty("Id")?.PropertyType == typeof(int);
            var method = mType.GetMethod("PostPipeline", BindingFlags.NonPublic | BindingFlags.Static);
            if (isInt)
            {
                try
                {
                    mIntKeyValue = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, T>>(json);
                }
                catch (Exception e)
                {
                    mIntKeyValue = new Dictionary<int, T>();
                    throw new Exception("序列化配置表" + mType.Name + ",发生错误:\n" + e);
                }

                var property = mType.GetProperty("Key");
                if (property != null)
                {
                    var getKey = property.GetGetMethod(false).CreateDelegate(typeof(Func<T,string>)) as Func<T,string>;
                    mStringKeyValue = new Dictionary<string, T>();
                    if (getKey != null)
                    {
                        foreach (var node in mIntKeyValue)
                        {
                            try
                            {
                                mStringKeyValue.Add(getKey(node.Value), node.Value);
                            }
                            catch(Exception ex)
                            {
                                Log.Error(ex);
                            }
                        } 
                    }
                    if (method != null)
                    {
                        method.Invoke(null, new object[] { mIntKeyValue, mStringKeyValue });
                    }
                }
                else
                {
                    if (method != null)
                    {
                        method.Invoke(null, new object[] { mIntKeyValue });
                    }
                }
            }
            else
            {
                try
                {
                    mStringKeyValue = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
                    var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, T>>(json);
                    foreach (var node in dict)
                    {
                        mStringKeyValue.Add(node.Key, node.Value);
                    }
                }
                catch(Exception e)
                {
                    mStringKeyValue = new Dictionary<string, T>();
                    throw new Exception("序列化配置表" + mType.Name + ",发生错误:\n" + e);
                }
                if (method != null)
                {
                    method.Invoke(null, new object[] { mStringKeyValue });
                }
            }
        }

        public static T Read(int id) => mIntKeyValue.GetValueOrDefault(id);

        public static T Read(string id) => id == null ? default : mStringKeyValue.GetValueOrDefault(id);

        public static Dictionary<int, T> ReadintDict() => mIntKeyValue;

        public static Dictionary<string, T> ReadstringDict() => mStringKeyValue;

        public static List<T> ReadList()
        {
            if (Values == null)
            {
                if (mIntKeyValue != null && mIntKeyValue.Count != 0)
                {
                    Values = mIntKeyValue.Values.ToList();
                }
                else if (mStringKeyValue != null && mStringKeyValue.Count != 0)
                {
                    Values = mStringKeyValue.Values.ToList();
                }
                else
                {
                    Values = new List<T>();
                }
            }

            return Values;
        }
    }
}