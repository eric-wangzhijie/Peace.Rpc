using Peace.Core.Rpc;
using Peace.Core.Rpc.Grpc;
using Peace.Rpc.Shared;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Peace.Rpc.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ////使用redis作为注册中心的模式
            //  GrpcChannel client = new GrpcChannel(-1, "1", null, "127.0.0.1:6379, password=first@2020--, poolsize=50, writeBuffer=102400, preheat=false", new TraceManager("test1"));

            //使用直接连接模式 类似Echo ,如果使用这种模式需要保整终结点api为无参构造函数，也可以自行改造
            GrpcChannel client = new GrpcChannel("127.0.0.1", 5000, "1", null, new TraceManager("test1"));
            ITestServiceClient testServiceClient = client.GetClient<ITestServiceClient>("TestService");

            for (; ; )
            {
                Console.WriteLine("请输入传输数据：");
                string data = Console.ReadLine();
                if (string.IsNullOrEmpty(data))
                {
                    continue;
                }
                for (int j = 0; j < 1; j++)
                {
                    Task.Run(async () =>
                   {
                       for (int i = 0; i < 1; i++)
                       {
                           try
                           {
                               Stopwatch stopwatch = Stopwatch.StartNew();

                               await testServiceClient.TestApi.TestTaskAsync(data);

                               Console.WriteLine(DateTime.Now + "  " + data + " Time spend:" + stopwatch.ElapsedMilliseconds + "ms");
                           }
                           catch (Exception ex)
                           {
                               Console.WriteLine(ex);
                           }
                       }
                   });
                    Thread.Sleep(500);
                }
            }
        }
    }
}
