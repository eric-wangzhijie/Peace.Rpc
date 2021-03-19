using System;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 远程调用异常
    /// </summary>
    class ServiceInternalException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="content"></param>
        public ServiceInternalException(string content) : base(content)
        {
        }
    }
}
