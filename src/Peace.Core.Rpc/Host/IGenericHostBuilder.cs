namespace Peace.Core.Rpc
{
    /// <summary>
    /// 通用的主机创建接口
    /// </summary>
    public interface IGenericHostBuilder
    {
        /// <summary>
        /// 配置服务
        /// </summary>
        /// <typeparam name="TIService">服务接口类型</typeparam>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns></returns>
        IGenericHostBuilder UseService<TIService, TService>() where TService : ServiceBase, TIService, new();
    }
}
