using System;
using System.Threading.Tasks;
using Peace.Common;

namespace Peace.Core.Rpc
{
    /// <summary>
    ///  服务注册和发现管理
    /// </summary>
    class Registry
    {
        private const int StopIntervalSeconds = 60;
        private readonly ServiceDirectoryProxy _directory = null;
        private readonly ILog _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="redisConnection">注册中心redis连接地址</param>
        /// <param name="logger">日志</param>
        public Registry(string redisConnection, ILog logger)
        {
            _logger = logger;
            _directory = new ServiceDirectoryProxy(redisConnection);
        }

        /// <summary>
        /// 发送服务实例心跳
        /// </summary> 
        /// <param name="serviceDesc">要注册的服务实例</param>
        public async Task Renew(ServiceMetadata serviceDesc)
        {
            try
            {
                if (serviceDesc.Host == "0.0.0.0")
                {
                    serviceDesc.Host = Environment.GetEnvironmentVariable("ServiceAddress") ?? Common.Utility.GetLocalIp();
                }
                serviceDesc.LastEchoTime = DateTime.Now;
                await _directory.Register(serviceDesc);
                _logger.WriteInfoLog("Renewed heartbeat...");
            }
            catch (Exception ex)
            {
                _logger.WriteErrorLog(ex);
            }
        }

        /// <summary>
        /// 注册服务实例
        /// </summary> 
        /// <param name="serviceDesc">要注册的服务实例</param>
        public async Task Register(ServiceMetadata serviceDesc)
        {
            DateTime start = DateTime.Now;
            bool isExited = false;
            while (true)
            {
                try
                {
                    ServiceMetadata service = await _directory.LookFor(serviceDesc.ServiceName, serviceDesc.Id);
                    if (service != null)
                    {
                        if (serviceDesc.Host == service.Host && serviceDesc.Port == service.Port)
                        {
                            _logger.WriteWarnLog($"Encounting a conflict. Found a service (name:{service.ServiceName} id:{service.Id} address:{serviceDesc.Host}:{serviceDesc.Port}) from registy.");
                        }
                        else
                        {
                            _logger.WriteWarnLog($"The service (name:{serviceDesc.ServiceName} id:{serviceDesc.Id} address:{serviceDesc.Host}:{serviceDesc.Port}) has registered.");
                        }
                        isExited = true;
                    }
                    else
                    {
                        if (serviceDesc.Host == "0.0.0.0")
                        {
                            serviceDesc.Host = Environment.GetEnvironmentVariable("ServiceAddress") ?? Common.Utility.GetLocalIp();
                        }
                        await _directory.Register(serviceDesc);
                        _logger.WriteInfoLog($"Register successfully...");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (DateTime.Now.Subtract(start).TotalSeconds > StopIntervalSeconds) //注册失败持续超过一分钟，将关闭进程
                    {
                        _logger.WriteErrorLog($"Register faild for {StopIntervalSeconds} seconds. Exiting server proccess...");
                        isExited = true;
                    }
                    _logger.WriteErrorLog(ex);
                }
                if (isExited)
                {
                    await Task.Delay(5000);
                    Environment.Exit(0);
                }
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// 取消掉服务实例
        /// </summary> 
        /// <param name="serviceDesc">要撤销的服务实例</param>
        public async Task Deregister(ServiceMetadata serviceDesc)
        {
            await _directory.Deregister(serviceDesc);
        }
    }
}
