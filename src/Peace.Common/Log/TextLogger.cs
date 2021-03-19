using System;
using System.Collections.Generic;
using System.Threading;

namespace Peace.Common
{
    /// <summary>
    /// 日志管理器
    /// </summary>
    public abstract class TextLogger : ILog
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="autoWrite">是否自动写日志</param> 
        /// <param name="serviceId">服务id</param>
        /// <param name="source">源</param>
        protected TextLogger(bool autoWrite, string serviceId, string source)
        {
            this.Source = source;
            this.ServiceId = serviceId;

            // 启动写日志的线程
            if (autoWrite)
            {
                Thread writeLogThread = new Thread(() => { this.WriteLog(); });
                writeLogThread.Name = "TextLoggerThread";
                writeLogThread.Start();
            }
        }

        /// <summary>
        /// 服务Id
        /// </summary>
        protected string ServiceId { get; set; }

        /// <summary>
        /// 源
        /// </summary>
        protected string Source { get; set; }

        /// <summary>
        /// 日志缓存
        /// </summary>
        protected ConcurrentList<TextLog> LogCache { get; set; } = new ConcurrentList<TextLog>();

        /// <summary>
        /// 写入普通日志
        /// </summary>
        /// <param name="text"></param>
        public void WriteInfoLog(string text)
        {
            Dictionary<string, object> logs = new Dictionary<string, object>();
            logs.Add(TextLog.ContentKey, text);
            WriteInfoLog(logs);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="logs">日志键值对</param>
        public void WriteInfoLog(Dictionary<string, object> logs)
        {
            Log(LogLevel.Info, logs);
        }

        /// <summary>
        /// 写入警告日志
        /// </summary>
        /// <param name="text"></param>
        public void WriteWarnLog(string text)
        {
            Dictionary<string, object> logs = new Dictionary<string, object>();
            logs.Add(TextLog.ContentKey, text);
            WriteWarnLog(logs);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="logs">日志键值对</param>
        public void WriteWarnLog(Dictionary<string, object> logs)
        {
            Log(LogLevel.Warn, logs);
        }

        /// <summary>
        /// 写入异常日志
        /// </summary>
        /// <param name="ex">异常信息</param>
        public void WriteErrorLog(Exception ex)
        {
            Dictionary<string, object> logs = new Dictionary<string, object>();
            logs.Add(TextLog.ExceptionKey, ex);
            WriteErrorLog(logs);
        }

        /// <summary>
        /// 写入异常日志
        /// </summary>
        /// <param name="text">异常信息</param>
        public void WriteErrorLog(string text)
        {
            Dictionary<string, object> logs = new Dictionary<string, object>();
            logs.Add(TextLog.ContentKey, text);
            WriteErrorLog(logs);
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="logs">日志键值对</param>
        public void WriteErrorLog(Dictionary<string, object> logs)
        {
            if (logs.ContainsKey(TextLog.ExceptionKey) && logs[TextLog.ExceptionKey] != null)
            {
                Exception exception = logs[TextLog.ExceptionKey] as Exception;
                if (exception.Data.Count > 0)
                {
                    Dictionary<string, int> counter = new Dictionary<string, int>();
                    foreach (System.Collections.DictionaryEntry de in exception.Data)
                    {
                        string key = de.Key.ToString();
                        if (logs.ContainsKey(key))
                        {
                            if (counter.ContainsKey(key))
                            {
                                counter[key] = counter[key] + 1;
                            }
                            else
                            {
                                counter.Add(key, 0);
                            }

                            key = key + "_" + counter[key].ToString();
                        }

                        logs.Add(key, de.Value);
                    }
                }
            }

            Log(LogLevel.Error, logs);
        }

        /// <summary>
        /// 持久化日志
        /// </summary>
        /// <returns>结果</returns>
        protected abstract bool Flush();

        /// <summary>
        /// 写日志
        /// </summary>
        private void WriteLog()
        {
            while (true)
            {
                // 写日志
                if (!this.Flush())
                {
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// 输出日志
        /// </summary> 
        /// <param name="logs"></param> 
        private void Log(LogLevel logLevel, Dictionary<string, object> logs)
        {
            DateTime now = DateTime.Now;
            if (!logs.ContainsKey(TextLog.LogTimeKey))
            {
                logs.Add(TextLog.LogTimeKey, now);
            }

            if (!logs.ContainsKey(TextLog.LogLevelKey))
            {
                logs.Add(TextLog.LogLevelKey, logLevel);
            }

            if (!logs.ContainsKey(TextLog.ServiceIdKey))
            {
                logs.Add(TextLog.ServiceIdKey, ServiceId);
            }

            if (!logs.ContainsKey(TextLog.SourceKey))
            {
                logs.Add(TextLog.SourceKey, Source);
            }

            TextLog textLog = new TextLog(now, logs);
            this.LogCache.Add(textLog);
            Console.WriteLine(textLog.ToString());
        }
    }
}