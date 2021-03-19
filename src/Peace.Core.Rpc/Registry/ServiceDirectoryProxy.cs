using Peace.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 服务目录
    /// </summary>
    class ServiceDirectoryProxy
    {
        private const int ServiceEchoTTLSeconds = 10;
        private const string ServiceDirectoryPrefix = "ServiceDirectory";

        private readonly IRedisManager _redisManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="redisConnection">redis</param>
        public ServiceDirectoryProxy(string redisConnection)
        {
            _redisManager = new RedisManager(redisConnection);
        }

        /// <summary>
        /// 注销服务实例
        /// </summary>
        /// <param name="serviceDesc">服务描述</param>
        public async Task Deregister(ServiceMetadata serviceDesc)
        {
            string key = GetKey(serviceDesc.ServiceName, serviceDesc.Id);
            await _redisManager.DeleteAsync(key).ConfigureAwait(false);
        }

        /// <summary>
        /// 注册服务实例
        /// </summary> 
        /// <param name="serviceDesc">要注册的服务实例</param>
        public async Task Register(ServiceMetadata serviceDesc)
        {
            string key = GetKey(serviceDesc.ServiceName, serviceDesc.Id);
            await _redisManager.SetAsync(key, serviceDesc, ServiceEchoTTLSeconds).ConfigureAwait(false);
        }

        /// <summary>
        /// 根据条件查询服务的实例
        /// </summary>  
        /// <param name="serviceName">服务的名称</param>
        /// <returns>服务实例的实例列表，格式是(小写的服务实例Id，服务实例)</returns>
        public async Task<List<ServiceMetadata>> LookFor(string serviceName)
        {
            string key = GetKey(serviceName, string.Empty);
            string pattern = key + "*";
            string[] keys = await _redisManager.GetKeysByPatternAsync(pattern).ConfigureAwait(false);
            if (keys != null && keys.Any())
            {
                List<ServiceMetadata> list = new List<ServiceMetadata>();
                string[] values = await _redisManager.GetByKeysAsync(keys);
                foreach (string josn in values)
                {
                    ServiceMetadata serviceDesc = JsonSerializer.Deserialize<ServiceMetadata>(josn);
                    list.Add(serviceDesc);
                }
                return list;
            }
            return null;
        }

        /// <summary>
        /// 根据条件查询服务的实例
        /// </summary>  
        /// <param name="serviceName">服务的名称</param>
        /// <param name="serviceId">服务的Id</param>
        /// <returns>服务实例的实例列表，格式是(小写的服务实例Id，服务实例)</returns>
        public async Task<ServiceMetadata> LookFor(string serviceName, string serviceId)
        {
            string key = GetKey(serviceName, serviceId);
            return await _redisManager.GetAsync<ServiceMetadata>(key).ConfigureAwait(false);
        }

        private string GetKey(string serviceName, string serviceId)
        {
            return $"{ServiceDirectoryPrefix}_{serviceName}_{serviceId}";
        }
    }
}
