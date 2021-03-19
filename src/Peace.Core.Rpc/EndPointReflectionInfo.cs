using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 终结点元数据
    /// </summary>
    public class EndPointReflectionInfo
    {
        /// <summary>
        /// 终结点类型
        /// </summary>
        public Type EndPointType { get; private set; }

        /// <summary>
        /// 方法元数据集合
        /// </summary>
        public List<MethodReflectionInfo> Methods { get; private set; }

        /// <summary>
        /// 终结点注解属性集合
        /// </summary>
        public List<Attribute> Attributes { get; private set; }

        /// <summary>
        /// 获取服务信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static EndPointReflectionInfo GetEndPointInfo(Type type)
        {
            var info = new EndPointReflectionInfo() { EndPointType = type, Attributes = type.GetCustomAttributes().ToList() };
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var methodInfoList = new List<MethodReflectionInfo>();
            foreach (var method in methods)
            {
                var mInfo = new MethodReflectionInfo()
                {
                    Attributes = method.GetCustomAttributes().ToList(),
                    Method = method,
                    Parameters = method.GetParameters(),
                    ReturnType = method.ReturnType,
                    IsAwaitable = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null
                };
                methodInfoList.Add(mInfo);
            }
            info.Methods = methodInfoList;
            return info;
        }

        /// <summary>
        /// 获取服务方法信息
        /// </summary>
        /// <param name="name">方法名称</param>
        /// <param name="args">参数集合</param>
        /// <param name="argTypes">参数类型集合</param>
        /// <param name="parameterCount">方法参数数量</param>
        /// <returns></returns>
        public MethodReflectionInfo EnsureMethodInfo(string name, object[] args, Type[] argTypes, int parameterCount)
        {
            var methods = Methods.FindAll(b => b.Method.Name == name && b.Method.GetParameters().Length == parameterCount);

            for (int i = 0; i < methods.Count(); i++)
            {
                bool isMatched = true;
                for (int j = 0; j < methods[i].Parameters.Length; j++)
                {
                    var parameterType = methods[i].Parameters[j].ParameterType;
                    if (args[j] == null && parameterType.IsValueType && !parameterType.IsGenericType)  //为空且为非空值类型，则不匹配
                    {
                        isMatched = false;
                        break;
                    }

                    if (argTypes != null && argTypes.Length > 0)
                    {
                        Type type = argTypes[j];
                        if (!parameterType.IsAssignableFrom(type))//传入参数类型不继承自定义类型，则不匹配
                        {
                            isMatched = false;
                            break;
                        }
                    }
                }
                if (isMatched)
                {
                    return methods[i];
                }
            }

            return null;
        }
    }
}
