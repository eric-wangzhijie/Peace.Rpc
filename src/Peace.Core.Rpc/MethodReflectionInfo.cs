using System;
using System.Collections.Generic;
using System.Reflection;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 反射的方法信息
    /// </summary>
    public class MethodReflectionInfo
    {
        /// <summary>
        /// 方法描述信息
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// 方法注解属性集合
        /// </summary>
        public List<Attribute> Attributes { get; set; }

        /// <summary>
        /// 方法参数集合
        /// </summary>
        public ParameterInfo[] Parameters { get; set; }

        /// <summary>
        /// 返回类型
        /// </summary>
        public Type ReturnType { get; set; }

        /// <summary>
        /// 是否该方法可以等待
        /// </summary>
        public bool IsAwaitable { get; set; }
    }
}
