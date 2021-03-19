using System;
using System.Threading;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 链路管理
    /// </summary>
    public class TraceManager : ITraceChain
    {
        private static readonly ThreadLocal<TraceContext> _currentTraceContext = new ThreadLocal<TraceContext>();
        private readonly string _spanId;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="spanId">链路节点id</param>
        public TraceManager(string spanId)
        {
            _spanId = spanId;
        }

        /// <summary>
        /// 初始化一个链路上下文
        /// </summary> 
        /// <returns></returns>
        public TraceContext Begin()
        {
            TraceContext traceContext = new TraceContext
            {
                TraceId = Guid.NewGuid().ToString(),
                SpanId = _spanId
            };
            _currentTraceContext.Value = traceContext;
            return traceContext;
        }

        /// <summary>
        /// 获取当前链路上下文
        /// </summary> 
        /// <returns>当前链路上下文</returns>
        public TraceContext GetCurrentTraceContext()
        {
            return _currentTraceContext.Value;
        }

        /// <summary>
        /// 链路结束
        /// </summary> 
        public void Finish()
        {
            _currentTraceContext.Value = null;
        }
    }
}
