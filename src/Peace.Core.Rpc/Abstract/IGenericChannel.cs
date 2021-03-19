namespace Peace.Core.Rpc
{
    /// <summary>
    /// 通信信道接口
    /// </summary>
    public interface IGenericChannel
    {
        /// <summary>
        /// 获取rpc客户端
        /// </summary>
        /// <typeparam name="T">客户端接口类型</typeparam>
        /// <param name="serviceName">服务名称</param> 
        /// <returns></returns>
        T GetClient<T>(string serviceName) where T : class;
    }
}
