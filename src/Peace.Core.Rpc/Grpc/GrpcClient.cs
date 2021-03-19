using System;

namespace Peace.Core.Rpc.Grpc
{
    /// <summary>
    /// client客户端
    /// </summary>
    public class GrpcClient : AbstractClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceName">服务名称</param>  
        /// <param name="serviceclientType">服务实例对象类型</param> 
        /// <param name="clientStub">客户端存根</param>
        public GrpcClient(string serviceName, Type serviceclientType, ClientStub clientStub) : base(serviceName, serviceclientType, clientStub)
        {
        }

        /// <summary>
        /// 获取客户端访问类
        /// </summary> 
        /// <param name="serviceName">服务名称</param>  
        /// <param name="serviceclientType">服务实例对象类型</param> 
        /// <param name="clientStub">客户端存根</param>
        /// <returns>客户端访问对象</returns>
        protected override IEndPoint InitializeEndPoint(string serviceName, Type serviceclientType, ClientStub clientStub)
        {
            return new GrpcEndPoint(serviceName, clientStub, serviceclientType);
        }
    }
}
