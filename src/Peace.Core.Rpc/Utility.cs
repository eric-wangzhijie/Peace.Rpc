using System;
using System.Threading.Tasks;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 帮助类
    /// </summary>
    public class Utility
    {
        /// <summary>
        /// 转换一个object对象到指定的类型
        /// </summary>
        /// <param name="value">对象值</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static object ChangeType(object value, Type type)
        {
            if (value == null)
            {
                return null;
            }

            if (type == value.GetType())
            {
                return value;
            }

            if (value.GetType().IsSubclassOf(type))
            {
                return GetGenericByType(type, value);
            }

            if (type.IsEnum)
            {
                if (value is string)
                {
                    return Enum.Parse(type, value as string);
                }
                else
                {
                    return Enum.ToObject(type, value);
                }
            }

            if (!type.IsInterface && type.IsGenericType)
            {
                Type innerType = type.GetGenericArguments()[0];
                object innerValue = ChangeType(value, innerType);
                return Activator.CreateInstance(type, new object[] { innerValue });
            }

            if (value is string && type == typeof(Guid))
            {
                return new Guid(value as string);
            }

            if (value is string && type == typeof(Version))
            {
                return new Version(value as string);
            }

            if (!(value is IConvertible))
            {
                return value;
            }

            return Convert.ChangeType(value, type);
        }

        /// <summary>
        /// 通过类型变量获取该泛型对象值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetGenericByType(Type type, object obj)
        {
            var method = typeof(Utility).GetMethod(nameof(ConvertTypeToTBaseValue));
            return method.MakeGenericMethod(type).Invoke(null, new object[] { obj });
        }

        /// <summary>
        /// 显示转换为Task泛型返回值
        /// </summary>
        /// <typeparam name="TBase"></typeparam> 
        /// <param name="value"></param>
        /// <returns></returns>
        public static TBase ConvertTypeToTBaseValue<TBase>(object value)
        {
            return (TBase)value;
        }


        /// <summary>
        /// 通过类型变量获取该泛型对象值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetTaskByType(Type type, object obj)
        {
            var method = typeof(Utility).GetMethod(nameof(ConvertTypeToTaskValue));
            return method.MakeGenericMethod(type).Invoke(null, new object[] { obj });
        }

        public static Task<TBase> ConvertTypeToTaskValue<TBase>(TBase value)
        {
            return Task.FromResult(value);
        }
    }
}
