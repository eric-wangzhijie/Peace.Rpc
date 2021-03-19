using Microsoft.Extensions.Configuration;
using System.IO;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 读取配置文件
    /// </summary>
    /// <typeparam name="T">配置项实体</typeparam>
    public class Config<T> where T : class, new()
    {
        private static T _settings;

        /// <summary>
        /// 读取配置文件[AppSettings]节点数据
        /// </summary>
        public static T Items
        {
            get
            {
                if (_settings == null)
                {
                    IConfiguration Configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .Build();

                    _settings = new T();
                    Configuration.GetSection(typeof(T).Name).Bind(_settings);
                }
                return _settings;
            }
        }
    }
}
