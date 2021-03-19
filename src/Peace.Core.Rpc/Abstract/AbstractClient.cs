using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 客户端基类
    /// </summary>
    public abstract class AbstractClient : DynamicObject
    {
        /// <summary>
        /// 请求超时设置
        /// </summary>
        protected const int RequestTimeoutSeconds = 60;

        /// <summary>
        /// [endpoint 接口名称/endpoint 对象实例]
        /// </summary>
        private readonly Dictionary<string, object> _endPoints = new Dictionary<string, object>();

        private readonly Type _serviceclientType;
        private readonly ClientStub _clientStub;

        /// <summary>
        /// 构造函数
        /// </summary> 
        /// <param name="serviceclientType">服务实例对象类型</param> 
        internal AbstractClient(string serviceName, Type serviceclientType, ClientStub clientStub)
        {
            _serviceclientType = serviceclientType;
            _clientStub = clientStub;

            var properties = _serviceclientType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(w => typeof(IEndPoint).IsAssignableFrom(w.PropertyType));
            foreach (var p in properties)
            {
                var endPoint = InitializeEndPoint(serviceName, p.PropertyType, _clientStub);
                //创建代理 
                _endPoints.Add(p.Name, endPoint);
            }
        }

        /// <summary>
        /// 获取客户端终结点访问类
        /// </summary>
        /// <param name="serviceName">服务名称</param> 
        /// <param name="type">对象类型</param>
        /// <param name="clientStub">存根</param>
        /// <returns>客户端访问对象</returns>
        protected abstract IEndPoint InitializeEndPoint(string serviceName, Type type, ClientStub clientStub);

        /// <summary>
        /// 调用入口
        /// </summary>
        /// <param name="binder">请求的绑定对象</param> 
        /// <param name="result">返回结果</param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            if (_endPoints.ContainsKey(binder.Name))
            {
                result = _endPoints[binder.Name];
                return true;
            }
            return false;
        }
    }
}
