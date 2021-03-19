using ImpromptuInterface;
using Peace.Common;
using System;
using System.Collections.Concurrent;   

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 客户端信道抽象类
    /// </summary>
    public abstract class AbstractChannel : IGenericChannel
    {
        private readonly ClientStub _clientStub;

        /// <summary>
        /// [client 接口名称/client 对象实例]
        /// </summary>
        private ConcurrentDictionary<string, object> _clients = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// 构造函数
        /// </summary> 
        /// <param name="clusterToken">集群授权码</param>
        internal AbstractChannel(string host, int port, string clusterToken, ILog logger, ITraceChain traceChain)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }
            if (port < 0 || port > ushort.MaxValue)
            {
                throw new ArgumentException(nameof(port));
            }
            if (string.IsNullOrEmpty(clusterToken))
            {
                throw new ArgumentNullException(nameof(clusterToken));
            }
            _clientStub = new ClientStub(host, port, clusterToken, logger,traceChain);
        }

        /// <summary>
        /// 当前模式下使用注册中心发现服务
        /// </summary>   
        /// <param name="version">服务实例版本</param>
        /// <param name="clusterToken">集群授权码</param>
        /// <param name="redisConnection">注册中心链接地址</param>
        /// <param name="traceChain">链路管理</param>
        internal AbstractChannel(int version, string clusterToken, ILog logger, string redisConnection, ITraceChain traceChain)
        {
            if (string.IsNullOrEmpty(clusterToken))
            {
                throw new ArgumentNullException(nameof(clusterToken));
            }
            _clientStub = new ClientStub(version, clusterToken, logger, redisConnection, traceChain);
        }

        /// <summary>
        /// 获取客户端终结点访问类
        /// </summary>
        /// <param name="serivceClientType">服务类型</param>
        /// <param name="clientStub">存根</param>
        /// <returns>客户端访问对象</returns>
        protected abstract AbstractClient InitializeClient(string serviceName, Type serivceClientType, ClientStub clientStub);

        /// <summary>
        /// 获取对应服务的客户端
        /// </summary>
        /// <typeparam name="T">客户端接口类型</typeparam>
        /// <param name="serviceName">服务名</param> 
        /// <returns>客户端访问服务实例</returns>
        public T GetClient<T>(string serviceName) where T : class
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException(nameof(serviceName));
            }
             
            var type = typeof(T);
            if (_clients.ContainsKey(serviceName))
            {
                return _clients[serviceName] as T;
            }
            var client = InitializeClient(serviceName, type, _clientStub);
            //创建代理
            T instance = client.ActLike<T>();
            _clients.TryAdd(serviceName, instance);
            return instance;
        }
    }
}
