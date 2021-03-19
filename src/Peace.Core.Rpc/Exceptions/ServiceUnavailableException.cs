using System;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 服务不可用
    /// </summary>
    class ServiceUnavailableException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        public ServiceUnavailableException(string message) : base(message)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ServiceUnavailableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
