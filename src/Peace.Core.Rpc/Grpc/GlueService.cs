using Grpc.Core;
using System.Threading.Tasks;

namespace Peace.Core.Rpc.Grpc
{
    /// <summary>
    /// 内部grpc通讯框架服务
    /// </summary>
    [BindServiceMethod(typeof(GlueService), "BindService")]
    internal class GlueService
    {
        private readonly GrpcServer _grpcServer;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="log">日志</param>
        /// <param name="grpcServer">redis</param>  
        public GlueService(GrpcServer grpcServer)
        {
            _grpcServer = grpcServer;
        }

        /// <summary>
        /// 发送任务状态
        /// </summary>
        /// <param name="request">任务状态请求</param>
        /// <param name="context">服务调用上下文</param>
        /// <returns>任务状态处理结果</returns>
        public async Task<byte[]> Invoke(byte[] request, ServerCallContext context)
        {
            return await _grpcServer.InvokeAsync(request).ConfigureAwait(false);
        }
    }
}