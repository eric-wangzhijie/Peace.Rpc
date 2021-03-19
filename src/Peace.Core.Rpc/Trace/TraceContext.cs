namespace Peace.Core.Rpc
{
    /// <summary>
    /// 调用链路上下文
    /// </summary>
    public class TraceContext
    {
        /// <summary>
        /// 链路Id
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// 链路节点Id，即服务id
        /// </summary>
        public string SpanId { get; set; }
    }
}
