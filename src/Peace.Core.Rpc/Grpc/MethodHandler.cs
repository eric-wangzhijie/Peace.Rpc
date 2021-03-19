using Grpc.Core;

namespace Peace.Core.Rpc.Grpc
{
    /// <summary>
    /// 方法处理
    /// </summary>
    class MethodHandler
    {
        static readonly Marshaller<byte[]> ThroughMarshaller = new Marshaller<byte[]>(x => x, x => x);

        /// <summary>
        /// 统一调用方法描述
        /// </summary>
        public static Method<byte[], byte[]> InvokeMethod = new Method<byte[], byte[]>(MethodType.Unary, nameof(GlueService), nameof(GlueService.Invoke), ThroughMarshaller, ThroughMarshaller);
    }
}
