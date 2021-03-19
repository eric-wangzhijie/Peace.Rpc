using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Peace.Common;

namespace Peace.Core.Rpc
{
    /// <summary>
    /// 抽象的服务基类
    /// </summary>
    /// <typeparam name="TProtocol">通讯协议类型</typeparam>
    public abstract class AbstractServer<TProtocol> : IServer
    {
        private readonly ServerStub _serverStub;
        private readonly Registry _registry;
        private readonly string _clusterToken;
        private readonly ILog _logger;
        private readonly IList<Thread> _workThreads = new List<Thread>();
        private ServiceMetadata _serviceMetadata;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceMetadata">服务实例的描述</param> 
        /// <param name="clusterToken">集群授权码</param>
        /// <param name="logger">日志</param>
        public AbstractServer(ServiceMetadata serviceMetadata, string clusterToken, ILog logger, string redisConnection = null)
        {
            if (string.IsNullOrEmpty(serviceMetadata.Id))
            {
                throw new ArgumentNullException(nameof(serviceMetadata.Id));
            }
            if (string.IsNullOrEmpty(serviceMetadata.Host))
            {
                throw new ArgumentNullException(nameof(serviceMetadata.Host));
            }
            if (string.IsNullOrEmpty(clusterToken))
            {
                throw new ArgumentNullException(nameof(clusterToken));
            }
            if (serviceMetadata.Port < 0 || serviceMetadata.Port > ushort.MaxValue)
            {
                throw new ArgumentException(nameof(serviceMetadata.Port));
            }
            _logger = logger;
            _serverStub = new ServerStub(serviceMetadata.Host, serviceMetadata.Port, logger);
            if (!string.IsNullOrEmpty(redisConnection))
            {
                _registry = new Registry(redisConnection, logger);
            }
            _clusterToken = clusterToken;
            _serviceMetadata = serviceMetadata;
        }

        /// <summary>
        /// 启动服务核心业务逻辑
        /// </summary>
        protected abstract Task StartCore();

        private int _state = 0;

        /// <summary>
        /// 启动服务
        /// </summary>
        public async Task Start(IServiceProvider serviceProvider = null)
        {
            if (Interlocked.CompareExchange(ref _state, 1, 0) != 1)
            {
                await StartCore();

                if (string.IsNullOrEmpty(_serverStub.Service.ServiceName))
                {
                    throw new ServiceInternalException($"No any service is found.");
                }

                if (_registry != null)
                {
                    _logger.WriteInfoLog("Connecting to registry...");
                    await _registry.Register(_serviceMetadata);
                    _serverStub.Functions.Add(int.MaxValue, Renew);
                }

                //执行初始化过程
                if (serviceProvider != null)
                {
                    InvokeMethod(serviceProvider, "OnInit");
                }
                if (_serverStub.Service.DeamonType != DeamonType.Unspecified)
                {
                    for (int i = 0; i < Environment.ProcessorCount; i++)
                    {
                        Thread thread = new Thread(async () => { await OnDeamon(); })
                        {
                            Name = "WorkThread",
                            IsBackground = true
                        };
                        thread.Start();
                        _workThreads.Add(thread);
                    }
                }
                _serverStub.Service.Status = ServiceStatus.Health;

                _logger.WriteInfoLog($"Server has been started and listening on {_serverStub.Host}:{_serverStub.Port}");
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public async Task Stop(TimeSpan timeout, IServiceProvider serviceProvider = null)
        {
            //清空服务内部线程
            _serverStub.StopRenewThreads();
            if (_registry != null)
            {
                //从注册中心注销服务实例
                await _registry.Deregister(_serviceMetadata);
            }
            //修改状态为跛脚鸭状态
            _serverStub.Service.Status = ServiceStatus.LameDuck;

            //等待一段时间，让业务和请求处理完成，否则直接结束
            bool allRequestsHandled = false;
            bool buisnessHandled = false;
            Task.Run(async () =>
            {
                while (_serverStub.Workloads.Count() != 0)
                {
                    await Task.Delay(10);
                }
                allRequestsHandled = true;
            });

            Task.Run(() =>
            {
                //执行终止业务 
                if (serviceProvider != null)
                {
                    InvokeMethod(serviceProvider, "OnStopping");
                }
                buisnessHandled = true;
            });
            DateTime startTime = DateTime.Now;
            while (!allRequestsHandled || !buisnessHandled)
            {
                if (DateTime.Now.Subtract(startTime).TotalSeconds < timeout.TotalSeconds)
                {
                    await Task.Delay(10);
                }
            }
        }

        private const int RenewIntervalSeconds = 5;
        private DateTime _renewTime = DateTime.Now;

        private async Task Renew()
        {
            if (DateTime.Now.Subtract(_renewTime).TotalSeconds > RenewIntervalSeconds)
            {
                _renewTime = DateTime.Now;
                await _registry.Renew(_serviceMetadata);
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="input">待序列化消息</param>
        /// <returns>消息响应体</returns>
        protected abstract TProtocol SerializeMessage(MessageResponse input);

        /// <summary>
        /// 序列化消息请求体
        /// </summary>
        /// <param name="buffer">序列化消息</param>
        /// <returns>消息协议泛型</returns>
        protected abstract MessageRequest DeserializeMessage(TProtocol buffer);

        /// <summary>
        /// 请求调用
        /// </summary>
        /// <param name="buffer">已序列化的消息请求</param>
        /// <returns>序列化消息</returns>
        internal async Task<TProtocol> InvokeAsync(TProtocol buffer)
        {
            MessageResponse response = new MessageResponse();
            MessageRequest request = null;
            DateTime? start = null;
            try
            {
                request = DeserializeMessage(buffer);
                //添加负载
                _serverStub.Workloads.Add(new Workload() { MessageId = request.MessageId, ReceiveTime = DateTime.Now });

                if (request.ClusterToken != _clusterToken)
                {
                    throw new IllegalClusterTokenException($"The cluster token({request.ClusterToken}) is illegal.");
                }

                if (_serverStub.Service.Status != ServiceStatus.Health)
                {
                    throw new ServiceUnavailableException($"the service current status is {_serverStub.Service.Status}.");
                }

                MethodReflectionInfo methodInfo = EnsureMethodInfo(request);
                IEndPoint endPoint = _serverStub.GetEndPoint(request.EndPoint);
                start = DateTime.Now;
                WriteLog(null, null, request, null);

                object ret = methodInfo.Method.Invoke(endPoint, request.Args?.ToArray());

                WriteLog(start, DateTime.Now, request, null);

                if (methodInfo.IsAwaitable) //处理异步
                {
                    dynamic task = (Task)ret;
                    if (methodInfo.ReturnType.IsGenericType)
                    {
                        response.Data = await task.ConfigureAwait(false);
                    }
                    else
                    {
                        await task.ConfigureAwait(false);
                    }
                }
                else
                {
                    response.Data = ret;
                }

                response.MessageId = request.MessageId;
                response.ReturnType = response.Data?.GetType()?.AssemblyQualifiedName;
                response.Success = true;
                return SerializeMessage(response);
            }
            catch (Exception ex)
            {
                WriteLog(start, DateTime.Now, request, ex);
            }
            finally
            {
                //释放负载
                _serverStub.Workloads.Release(request.MessageId);
            }
            return SerializeMessage(response);
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="TService">服务接口</typeparam> 
        public IServer UseService<TService>() where TService : ServiceBase, new()
        {
            Type type = typeof(TService);
            if (_serverStub.Service == null)
            {
                _serverStub.Service = Activator.CreateInstance(type) as ServiceBase;
            }
            else
            {
                throw new ServiceInternalException($"The service[{_serverStub.Service.ServiceName}] has registed.");
            }
            return this;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="serviceInstance">服务接口</param> 
        public IServer UseService(ServiceBase serviceInstance)
        {
            if (serviceInstance == null)
            {
                throw new ServiceInternalException($"The service instance can not be null.");
            }

            if (_serverStub.Service == null)
            {
                _serverStub.Service = serviceInstance;
            }
            else
            {
                throw new ServiceInternalException($"The service[{serviceInstance.ServiceName}] has registed.");
            }
            return this;
        }

        /// <summary>
        /// 注册终结点
        /// </summary>
        /// <typeparam name="TIEndpoint">终端接口</typeparam>
        /// <typeparam name="TEndpoint">终端类型</typeparam>
        public IServer RegisterEndPoint<TIEndpoint, TEndpoint>() where TEndpoint : IEndPoint, TIEndpoint where TIEndpoint : IEndPoint
        {
            Type interfaceType = typeof(TIEndpoint);
            _serverStub.AddEndPointInfo(interfaceType);
            _serverStub.AddEndPoint(interfaceType, typeof(TEndpoint));
            return this;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="endPointInterfaceType">服务接口</param>
        /// <param name="endPointType">服务类型</param>
        public IServer RegisterEndPoint(Type endPointInterfaceType, Type endPointType)
        {
            if (!endPointInterfaceType.IsAssignableFrom(endPointType))
            {
                throw new InvalidOperationException($"The type[{endPointType}] don't inherit from type[{endPointInterfaceType}].");
            }
            _serverStub.AddEndPointInfo(endPointInterfaceType);
            _serverStub.AddEndPoint(endPointInterfaceType, endPointType);
            return this;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="endPointInterfaceType">服务接口</param> 
        /// <param name="endPointInstance">服务实例</param> 
        public IServer RegisterEndPoint(Type endPointInterfaceType, IEndPoint endPointInstance)
        {
            if (!endPointInterfaceType.IsAssignableFrom(endPointInstance.GetType()))
            {
                throw new InvalidOperationException($"The type[{endPointInstance.GetType()}] don't inherit from type[{endPointInterfaceType}].");
            }
            _serverStub.AddEndPointInfo(endPointInterfaceType);
            _serverStub.AddEndPoint(endPointInterfaceType, endPointInstance);
            return this;
        }

        private void WriteLog(DateTime? start, DateTime? end, MessageRequest request, Exception ex)
        {
            Dictionary<string, object> logs = new Dictionary<string, object>
            {
                { TextLog.MethodNameKey, $"{request?.EndPoint}/{request?.Method}" },
                { TextLog.LogTimeKey, DateTime.Now },
                { TextLog.TraceIdKey, request?.TraceContext?.TraceId },
                { nameof(TraceContext.SpanId), request?.TraceContext?.SpanId }
            };
            if (end != null)
            {
                logs.Add(TextLog.ElapsedMillisecondsKey, ((DateTime)end).Subtract((DateTime)start).TotalMilliseconds);
            }
            else
            {
                logs.Add(TextLog.RequestParams, request?.Args);
            }
            if (ex != null)
            {
                logs.Add(TextLog.ExceptionKey, ex);
            }
            _logger.WriteInfoLog(logs);
        }

        /// <summary>
        /// 获取服务方法反射信息
        /// </summary>
        /// <param name="request">请求消息</param>
        /// <returns>方法反射信息</returns>
        protected MethodReflectionInfo EnsureMethodInfo(MessageRequest request)
        {
            if (_serverStub.Service.ServiceName != request.Service)
            {
                throw new ServiceInternalException($"The service {request.Service} is not found.");
            }
            var endPointInfo = _serverStub.GetEndPointInfo(request.EndPoint);
            if (endPointInfo == null)
            {
                throw new ServiceInternalException($"The endpoint {request.EndPoint} is not found.");
            }
            List<Type> types = new List<Type>();
            for (int i = 0; i < request.ArgTypes?.Count; i++)
            {
                types.Add(Type.GetType(request.ArgTypes[i]));
            }
            MethodReflectionInfo methodInfo = endPointInfo.EnsureMethodInfo(request.Method, request.Args?.ToArray(), types.ToArray(), request.Args?.Count ?? 0);

            if (methodInfo == null)
            {
                throw new ServiceInternalException($"The parameters of service method {request.Method} is unmatch. Parameters:{(request.ArgTypes == null ? "" : string.Join("; ", request.ArgTypes))}");
            }

            return methodInfo;
        }

        private void InvokeMethod(IServiceProvider serviceProvider, string methodName)
        {
            var methods = _serverStub.Service.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            MethodInfo methodInfo = methods.SingleOrDefault(s => s.Name == methodName);
            if (methodInfo == null)
            {
                return;
            }
            var parameterInfos = methodInfo.GetParameters();
            var parameters = new object[parameterInfos.Length];
            for (var index = 0; index < parameterInfos.Length; index++)
            {
                var parameterInfo = parameterInfos[index];
                try
                {
                    parameters[index] = serviceProvider.GetRequiredService(parameterInfo.ParameterType);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(
                        "Could not resolve a service of type '{0}' for the parameter '{1}' of method '{2}' on type '{3}'.",
                        parameterInfo.ParameterType.FullName,
                        parameterInfo.Name,
                        methodInfo.Name,
                        methodInfo.DeclaringType.FullName), ex);
                }
            }
            methodInfo.Invoke(_serverStub.Service, BindingFlags.DoNotWrapExceptions, binder: null, parameters: parameters, culture: null);
        }

        private int _initialDaemonId = 0;

        /// <summary>
        /// 后台运行作业
        /// </summary>
        internal async Task OnDeamon()
        {
            int daemonId = Interlocked.Increment(ref _initialDaemonId);
            while (true)
            {
                try
                {
                    await _serverStub.Service.DeamonCore(daemonId);
                }
                catch (Exception ex)
                {
                    Dictionary<string, object> logs = new Dictionary<string, object>
                    {
                        {TextLog.MethodNameKey, _serverStub.Service.ServiceName + "." + nameof(OnDeamon)},
                        {TextLog.ExceptionKey, ex }
                    };
                    _logger.WriteErrorLog(logs);
                }
                finally
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}