namespace Peace.Core.Rpc
{
    /// <summary>
    /// 消息体基类
    /// </summary> 
    public abstract class MessageBase
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public string MessageId { get; set; }
    }
}