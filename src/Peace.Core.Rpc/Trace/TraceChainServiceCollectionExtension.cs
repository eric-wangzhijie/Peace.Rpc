using Microsoft.Extensions.DependencyInjection;
using System;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 链路管理
    /// </summary>
    public static class TraceChainServiceCollectionExtension
    {
        /// <summary>
        /// 注入调用链路管理
        /// </summary>
        /// <param name="services">服务集合</param> 
        public static IServiceCollection AddInvokeTracing(this IServiceCollection services, string serivceId)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<ITraceChain>(new TraceManager(serivceId));
            return services;
        }
    }
}
