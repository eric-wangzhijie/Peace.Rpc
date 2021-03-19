using Grpc.Core;

namespace Peace.Core.Rpc.Grpc
{
    /// <summary>
    /// grpc服务绑定
    /// </summary>
    internal class GlueServiceBinder : ServiceBinderBase
    {
        /// <summary>
        /// 绑定服务
        /// </summary>
        /// <param name="serviceImpl"></param>
        /// <returns></returns>
        public static ServerServiceDefinition BindService(GlueService serviceImpl)
        {
            return ServerServiceDefinition.CreateBuilder().AddMethod(MethodHandler.InvokeMethod, serviceImpl.Invoke).Build();
        }
    }
}
