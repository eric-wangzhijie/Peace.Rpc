using System;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 客户端连接信息
    /// </summary>
    public class ClientConnection
    {
        /// <summary>
        /// 服务
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// 服务地址ip
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 服务地址端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool Available { get; set; }

        /// <summary>
        /// 最后的不可用时间
        /// </summary>
        public DateTime LastFailedTime { get; set; } = DateTime.MinValue;
    }
}
