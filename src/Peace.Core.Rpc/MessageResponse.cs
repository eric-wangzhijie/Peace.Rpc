namespace Peace.Core.Rpc
{
    /// <summary>
    /// 请求响应
    /// </summary> 
    public class MessageResponse : MessageBase
    {
        /// <summary>
        /// 是否请求成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 返回值类型
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// 响应数据
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }
    }
}
