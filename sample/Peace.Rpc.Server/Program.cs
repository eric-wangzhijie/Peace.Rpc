using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Peace.Common;
using Peace.Core.Rpc;
using Peace.Core.Rpc.Grpc;
using Peace.Rpc.Shared;
using System;

namespace Peace.Rpc.Server
{
    class Program
    {
        public static void Main(string[] args)
        {
            //使用redis作为注册中心的模式
            //  CreateHostBuilder(args).Build().Run();

            //使用直接连接模式 类似Echo ,如果使用这种模式需要保整终结点api为无参构造函数，也可以自行改造
            StartService();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             .ConfigureLogging(logging =>
             {
                 logging.ClearProviders();
             })
             .ConfigureRPCHost(builder =>
             {
                 builder.UseService<ITestServiceClient, TestService>();
             });


        public static void StartService()
        {
            TestService testService = new TestService();
            ServiceMetadata serviceDescription = new ServiceMetadata
            {
                Id = ServiceSettings.Items.ServiceId,
                ServiceName = testService.ServiceName,
                ServiceType = testService.Type,
                Host = ServiceSettings.Items.Address.Split(':')[0],
                Port = int.Parse(ServiceSettings.Items.Address.Split(':')[1]),
                EffectiveTime = DateTime.Now
            };
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
            FileTextLogger fileTextLogger = new FileTextLogger(true, ServiceSettings.Items.ServiceId, ServiceSettings.Items.Address, path);
            IServer server = new GrpcServer(serviceDescription, ServiceSettings.Items.ClusterToken, fileTextLogger);
            //注册服务  
            server.UseService(testService);

            //注册终结点
            server.RegisterEndPoint(typeof(ITestApi), typeof(TestApi));

            server.Start();
        }
    }

}
