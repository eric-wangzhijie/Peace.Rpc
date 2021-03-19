using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Peace.Core.Rpc.Grpc;
using System;
using System.Linq;
using System.Reflection;
using Peace.Common;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 服务的主机创建
    /// </summary>
    class GenericHostBuilder : IGenericHostBuilder
    {
        public static TimeSpan ShutdownTimeoutSeconds = TimeSpan.FromSeconds(30);
        private readonly IServiceCollection _services;

        private ServiceBase _serviceInstance;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="services">注入服务集合</param>
        public GenericHostBuilder(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Intialize()
        {
            if (string.IsNullOrEmpty(ServiceSettings.Items.Address) || ServiceSettings.Items.Address.Split(':').Length != 2)
            {
                throw new ArgumentException("The serivce address is illegal.");
            }

            //基础组件依赖注入   
            _services.AddFileLog(new TextLogOptions { ServiceId = ServiceSettings.Items.ServiceId, Source = ServiceSettings.Items.Address });
            _services.AddInvokeTracing(ServiceSettings.Items.ServiceId);

            //解析终结点
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.IsClass && typeof(IEndPoint).IsAssignableFrom(t));
            var properties = _serviceInstance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(w => typeof(IEndPoint).IsAssignableFrom(w.PropertyType));
            foreach (PropertyInfo p in properties)
            {
                _services.AddSingleton(p.PropertyType, types.First(f => p.PropertyType.IsAssignableFrom(f)));
            }

            //启动服务
            _services.AddSingleton<IHostedService>(provider =>
            {
                string host = ServiceSettings.Items.Address.Split(':')[0];
                int port = int.Parse(ServiceSettings.Items.Address.Split(':')[1]);

                ServiceMetadata serviceDescription = new ServiceMetadata
                {
                    Id = ServiceSettings.Items.ServiceId,
                    ServiceName = _serviceInstance.ServiceName,
                    ServiceType = _serviceInstance.Type,
                    Host = host,
                    Port = port,
                    EffectiveTime = DateTime.Now
                };

                IServer server = new GrpcServer(serviceDescription, ServiceSettings.Items.ClusterToken, provider.GetRequiredService<ILog>(), ServiceSettings.Items.RegistryRedisConnection);

                //注册服务  
                server.UseService(_serviceInstance);

                //注册终结点 
                foreach (var p in properties)
                {
                    object value = provider.GetRequiredService(p.PropertyType);
                    server.RegisterEndPoint(p.PropertyType, value as IEndPoint);
                    //将值填充到服务实例中
                    p.SetValue(_serviceInstance, value);
                }
                return new LifetimeEventsHostedService(host, port, server, provider);
            });
            _services.AddOptions<HostOptions>().Configure(opts => opts.ShutdownTimeout = GenericHostBuilder.ShutdownTimeoutSeconds);
        }

        /// <summary>
        /// 配置服务
        /// </summary>
        /// <typeparam name="TIService">服务接口类型</typeparam>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns></returns>
        public IGenericHostBuilder UseService<TIService, TService>() where TService : ServiceBase, TIService, new()
        {
            Type interfaceType = typeof(TIService);
            Type type = typeof(TService);

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            MethodInfo methodInfo = methods.SingleOrDefault(s => s.Name == "ConfigureServices" && s.GetParameters().Length == 1 && s.GetParameters().First().ParameterType == typeof(IServiceCollection));
            if (methodInfo == null)
            {
                throw new ArgumentException($"Can not find a method ConfigureServices with a parameter IServiceCollection in service[{type.FullName}].");
            }

            _serviceInstance = Activator.CreateInstance(type) as ServiceBase;
            _services.AddSingleton(interfaceType, _serviceInstance);
            methodInfo.Invoke(_serviceInstance, new object[] { _services });
            return this;
        }
    }
}