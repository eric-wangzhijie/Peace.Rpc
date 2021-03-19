using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 服务发现
    /// </summary>
    class ServiceDiscovery
    {
        private readonly ServiceDirectoryProxy _directory;
        private readonly int _version;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="redisConnection">注册中心redis连接地址</param> 
        /// <param name="version">服务实例版本</param>
        public ServiceDiscovery(string redisConnection, int version)
        {
            _version = version;
            _directory = new ServiceDirectoryProxy(redisConnection);
        }

        /// <summary>
        /// 根据条件查询服务的实例
        /// </summary>  
        /// <param name="serviceMetadata">服务元数据</param>
        /// <returns>服务实例的实例列表，格式是(小写的服务实例Id，服务实例)</returns>
        public async Task<List<ServiceMetadata>> GetService(ServiceMetadata serviceMetadata)
        {
            List<ServiceMetadata> versionSMs = new List<ServiceMetadata>();
            List<ServiceMetadata> lowVersionSMs = new List<ServiceMetadata>();
            var foundServices = await _directory.LookFor(serviceMetadata.ServiceName);
            if (foundServices == null)
            {
                return null;
            }
            foreach (ServiceMetadata foundService in foundServices)
            { 
                if (foundService.Version == _version)
                {
                    versionSMs.Add(foundService);
                }
                if (foundService.Version == -1)
                {
                    lowVersionSMs.Add(foundService);
                }
            }
            if (versionSMs.Any())
            {
                return versionSMs;
            }
            return lowVersionSMs;
        }
    }
}
