using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Peace.Common;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 存根
    /// </summary>
    public abstract class StubBase
    {
        /// <summary>
        /// [终结点类型name/终结点反射信息]
        /// </summary>
        private readonly Dictionary<string, EndPointReflectionInfo> _endPointInfos = new Dictionary<string, EndPointReflectionInfo>();
        private bool workThreadsStopped = false;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger">日志</param> 
        public StubBase(ILog logger)
        {
            Functions = new Dictionary<int, Func<Task>>();
            Logger = logger;
            Thread thread = new Thread(async () => { await Deamon(); })
            {
                IsBackground = true,
                Name = "StubBase"
            };
            thread.Start();
        }

        /// <summary>
        /// 日志
        /// </summary>
        public ILog Logger { get; }

        /// <summary>
        /// 后台执行的委托,会开一个线程去处理，函数的优先级由key决定，key越大越先执行
        /// </summary>
        public Dictionary<int, Func<Task>> Functions { get; }

        /// <summary>
        /// 获取服务信息
        /// </summary>
        /// <param name="type">服务类型</param>
        /// <returns>服务信息</returns>
        public void AddEndPointInfo(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (_endPointInfos.ContainsKey(type.Name))
            {
                throw new ServiceInternalException($"The endpoint {type.Name} has registed.");
            }
            _endPointInfos.Add(type.Name, EndPointReflectionInfo.GetEndPointInfo(type));
        }

        /// <summary>
        /// 获取服务信息
        /// </summary>
        /// <param name="typeName">服务类型</param>
        /// <returns>服务信息</returns>
        public EndPointReflectionInfo GetEndPointInfo(string typeName)
        {
            if (_endPointInfos.ContainsKey(typeName))
            {
                return _endPointInfos[typeName];
            }
            return null;
        }

        /// <summary>
        /// 获取服务信息
        /// </summary>
        /// <param name="type">服务类型</param>
        /// <returns>服务信息</returns>
        public EndPointReflectionInfo GetOrSetApiInfo(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (!_endPointInfos.ContainsKey(type.Name))
            {
                AddEndPointInfo(type);
            }
            return _endPointInfos[type.Name];
        }

        /// <summary>
        /// 终止所有的工作线程
        /// </summary>
        public void StopRenewThreads()
        {
            workThreadsStopped = true;
        }

        private async Task Deamon()
        {
            while (true)
            {
                if (workThreadsStopped)
                {
                    break;
                }
                try
                {
                    if (Functions.Any())
                    {
                        foreach (var kv in Functions.OrderByDescending(o => o.Key))
                        {
                            await kv.Value();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteErrorLog(ex);
                }
                await Task.Delay(1000);
            }
        }
    }
}