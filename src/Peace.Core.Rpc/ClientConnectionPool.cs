using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 服务连接池
    /// </summary>
    class ClientConnectionPool
    {
        /// <summary>
        /// 控制往注册中心获取数据的频率，防止造成注册中心的压力过大
        /// </summary>
        private const int RemoveFaildServiceIntervalSeconds = 5 * 60;

        /// <summary>
        /// 每间隔3分钟刷新所有当前已经发现的服务列表
        /// </summary>
        private const int RefreshAllIntervalSeconds = 3 * 60;

        /// <summary>
        /// [services / address list]
        /// </summary>
        private readonly Dictionary<string, Queue<ClientConnection>> _peers = new Dictionary<string, Queue<ClientConnection>>();

        /// <summary>
        /// [services / refresh time]
        /// </summary>
        private readonly Dictionary<string, DateTime> _serviceRefreshTime = new Dictionary<string, DateTime>();
        private readonly object _lockObject = new object();
        private readonly ServiceDiscovery _serviceDiscovery;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="redisConnection">注册中心redis连接地址</param> 
        public ClientConnectionPool(string redisConnection, int version)
        {
            _serviceDiscovery = new ServiceDiscovery(redisConnection, version);
        }

        /// <summary> 
        /// 获取一个的连接
        /// </summary>
        /// <param name="serviceMetadata">服务元数据</param> 
        /// <returns>一个可用的服务地址</returns>
        public ClientConnection Ensure(ServiceMetadata serviceMetadata)
        {
            lock (_lockObject)
            {
                string key = GetServiceKey(serviceMetadata);
                if (!_peers.ContainsKey(key))
                {
                    Refresh(serviceMetadata);
                }
                else
                {
                    Queue<ClientConnection> q = _peers[key];
                    ClientConnection conn = q.Dequeue();

                    //如果服务请求失败长达3分钟仍未恢复则拿掉该服务地址
                    if (conn.LastFailedTime != DateTime.MinValue && DateTime.Now.Subtract(conn.LastFailedTime).TotalSeconds > RefreshAllIntervalSeconds)
                    {
                        return null;
                    }

                    q.Enqueue(conn);
                    return conn;
                }
                return null;
            }
        }

        /// <summary> 
        /// 获取可用的服务连接数量
        /// </summary>
        /// <param name="serviceMetadata">服务元数据</param>  
        /// <returns>一个可用的服务地址</returns>
        public int GetCount(ServiceMetadata serviceMetadata)
        {
            lock (_lockObject)
            {
                string key = GetServiceKey(serviceMetadata);
                int total = 0;
                if (!_peers.ContainsKey(key))
                {
                    Refresh(serviceMetadata);
                }
                else
                {
                    total = _peers[key].Count;
                }
                return total;
            }
        }

        /// <summary>
        /// 移除一个服务的连接
        /// </summary>
        /// <param name="serviceMetadata">服务元数据</param>
        public void Remove(ServiceMetadata serviceMetadata)
        {
            lock (_lockObject)
            {
                string key = GetServiceKey(serviceMetadata);
                if (_peers.ContainsKey(key))
                {
                    _peers.Remove(key);
                    _serviceRefreshTime.Remove(key);
                }
            }
        }

        private void Refresh(ServiceMetadata serviceMetadata)
        {
            string key = GetServiceKey(serviceMetadata);

            //如果服务发现失败长达5分钟仍未恢复则移除该服务并跳过该服务检查，防止雪崩
            if (_serviceRefreshTime.ContainsKey(key) && DateTime.Now.Subtract(_serviceRefreshTime[key]).TotalSeconds > RemoveFaildServiceIntervalSeconds)
            {
                Remove(serviceMetadata);
                return;
            }

            List<ServiceMetadata> list = _serviceDiscovery.GetService(serviceMetadata).Result;
            if (list != null && list.Any())
            {
                Queue<ClientConnection> clientConnections = new Queue<ClientConnection>();
                foreach (ServiceMetadata foundService in list)
                {
                    clientConnections.Enqueue(new ClientConnection
                    {
                        Available = true,
                        Host = foundService.Host,
                        Port = foundService.Port,
                        ServiceId = foundService.Id
                    });
                }
                if (_peers.ContainsKey(key))
                {
                    _peers[key] = clientConnections;
                    _serviceRefreshTime[key] = DateTime.Now;
                }
                else
                {
                    _peers.Add(key, clientConnections);
                    _serviceRefreshTime.Add(key, DateTime.Now);
                }
            }
        }

        private string GetServiceKey(ServiceMetadata serviceMetadata)
        {
            return serviceMetadata.ServiceName;
        }

        private DateTime _now = DateTime.Now;

        /// <summary>
        /// 更新所有可用服务地址
        /// </summary> 
        public Task RefreshAll()
        {
            if (DateTime.Now.Subtract(_now).TotalSeconds < RefreshAllIntervalSeconds)
            {
                return Task.CompletedTask;
            }
            _now = DateTime.Now;

            lock (_lockObject)
            {
                var serviceKeys = _peers.Select(s => s.Key);
                if (serviceKeys != null && serviceKeys.Any())
                {
                    foreach (string serviceKey in serviceKeys)
                    {
                        Refresh(new ServiceMetadata() { ServiceName = serviceKey });
                    }
                }
                return Task.CompletedTask;
            }
        }
    }
}
