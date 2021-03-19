using System;
using System.Collections.Concurrent;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 负载管理
    /// </summary>
    class Workloads
    {
        /// <summary>
        /// 请求超时设置，单位秒
        /// </summary>
        public const int RequestTimeoutSeconds = 60;

        /// <summary>
        /// [MessageId /MessageResponse]
        /// </summary>
        private ConcurrentDictionary<string, Workload> _workloads = new ConcurrentDictionary<string, Workload>();

        /// <summary>
        /// 构造函数
        /// </summary> 
        public Workloads()
        {
        }

        /// <summary>
        /// 添加响应
        /// </summary>
        /// <param name="workload">负载元数据</param> 
        public void Add(Workload workload)
        {
            if (workload == null)
            {
                return;
            }
            _workloads.TryAdd(workload.MessageId, workload);
        }

        /// <summary>
        /// 释放负载
        /// </summary>
        /// <param name="messageId">消息id</param> 
        public void Release(string messageId)
        {
            if (_workloads.ContainsKey(messageId))
            {
                _workloads.TryRemove(messageId, out _);
            }
        }

        /// <summary>
        /// 当前负载压力
        /// </summary> 
        public int Count()
        {
            return _workloads.Count;
        }
    }

    /// <summary>
    /// 负载元数据
    /// </summary>
    class Workload
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime ReceiveTime { get; set; } 
    }
}
