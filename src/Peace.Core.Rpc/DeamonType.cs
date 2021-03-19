namespace Peace.Core.Rpc
{
    /// <summary>
    /// 执行作业类型
    /// </summary>
    public enum DeamonType
    {
        /// <summary>
        /// 未指定
        /// </summary>
        Unspecified,

        /// <summary>
        /// 单线程处理
        /// </summary>
        SingleThread,

        /// <summary>
        /// 多线程处理
        /// </summary>
        MutilThread
    }
}

