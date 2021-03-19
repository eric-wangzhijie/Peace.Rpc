using System;
using System.Collections.Generic;
using System.Text;

namespace Peace.Common
{
    /// <summary>
    /// 日志内容
    /// </summary>
    public class TextLog
    {
        /// <summary>
        /// 响应
        /// </summary>
        public const string ResponseBodyKey = "ResponseBody";

        /// <summary>
        /// 请求参数
        /// </summary>
        public const string RequestParams = "RequestParams";

        /// <summary>
        /// 链路id
        /// </summary>
        public const string TraceIdKey = "TraceId";

        /// <summary>
        /// 方法名称
        /// </summary>
        public const string MethodNameKey = "MethodName";

        /// <summary>
        /// 耗时
        /// </summary>
        public const string ElapsedMillisecondsKey = "ElapsedMilliseconds";

        /// <summary>
        /// 日志内容
        /// </summary>
        public const string ContentKey = "Content";

        /// <summary>
        /// 日志级别
        /// </summary>
        public const string LogLevelKey = "LogLevel";

        /// <summary>
        /// 服务Id
        /// </summary>
        public const string ServiceIdKey = "ServiceId";

        /// <summary>
        /// 日志源
        /// </summary>
        public const string SourceKey = "Source";

        /// <summary>
        /// 日志源
        /// </summary>
        public const string UserIdKey = "UserId";

        /// <summary>
        /// 异常信息
        /// </summary>
        public const string ExceptionKey = "Exception";

        /// <summary>
        /// 记录时间
        /// </summary>
        public const string LogTimeKey = "LogTime";

        /// <summary>
        /// 租户id
        /// </summary>
        public const string TenantCodeKey = "TenantCode";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="time">日志时间</param>
        /// <param name="logs">日志内容</param> 
        public TextLog(DateTime time, Dictionary<string, object> logs)
        {
            Time = time;
            Content = logs;
        }

        /// <summary>
        /// 日志时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 日志内容
        /// </summary>
        public Dictionary<string, object> Content { get; set; }

        /// <summary>
        /// 转换成字符串
        /// </summary>
        /// <returns>日志内容</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.Time.ToString("yyyy-MM-dd HH:mm:ss.ffff") + ": ");

            foreach (string key in this.Content.Keys)
            {
                string value = Utility.ConvertToString(this.Content[key]);
                builder.Append(Environment.NewLine);
                builder.Append("\t" + key + ": " + value);
            }
            return builder.ToString();
        }
    }
}
