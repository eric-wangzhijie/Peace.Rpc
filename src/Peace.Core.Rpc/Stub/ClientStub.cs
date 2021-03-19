using Peace.Common;
using System.Net;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 客户端存根
    /// </summary>
    public class ClientStub : StubBase
    {
        private readonly ClientConnectionPool _connectionPool;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="host">远程服务地址ip</param>
        /// <param name="port">远程服务地址端口</param> 
        /// <param name="clusterToken">token</param>
        /// <param name="logger">日志</param>
        public ClientStub(string host, int port, string clusterToken, ILog logger, ITraceChain traceChain) : base(logger)
        {
            EndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            ClusterToken = clusterToken;
            TraceChain = traceChain;
            EnableServiceDiscovery = false;
        }

        /// <summary>
        /// 构造函数
        /// </summary>   
        /// <param name="version">服务实例版本</param> 
        /// <param name="clusterToken">token</param>
        /// <param name="logger">日志</param>
        /// <param name="redisConnection">注册中心连接地址</param>
        public ClientStub(int version, string clusterToken, ILog logger, string redisConnection, ITraceChain traceChain) : base(logger)
        {
            ClusterToken = clusterToken;
            EnableServiceDiscovery = true;
            TraceChain = traceChain;
            _connectionPool = new ClientConnectionPool(redisConnection, version);
            Functions.Add(int.MinValue, _connectionPool.RefreshAll);
        }

        /// <summary>
        /// 集群token
        /// </summary>
        public string ClusterToken { get; }

        /// <summary>
        /// 远程服务端地址
        /// </summary>
        public IPEndPoint EndPoint { get; }

        /// <summary>
        /// 链路管理
        /// </summary>
        public ITraceChain TraceChain { get; }

        /// <summary>
        /// 启用注册中心服务发现
        /// </summary>
        internal bool EnableServiceDiscovery { get; }


        /// <summary> 
        /// 获取可用的服务连接数量
        /// </summary> 
        /// <param name="serviceMetadata">服务元数据</param>
        /// <returns>一个可用的服务地址信息</returns>
        public int GetConnectionCount(ServiceMetadata serviceMetadata)
        {
            return _connectionPool.GetCount(serviceMetadata);
        }

        /// <summary>
        /// 移除一个服务的连接
        /// </summary>
        /// <param name="serviceMetadata">服务元数据</param>
        public void RemoveConnection(ServiceMetadata serviceMetadata)
        {
            _connectionPool.Remove(serviceMetadata);
        }

        /// <summary> 
        /// 获取一个的连接
        /// </summary>
        /// <param name="serviceMetadata">服务元数据</param>  
        /// <returns>一个可用的服务地址信息</returns>
        public ClientConnection EnsureConnection(ServiceMetadata serviceMetadata)
        {
            return _connectionPool.Ensure(serviceMetadata);
        }
    }
}
