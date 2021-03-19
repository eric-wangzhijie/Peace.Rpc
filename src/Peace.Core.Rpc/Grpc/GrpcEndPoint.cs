using Grpc.Core;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace Peace.Core.Rpc.Grpc
{
    /// <summary>
    /// DotNetty客户端
    /// </summary>
    internal class GrpcEndPoint : AbstractEndPoint<byte[]>
    {
        /// <summary>
        /// [services / refresh time]
        /// </summary>
        private static readonly ConcurrentDictionary<string, Channel> _chancelDic = new ConcurrentDictionary<string, Channel>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="clientStub"></param> 
        internal GrpcEndPoint(string serviceName, ClientStub clientStub, Type type) : base(serviceName, clientStub, type)
        {
        }

        /// <summary>
        /// 请求调用
        /// </summary>
        /// <param name="messageId">消息Id</param>
        /// <param name="message">序列化后的消息体</param>
        /// <param name="endPoint">远程服务地址</param> 
        /// <returns></returns>
        protected override MessageResponse Invoke(string messageId, byte[] message, IPEndPoint endPoint)
        {
            string address = endPoint.Address.ToString() + ":" + endPoint.Port;
            if (!_chancelDic.ContainsKey(address))
            {
                _chancelDic.TryAdd(address, new Channel(endPoint.Address.ToString(), endPoint.Port, SslCredentials.Insecure));
            }
            GlueServiceClient client = new GlueServiceClient(_chancelDic[address]);
            try
            {
                byte[] response = client.Invoke(message, null, DateTime.UtcNow.AddSeconds(Workloads.RequestTimeoutSeconds)).GetAwaiter().GetResult();
                return DeserializeMessage(response);
            }
            catch (Exception ex)
            {
                if (ex is RpcException)
                {
                    RpcException rpcException = ex as RpcException;
                    if (rpcException.StatusCode == StatusCode.Unavailable ||
                        rpcException.StatusCode == StatusCode.NotFound ||
                        rpcException.StatusCode == StatusCode.Unimplemented)
                    {
                        throw new RpcConnectException($"Can not connect to destination: {address}", rpcException);
                    }
                }
                throw;
            }
        }

        /// <summary>
        /// 序列化数据
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected override byte[] SerializeMessage(MessageRequest obj)
        {
            if (obj.ArgTypes != null)
            {
                List<object> list = new List<object>();
                for (int i = 0; i < obj.ArgTypes.Count; i++)
                {
                    list.Add(MessagePackSerializer.Serialize(obj.Args[i], MessagePack.Resolvers.ContractlessStandardResolver.Options));
                }
                obj.Args = list;
            }
            return MessagePackSerializer.Serialize(obj, MessagePack.Resolvers.ContractlessStandardResolver.Options);
        }

        /// <summary>
        /// 反序列化数据
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected override MessageResponse DeserializeMessage(byte[] input)
        {
            MessageResponse ret = MessagePackSerializer.Deserialize<MessageResponse>(input, MessagePack.Resolvers.ContractlessStandardResolver.Options);
            if (!string.IsNullOrEmpty(ret.ReturnType))
            {
                ret.Data = MessagePackSerializer.Deserialize(Type.GetType(ret.ReturnType), (byte[])ret.Data, MessagePack.Resolvers.ContractlessStandardResolver.Options);
            }
            return ret;
        }
    }
}
