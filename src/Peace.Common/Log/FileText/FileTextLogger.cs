using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Peace.Common
{
    /// <summary>
    /// 将日志写到本地的日志文件管理器
    /// </summary>
    public class FileTextLogger : TextLogger
    {
        private readonly string _logPath;
        private readonly object _lock = new object();

        //删除日志的开始时间
        private readonly DateTime _deleteStartTime = Convert.ToDateTime("00:01");

        //下次执行删除日志的时间
        private DateTime _nextDeleteLogTime;

        //执行删除日志操作的间隔时间
        private const int IntervalDeleteLogDays = 1;

        //删除日志间隔时间
        private const int IntervalDeleteLogMonths = 1;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="autoWrite">是否自动写日志</param>
        /// <param name="serviceId">服务id</param>
        /// <param name="source">源</param>
        /// <param name="logPath">日志路径</param>
        /// <param name="autoDelete">是否自动删日志</param>
        public FileTextLogger(bool autoWrite, string serviceId, string source, string logPath)
            : base(autoWrite, serviceId, source)
        {
            this._logPath = logPath;
        }

        /// <summary>
        ///持久化日志
        /// </summary>
        /// <returns>处理结果</returns>
        protected override bool Flush()
        {
            TextLog[] logs = this.LogCache.TakeAll();
            if (logs == null || logs.Length == 0)
            {
                return false;
            }

            try
            {
                lock (this._lock)
                {
                    this.Write(logs);

                    this.DeleteLog(); //删除日志
                }
            }
            catch
            {
                // 如果出现异常则忽略 
            }

            return true;
        }

        /// <summary>
        /// 持久化日志到本地文件
        /// </summary>
        private void Write(TextLog[] logs)
        {
            DateTime now = DateTime.UtcNow;
            string logFilePath = Path.Combine(this._logPath, now.Year + "-" + now.Month + "-" + now.Day + ".txt");
            Utility.EnsureFileDirectory(this._logPath);

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (TextLog log in logs)
            {
                if (builder.Length > 0)
                {
                    builder.Append(Environment.NewLine);
                }

                builder.Append(log.ToString());
            }

            string text = builder.ToString();

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(text);
            }
        }

        /// <summary>
        /// 执行删日志操作
        /// </summary>
        private void DeleteLog()
        {
            if (_nextDeleteLogTime > DateTime.Now)
            {
                return;
            }
            Delete();
            //更新下次执行时间
            _nextDeleteLogTime = _nextDeleteLogTime == DateTime.MinValue
                ? _deleteStartTime.AddDays(IntervalDeleteLogDays)
                : _nextDeleteLogTime
                    .AddDays(IntervalDeleteLogDays);
        }

        /// <summary>
        /// 删除日志
        /// </summary>
        private void Delete()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_logPath);
            IEnumerable<FileInfo> fileInfos = directoryInfo.GetFiles()
                .Where(x =>
                    x.Extension.Equals(".txt") &&
                    x.LastWriteTime <= DateTime.Now.AddMonths(-IntervalDeleteLogMonths));
            foreach (var fileInfo in fileInfos)
            {
                fileInfo.Delete();
            }
        }
    }
}