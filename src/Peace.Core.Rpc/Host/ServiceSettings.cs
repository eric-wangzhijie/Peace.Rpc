namespace Peace.Core.Rpc
{
    /// <summary>
    /// 站点配置
    /// </summary>
    public class ServiceSettings : Config<ServiceSettings>
    {
        /// <summary>
        /// 服务实例唯一ID
        /// </summary>
        public string ServiceId { get; set; } 

        /// <summary>
        /// 服务实例版本,发版过程中仅允许同时存在最多两个不同版本的实例
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 服务地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 集群令牌
        /// </summary>
        public string ClusterToken { get; set; } 

        /// <summary>
        /// 注册中心redis连接字符串
        /// </summary>
        public string RegistryRedisConnection { get; set; } 
    }
}
