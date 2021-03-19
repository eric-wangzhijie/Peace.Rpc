using Grpc.Core;
using System;
using System.Threading;

namespace Peace.Core.Rpc.Grpc
{
    /// <summary>
    /// 内部grpc通讯客户端
    /// </summary>
    public partial class GlueServiceClient : ClientBase<GlueServiceClient>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="channel">信道</param>
        public GlueServiceClient(ChannelBase channel) : base(channel)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configuration">客户端配置</param>
        protected GlueServiceClient(ClientBaseConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// 异步的一元调用
        /// </summary>
        /// <param name="request">请求数据</param>
        /// <param name="headers">元数据</param>
        /// <param name="deadline">超时</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        public virtual AsyncUnaryCall<byte[]> Invoke(byte[] request, Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        {
            return Invoke(request, new CallOptions(headers, deadline, cancellationToken));
        }

        /// <summary>
        /// 异步的一元调用
        /// </summary>
        /// <param name="request">请求数据</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual AsyncUnaryCall<byte[]> Invoke(byte[] request, CallOptions options)
        {
            return CallInvoker.AsyncUnaryCall(MethodHandler.InvokeMethod, null, options, request);
        }

        protected override GlueServiceClient NewInstance(ClientBaseConfiguration configuration)
        {
            return new GlueServiceClient(configuration);
        }
    }
}
