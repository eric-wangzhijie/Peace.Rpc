using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Peace.Common; 

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 服务生命周期管理
    /// </summary>
    class LifetimeEventsHostedService : IHostedService
    {
        private readonly ILog _logger;
        private readonly IServer _server;
        private readonly string _host;
        private readonly int _port;
        private readonly IServiceProvider _serviceProvider;

        IHostApplicationLifetime _appLifetime;

        /// <summary>
        /// 构造函数
        /// </summary> 
        public LifetimeEventsHostedService(string host, int port, IServer server, IServiceProvider serviceProvider)
        {
            _host = host;
            _port = port;
            _server = server;
            _logger = serviceProvider.GetRequiredService<ILog>();
            _serviceProvider = serviceProvider;
            _appLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _server.Start(_serviceProvider);
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _server.Stop(GenericHostBuilder.ShutdownTimeoutSeconds, _serviceProvider);
            _appLifetime.StopApplication();
        }

        public void OnStarted()
        { 
        }

        public void OnStopping()
        {
            //执行初始化过程 
            _logger.WriteInfoLog("OnStopping has been called.");
        }

        public void OnStopped()
        {
            _logger.WriteInfoLog("OnStopped has been called.");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Environment.ExitCode = 143;
            }
            else
            {
                Environment.ExitCode = 0;
            }
        }
    }
}
