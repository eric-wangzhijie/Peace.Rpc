using System.Collections.Generic;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 请求消息体
    /// </summary> 
    public class MessageRequest : MessageBase
    {
        /// <summary>
        /// 授权token
        /// </summary>
        public string ClusterToken { get; set; }

        /// <summary>
        /// 请求的服务
        /// </summary>
        public string Service { get; set; } 

        /// <summary>
        /// 请求的终结点
        /// </summary>
        public string EndPoint { get; set; }

        /// <summary>
        /// 请求的方法
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 参数类型集合
        /// </summary>
        public List<string> ArgTypes { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary> 
        public List<object> Args { get; set; }

        /// <summary>
        /// 链路上下文
        /// </summary>
        public TraceContext TraceContext { get; set; }
    }
}
