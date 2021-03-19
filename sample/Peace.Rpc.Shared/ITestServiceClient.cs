
using Peace.Core.Rpc;

namespace Peace.Rpc.Shared
{
    public interface ITestServiceClient
    { 
        /// <summary>
        /// 服务名称
        /// </summary>
        public const string ServiceName = "TestService";

        /// <summary>
        /// 服务类型
        /// </summary>
        public const ServiceType Type = ServiceType.Stateless;

        public ITestApi TestApi { get; }
    }
}
