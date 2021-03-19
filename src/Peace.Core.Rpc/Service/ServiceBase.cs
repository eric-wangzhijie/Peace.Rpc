using System;
using System.Threading;
using System.Threading.Tasks;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 服务启动接口类
    /// </summary> 
    public abstract class ServiceBase
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public abstract string ServiceName { get; }

        /// <summary>
        /// 服务类型
        /// </summary>
        public abstract ServiceType Type { get; }

        /// <summary>
        /// 服务状态
        /// </summary>
        public ServiceStatus Status { get; internal set; } = ServiceStatus.Unavailable;

        /// <summary>
        /// 当前后台作业执行类型
        /// </summary>
        public abstract DeamonType DeamonType { get; }

        /// <summary>
        ///  后台作业
        /// </summary>
        protected abstract Task Deamon(int deamonId);

        private int @lock = 0;

        internal async Task DeamonCore(int deamonId)
        {
            switch (DeamonType)
            {
                case DeamonType.Unspecified:
                    break;
                case DeamonType.SingleThread:
                    if (Interlocked.CompareExchange(ref @lock, 1, 0) != 1)
                    {
                        await Deamon(deamonId);
                        Interlocked.Exchange(ref @lock, 0);
                    }
                    break;
                case DeamonType.MutilThread:
                    await Deamon(deamonId);
                    break;
                default:
                    throw new NotImplementedException();
            }
            await Deamon(deamonId);
        }
    }
}