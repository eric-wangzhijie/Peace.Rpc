using CSRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Peace.Common
{
    /// <summary>
    /// redis管理
    /// </summary>
    public class RedisManager : IRedisManager
    {
        /// <summary>
        /// 仅不存在时允许操作
        /// </summary>
        public const string Nx = "Nx";

        /// <summary>
        /// 仅存在时允许操作
        /// </summary>
        public const string Xx = "Xx";

        private readonly CSRedisClient _redisClient;
        private readonly string _prefix;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connStr">redis链接字符串</param> 
        public RedisManager(string connStr)
        {
            if (string.IsNullOrEmpty(connStr))
            {
                throw new ArgumentNullException(nameof(connStr));
            }
            //初始化 _redisClient
            _redisClient = new CSRedisClient(connStr);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connStr">redis链接字符串</param>
        /// <param name="prefix">当多进程公用同个redis时候，为了区分不同进程使用同个类型key的场景，给key补充特定的前缀，也可以不指定</param>
        public RedisManager(string connStr, string prefix)
        {
            if (string.IsNullOrEmpty(connStr))
            {
                throw new ArgumentNullException(nameof(connStr));
            }
            _prefix = prefix;
            //初始化 _redisClient
            _redisClient = new CSRedisClient(connStr);
        }

        /// <summary>
        /// 是否存在指定的key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>key对应的value</returns>
        public async Task<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            key = WrapperKeyWithPrefix(new[] { key }).First();
            return await _redisClient.ExistsAsync(key).ConfigureAwait(false);
        }

        /// <summary>
        /// 获取单个key对应的value
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>key对应的value</returns>
        public async Task<string> GetAsync(string key)
        {
            key = WrapperKeyWithPrefix(new[] { key }).First();
            return await _redisClient.GetAsync(key).ConfigureAwait(false);
        }

        /// <summary>
        /// 获取单个key对应的对象
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>key对应的value</returns>
        public async Task<T> GetAsync<T>(string key)
        {
            key = WrapperKeyWithPrefix(new[] { key }).First();
            return await _redisClient.GetAsync<T>(key).ConfigureAwait(false);
        }

        /// <summary>
        /// 批量获取keys对应的values
        /// </summary>
        /// <param name="keys">keys</param>
        /// <returns>keys对应的values</returns>
        public async Task<string[]> GetByKeysAsync(string[] keys)
        {
            if (keys == null || !keys.Any())
            {
                return null;
            }
            keys = WrapperKeyWithPrefix(keys);
            return await _redisClient.MGetAsync(keys).ConfigureAwait(false);
        }

        /// <summary>
        /// 查找所有分区节点中符合给定模式(pattern)的 key
        /// </summary>
        /// <param name="pattern">模糊匹配的模板</param>
        /// <returns></returns>
        public async Task<string[]> GetKeysByPatternAsync(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return null;
            }
            pattern = WrapperKeyWithPrefix(new[] { pattern }).First();
            return await _redisClient.KeysAsync(pattern).ConfigureAwait(false);
        }

        /// <summary>
        /// 同时设置一个或多个 key-value 对
        /// </summary>
        /// <param name="keyValues">键值对的集合</param>
        /// <returns></returns>
        public async Task<bool> SetKeyValuesAsync(Dictionary<string, object> keyValues)
        {
            if (keyValues == null || !keyValues.Any())
            {
                return false;
            }
            List<object> list = new List<object>();
            foreach (KeyValuePair<string, object> kv in keyValues)
            {
                string key = WrapperKeyWithPrefix(new[] { kv.Key }).First();
                list.Add(key);
                list.Add(kv.Value);
            }
            return await _redisClient.MSetAsync(list.ToArray()).ConfigureAwait(false);
        }

        /// <summary>
        /// 设置键值对到redis中
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expireSeconds">自动过期时间，单位秒，默认值是-1表示永不过期</param>
        /// <param name="exists">指定'Nx'则当key不存在的时候才插入，指定Xx则当key存在的时候才插入</param>
        /// <returns>key对应的value</returns>
        public async Task<bool> SetAsync(string key, object value, int expireSeconds = -1, string exists = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            key = WrapperKeyWithPrefix(new[] { key }).First();
            if (!string.IsNullOrEmpty(exists))
            {
                RedisExistence? redisExistence = null;
                if (string.Compare(exists, RedisExistence.Nx.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    redisExistence = RedisExistence.Nx;
                }
                else if (string.Compare(exists, RedisExistence.Xx.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    redisExistence = RedisExistence.Xx;
                }
                return await _redisClient.SetAsync(key, value, expireSeconds, redisExistence);
            }
            else
            {
                return await _redisClient.SetAsync(key, value, expireSeconds, null).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 只有在 key 不存在时设置 key 的值
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>key对应的value</returns>
        public async Task<bool> SetIfNotExsitAsync(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            key = WrapperKeyWithPrefix(new[] { key }).First();
            return await _redisClient.SetNxAsync(key, value).ConfigureAwait(false);
        }

        /// <summary>
        /// 更新redis中的键值对
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expireSeconds">自动过期时间，单位秒，默认值是-1表示永不过期</param> 
        /// <returns>key对应的value</returns>
        public async Task<bool> UpdateAsync(string key, object value, int expireSeconds = -1)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            key = WrapperKeyWithPrefix(new[] { key }).First();
            return await _redisClient.SetAsync(key, value, expireSeconds, RedisExistence.Xx).ConfigureAwait(false);
        }

        /// <summary>
        /// 删除存在的key
        /// </summary>
        /// <param name="keys">key集合</param>
        /// <returns>true或者false</returns>
        public async Task<long> DeleteAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return 0;
            }
            key = WrapperKeyWithPrefix(new[] { key }).First();
            return await _redisClient.DelAsync(new string[] { key }).ConfigureAwait(false);
        }

        /// <summary>
        /// 删除存在的key集合
        /// </summary>
        /// <param name="keys">key集合</param>
        /// <returns>true或者false</returns>
        public async Task<long> DeleteAsync(string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                return 0;
            }
            keys = WrapperKeyWithPrefix(keys);
            return await _redisClient.DelAsync(keys).ConfigureAwait(false);
        }

        /// <summary>
        /// 执行lua脚本
        /// </summary>
        /// <param name="script">脚本内容</param>
        /// <param name="key">key</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public async Task<object> ExecLuaAsync(string script, string key, params string[] args)
        {
            key = WrapperKeyWithPrefix(new[] { key }).First();
            return await _redisClient.EvalAsync(script, key, args).ConfigureAwait(false);
        }

        private string[] WrapperKeyWithPrefix(string[] keys)
        {
            List<string> newKeys = new List<string>();
            foreach (string key in keys)
            {
                newKeys.Add(_prefix + key);
            }
            return newKeys?.ToArray();
        }
    }
}
