using Peace.Common;
using Peace.Rpc.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Peace.Rpc.Server
{
    public class TestApi : ITestApi
    {
        //private readonly IHttpClientFactory _httpClientFactory;

        //public TestApi(ILog log, IHttpClientFactory httpClientFactory)
        //{
        //    _httpClientFactory = httpClientFactory;
        //}
          
        public string TestGetData(string data)
        { 
            Console.WriteLine(DateTime.Now + "  Receive Data:" + data);
            return data;
        }

        public async Task<string> TestGetDataAsync(string data)
        { 
            Console.WriteLine(DateTime.Now + "  Receive Data:" + data);
            return await Task.FromResult(data);
        } 

        public string TestGetData(int i, string data)
        { 
            Console.WriteLine(DateTime.Now + "  Receive Data:" + data);
            return data;
        }

        public string TestGetData(string data, int i)
        { 
            Console.WriteLine(DateTime.Now + "  Receive Data:" + data);
            return data + "_" + i;
        }

        public Dictionary<string, int> TestGetDataWithParameters(string p, object obj)
        { 
            Dictionary<string, int> sss = new Dictionary<string, int>();
            sss.Add("iiiiasdas", (int)obj);
            return sss;
        }

        public void TestVoid(string data)
        {
        }

        public async Task TestTaskAsync(string data)
        { 
            //var client = _httpClientFactory.CreateClient();
            //var test2 = await client.GetStringAsync("http://www.baidu.com");
            Console.WriteLine(DateTime.Now + $" Receive Data:" + data);
        }
    }
}
