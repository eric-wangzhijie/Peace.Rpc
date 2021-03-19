using System;
using System.Collections.Generic;

namespace Peace.Common
{
    /// <summary>
    /// 日志接口
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="log">日志</param>
        void WriteInfoLog(string log);

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="log">日志</param>
        void WriteInfoLog(Dictionary<string, object> log);

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="log">日志</param>
        void WriteWarnLog(string text);

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="log">日志</param>
        void WriteWarnLog(Dictionary<string, object> log);

        /// <summary>
        /// 写异常日志
        /// </summary>
        /// <param name="ex">异常信息</param>
        void WriteErrorLog(Exception ex);

        /// <summary>
        /// 写异常日志
        /// </summary>
        /// <param name="ex">异常信息</param>
        void WriteErrorLog(string text);

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="log">日志</param>
        void WriteErrorLog(Dictionary<string, object> log);
    }
}
