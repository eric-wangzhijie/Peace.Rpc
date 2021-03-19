using Microsoft.Extensions.DependencyInjection;
using Peace.Core.Rpc;
using Peace.Rpc.Shared;
using System.Threading.Tasks;

namespace Peace.Rpc.Server
{
    public class TestService : ServiceBase, ITestServiceClient
    {
        public override string ServiceName => ITestServiceClient.ServiceName;
        public override ServiceType Type => ITestServiceClient.Type;

        public ITestApi TestApi { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            //注册终结点
            services.AddSingleton<ITestApi, TestApi>();
        }

        /// <summary>
        /// 完成依赖注入后执行此方法
        /// </summary>
        public void OnInit()
        {

        }

        /// <summary>
        /// 优雅下线执行该方法
        /// </summary>
        public void OnStopping()
        {

        }

        /// <summary>
        /// 后台作业类型
        /// </summary>
        public override DeamonType DeamonType => DeamonType.Unspecified;

        /// <summary>
        /// 后台作业
        /// </summary>
        /// <param name="deamonId"></param>
        /// <returns></returns>
        protected override async Task Deamon(int deamonId)
        {
            await Task.CompletedTask;
        }
    }
}
