using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 客户端基类
    /// </summary>
    internal abstract class AbstractEndPoint<TProtocol> : DynamicObject, IEndPoint
    {
        private const int FailedConnectionRetryIntervalSeconds = 10;
        private readonly ClientStub _clientStub;
        private readonly EndPointReflectionInfo _endPointInfo;
        private readonly string _serviceName; 

        /// <summary>
        /// 构造函数
        /// </summary> 
        /// <param name="serviceName">服务名称</param> 
        /// <param name="clientStub">客户端存根</param>
        /// <param name="type">终结点对象类型</param>
        internal AbstractEndPoint(string serviceName, ClientStub clientStub, Type type)
        { 
            _serviceName = serviceName;
            _clientStub = clientStub;
            _endPointInfo = _clientStub.GetOrSetApiInfo(type);
        }

        /// <summary>
        /// 序列化消息请求体
        /// </summary>
        /// <param name="obj">消息请求体</param>
        /// <returns>消息协议泛型</returns>
        protected abstract TProtocol SerializeMessage(MessageRequest obj);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="input">序列化后的消息体</param>
        /// <returns>消息响应体</returns>
        protected abstract MessageResponse DeserializeMessage(TProtocol input);

        /// <summary>
        /// 请求调用
        /// </summary>
        /// <param name="messageId">消息Id</param>
        /// <param name="message">序列化后的消息体</param>
        /// <param name="endPoint">远程服务端地址</param> 
        /// <returns></returns>
        protected abstract MessageResponse Invoke(string messageId, TProtocol message, IPEndPoint endPoint);

        /// <summary>
        /// 调用入口
        /// </summary>
        /// <param name="binder">请求的绑定对象</param>
        /// <param name="args">参数集合</param>
        /// <param name="result">返回结果</param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            //处理参数  
            MethodReflectionInfo methodInfo = _endPointInfo.EnsureMethodInfo(binder.Name, args, null, binder.CallInfo.ArgumentCount);

            bool isExistedTraceContext = _clientStub.TraceChain.GetCurrentTraceContext() != null;
            //序列化消息数据
            var request = new MessageRequest
            {
                MessageId = Guid.NewGuid().ToString(),
                TraceContext = isExistedTraceContext ? _clientStub.TraceChain.GetCurrentTraceContext() : _clientStub.TraceChain.Begin(),
                Service = _serviceName,
                EndPoint = _endPointInfo.EndPointType.Name,
                Method = binder.Name,
                ClusterToken = _clientStub.ClusterToken, 
                ArgTypes = methodInfo.Parameters.Any() ? new List<string>() : null,
                Args = methodInfo.Parameters.Any() ? new List<object>() : null
            };

            for (int i = 0; i < methodInfo.Parameters.Length; i++)
            {
                request.ArgTypes.Add(methodInfo.Parameters[i].ParameterType.AssemblyQualifiedName); //todo 但继承的子类不在同个程序集的时候无法反射
                request.Args.Add(args[i]);
            }

            TProtocol message = SerializeMessage(request);
            result = null;
            if (_clientStub.EnableServiceDiscovery)
            {
                bool success = InvokeTolerably(request.MessageId, message, out Exception lastException, out object ret);
                if (!success && lastException != null)
                {
                    throw lastException;
                }
                result = ret;
            }
            else
            {
                MessageResponse response = Invoke(request.MessageId, message, _clientStub.EndPoint);
                result = response.Data;
            }

            if (methodInfo.IsAwaitable) //处理异步返回值
            {
                if (methodInfo.ReturnType.IsGenericType)
                {
                    Type genericParamType = methodInfo.Method.ReturnType.GenericTypeArguments.First();
                    dynamic value = Utility.ChangeType(result, genericParamType);
                    result = Utility.GetTaskByType(genericParamType, value);
                }
                else
                {
                    result = Task.CompletedTask;
                }
            }

            //完成链路
            if (!isExistedTraceContext)
            {
                _clientStub.TraceChain.Finish();
            }
            return true;
        }

        private bool InvokeTolerably(string messageId, TProtocol message, out Exception lastException, out object ret)
        {
            ret = default;
            lastException = null;
            ServiceMetadata serviceMetadata = new ServiceMetadata() { ServiceName = _serviceName };
            int total = _clientStub.GetConnectionCount(serviceMetadata);
            if (total == 0)
            {
                lastException = new ServiceNotFoundException($"No any available address for {_serviceName}");
            }

            //多尝试一次获取连接
            for (int i = 0; i < total + 1; i++)
            {
                ClientConnection conn = null;
                try
                {
                    conn = _clientStub.EnsureConnection(serviceMetadata);
                    if (conn != null)
                    {
                        // 检查是否过了重试时间
                        if (!conn.Available && DateTime.Now.Subtract(conn.LastFailedTime).TotalSeconds < FailedConnectionRetryIntervalSeconds)
                        {
                            continue;
                        }
                        //等待返回
                        MessageResponse response = Invoke(messageId, message, new IPEndPoint(IPAddress.Parse(conn.Host), conn.Port));
                        ret = response.Data;
                        conn.Available = true;
                        lastException = null;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is RpcConnectException) //连接失败进行重试
                    {
                        conn.Available = false;
                        conn.LastFailedTime = DateTime.Now;
                    }
                    if (ex is ServiceUnavailableException)//服务端变为不可用状态移除连接
                    {
                        _clientStub.RemoveConnection(serviceMetadata);
                    }
                    lastException = ex;
                }
            }
            return false;
        }
    }
}
