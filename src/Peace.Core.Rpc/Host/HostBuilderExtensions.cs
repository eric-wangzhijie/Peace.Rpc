using Microsoft.Extensions.Hosting;
using System;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 主机创建类的扩展
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// 主机配置
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="configure">配置</param>
        /// <returns></returns>
        public static IHostBuilder ConfigureRPCHost(this IHostBuilder hostBuilder, Action<IGenericHostBuilder> configure)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                GenericHostBuilder genericHostBuilder = new GenericHostBuilder(services);
                configure(genericHostBuilder);
                genericHostBuilder.Intialize();
            });
            return hostBuilder;
        }
    }
}
