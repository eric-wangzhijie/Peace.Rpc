using System;
using System.Text.Json.Serialization;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 要注册的服务元数据
    /// </summary> 
    public class ServiceMetadata
    {
        /// <summary>
        /// 服务实例的Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 服务实例的主机ip
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 服务实例的端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 服务实例版本,发版过程中仅允许同时存在最多两个不同版本的实例,默认版本为-1，仅当无法匹配对应灰度的时候则降级灰度到-1的实例上
        /// </summary>
        public int Version { get; set; } = -1; 

        /// <summary>
        /// 服务的名称
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 服务类型
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// 生效时间 对于无状态服务来说，注册的时候即生效时间
        /// </summary>
        public DateTime EffectiveTime { get; set; }

        /// <summary>
        /// 最后一次心跳线的时间
        /// </summary>
        public DateTime LastEchoTime { get; set; }
    }
}