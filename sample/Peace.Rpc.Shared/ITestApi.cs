using Peace.Core.Rpc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peace.Rpc.Shared
{
    public interface ITestApi : IEndPoint
    {  
        void TestVoid(string data);
          
        Task<string> TestGetDataAsync(string data);

        Task TestTaskAsync(string data);

        string TestGetData(string data);

        string TestGetData(int i, string data);

        string TestGetData(string data, int i);

        Dictionary<string, int> TestGetDataWithParameters(string p, object obj);
    } 
}
