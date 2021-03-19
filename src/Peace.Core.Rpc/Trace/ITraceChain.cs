namespace Peace.Core.Rpc
{
    /// <summary>
    /// 调用链路接口
    /// </summary>
    public interface ITraceChain
    {
        /// <summary>
        /// 初始化一个链路上下文
        /// </summary> 
        /// <returns></returns>
        TraceContext Begin();

        /// <summary>
        /// 获取当前链路上下文
        /// </summary> 
        /// <returns>当前链路上下文</returns>
        TraceContext GetCurrentTraceContext();

        /// <summary>
        /// 链路结束
        /// </summary> 
        void Finish();
    }
}