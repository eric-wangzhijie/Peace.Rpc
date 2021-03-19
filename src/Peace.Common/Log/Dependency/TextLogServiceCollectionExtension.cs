using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;

namespace Peace.Common
{
    /// <summary>
    /// 日志服务扩展
    /// </summary>
    public static class TextLogServiceCollectionExtension
    {
        private static ILog _log;

        /// <summary>
        /// 注入多租户服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="options">配置参数</param>
        public static IServiceCollection AddFileLog(this IServiceCollection services, TextLogOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            string path = options.Path;
            if (string.IsNullOrEmpty(path))
            {
                path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
            }
            _log = new FileTextLogger(true, options.ServiceId, options.Source, path);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            services.TryAddSingleton<ILog>(_log);
            return services;
        } 

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Dictionary<string, object> logs = new Dictionary<string, object>();
            logs.Add(TextLog.LogTimeKey, DateTime.Now);
            try
            {
                Exception exception = (Exception)args.ExceptionObject;
                logs.Add(TextLog.ExceptionKey, exception);
                logs.Add(nameof(args.IsTerminating), args.IsTerminating);
            }
            catch (Exception ex)
            {
                logs.Add(TextLog.ExceptionKey, ex);
            }
            finally
            {
                _log.WriteErrorLog(logs);
            }
            //确保日志写入
            System.Threading.Thread.Sleep(2000);
        }
    }
}
