using System;
using System.Threading.Tasks;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 服务端接口类
    /// </summary> 
    public interface IServer
    {
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="serviceProvider">服务注入生产者</param>
        Task Start(IServiceProvider serviceProvider = null);

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="serviceProvider">服务注入生产者</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        Task Stop(TimeSpan timeout, IServiceProvider serviceProvider = null);

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="TService">服务接口</typeparam> 
        IServer UseService<TService>() where TService : ServiceBase, new();

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="serviceInstance">服务接口</param> 
        IServer UseService(ServiceBase serviceInstance);

        /// <summary>
        /// 注册终结点
        /// </summary>
        /// <typeparam name="TIEndpoint">终端接口</typeparam>
        /// <typeparam name="TEndpoint">终端类型</typeparam>
        IServer RegisterEndPoint<TIEndpoint, TEndpoint>() where TEndpoint : IEndPoint, TIEndpoint where TIEndpoint : IEndPoint;

        /// <summary>
        /// 注册终结点
        /// </summary>
        /// <param name="endPointInterfaceType">服务接口</param>
        /// <param name="endPointType">服务类型</param> 
        IServer RegisterEndPoint(Type endPointInterfaceType, Type endPointType);

        /// <summary>
        /// 注册终结点
        /// </summary>
        /// <param name="endPointInterfaceType">服务接口</param> 
        /// <param name="endPointInstance">服务实例</param> 
        IServer RegisterEndPoint(Type endPointInterfaceType, IEndPoint endPointInstance);
    }
}