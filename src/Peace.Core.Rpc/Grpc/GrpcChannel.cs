using System;
using Peace.Common;

namespace Peace.Core.Rpc.Grpc
{
    /// <summary>
    /// Grpc通讯管理
    /// </summary>
    public class GrpcChannel : AbstractChannel
    {
        /// <summary>
        /// dotNetty 客户端通讯管理
        /// </summary> 
        /// <param name="host">要连接的远程服务ip</param>
        /// <param name="port">要连接的远程服务端口</param>
        /// <param name="clusterToken">集群授权码</param>
        /// <param name="logger">日志</param>
        public GrpcChannel(string host, int port, string clusterToken, ILog logger, ITraceChain traceChain) : base(host, port, clusterToken, logger, traceChain)
        {
        }

        /// <summary>
        /// dotNetty 客户端通讯管理,当前模式下使用注册中心发现服务
        /// </summary>   
        /// <param name="clusterToken">集群授权码</param>
        /// <param name="logger">日志</param>
        /// <param name="redisConnection">注册中心redis</param>
        /// <param name="traceChain">链路管理</param>
        public GrpcChannel(int version, string clusterToken, ILog logger, string redisConnection, ITraceChain traceChain)
            : base(version, clusterToken, logger, redisConnection, traceChain)
        {
        }

        /// <summary>
        /// 获取客户端访问类
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="serivceClientType">服务元数据信息</param>
        /// <param name="clientStub">存根</param>
        /// 
        /// <returns>客户端访问对象</returns>
        protected override AbstractClient InitializeClient(string serviceName, Type serivceClientType, ClientStub clientStub)
        {
            return new GrpcClient(serviceName, serivceClientType, clientStub);
        }
    }
}
