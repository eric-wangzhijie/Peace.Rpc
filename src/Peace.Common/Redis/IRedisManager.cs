using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peace.Common
{
    /// <summary>
    /// redis管理器接口
    /// </summary>
    public interface IRedisManager
    {
        /// <summary>
        /// 获取单个key对应的value
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>key对应的value</returns>
        Task<string> GetAsync(string key);

        /// <summary>
        /// 获取单个key对应的v对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// 是否存在指定的key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>key对应的value</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 批量获取keys对应的values
        /// </summary>
        /// <param name="keys">keys</param>
        /// <returns>keys对应的values</returns>
        Task<string[]> GetByKeysAsync(string[] keys);

        /// <summary>
        /// 查找所有分区节点中符合给定模式(pattern)的 key
        /// </summary>
        /// <param name="pattern">模糊匹配的模板</param>
        /// <returns></returns>
        Task<string[]> GetKeysByPatternAsync(string pattern);

        /// <summary>
        /// 同时新增一个或多个 key-value 对
        /// </summary>
        /// <param name="keyValues">键值对集合</param>
        /// <returns></returns>
        Task<bool> SetKeyValuesAsync(Dictionary<string, object> keyValues);

        /// <summary>
        /// 设置键值对到redis中
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expireSeconds">自动过期时间，单位秒，默认值是-1表示永不过期</param>
        /// <param name="exists">指定'Nx'则当key不存在的时候才插入，指定Xx则当key存在的时候才插入</param>
        /// <returns>key对应的value</returns>
        Task<bool> SetAsync(string key, object value, int expireSeconds = -1, string exists = null);

        /// <summary>
        /// 只有在 key 不存在时设置 key 的值
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>key对应的value</returns>
        Task<bool> SetIfNotExsitAsync(string key, object value);

        /// <summary>
        /// 删除存在的key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>true或者false</returns>
        Task<long> DeleteAsync(string key);

        /// <summary>
        /// 删除存在的key集合
        /// </summary>
        /// <param name="keys">key集合</param>
        /// <returns>true或者false</returns>
        Task<long> DeleteAsync(string[] keys);

        /// <summary>
        /// 执行lua脚本
        /// </summary>
        /// <param name="script">脚本内容</param>
        /// <param name="key">key</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        Task<object> ExecLuaAsync(string script, string key, params string[] args);
    }
}