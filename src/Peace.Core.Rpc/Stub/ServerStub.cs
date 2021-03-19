using System;
using System.Collections.Generic;
using Peace.Common;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 服务存根
    /// </summary>
    class ServerStub : StubBase
    {
        /// <summary>
        ///  [Api接口名称/Api对象实例]
        /// </summary>
        private readonly Dictionary<string, IEndPoint> _endPoints = new Dictionary<string, IEndPoint>();
        private readonly Workloads _workloads = new Workloads();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="logger"></param>
        public ServerStub(string host, int port, ILog logger) : base(logger)
        {
            Host = host;
            Port = port;
            Workloads = _workloads;
        }

        /// <summary>
        /// 监听的Ip地址
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// 监听的端口
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// 服务启动类
        /// </summary>
        public ServiceBase Service { get; internal set; }

        /// <summary>
        /// 请求负载
        /// </summary>
        public Workloads Workloads { get; }

        /// <summary>
        /// 添加服务实例
        /// </summary>
        /// <param name="interfaceType">服务接口类型</param>
        /// <param name="type">服务类型</param>
        /// <returns>服务信息</returns>
        internal void AddEndPoint(Type interfaceType, Type type)
        {
            if (_endPoints.ContainsKey(interfaceType.Name))
            {
                throw new ServiceInternalException($"The endpoint {type} has registed.");
            }
            IEndPoint endpoint = Activator.CreateInstance(type) as IEndPoint;
            _endPoints.Add(interfaceType.Name, endpoint);
        }

        /// <summary>
        /// 添加服务实例
        /// </summary>
        /// <param name="interfaceType">服务接口类型</param> 
        /// <param name="instance">服务对象实例</param>
        /// <returns>服务信息</returns>
        internal void AddEndPoint(Type interfaceType, IEndPoint instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_endPoints.ContainsKey(interfaceType.Name))
            {
                throw new ServiceInternalException($"The endpoint {interfaceType.Name} has registed.");
            }
            _endPoints.Add(interfaceType.Name, instance);
        }

        /// <summary>
        /// 获取服务实例
        /// </summary>
        /// <param name="interfaceTypeName">服务接口类型</param>
        /// <returns>服务信息</returns>
        internal IEndPoint GetEndPoint(string interfaceTypeName)
        {
            if (_endPoints.ContainsKey(interfaceTypeName))
            {
                return _endPoints[interfaceTypeName];
            }
            return null;
        }
    }
}
