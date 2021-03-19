using Newtonsoft.Json;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Peace.Common
{
    public class Utility
    {
        /// <summary>
        /// 检查目录
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void EnsureFileDirectory(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || filePath.LastIndexOf(Path.DirectorySeparatorChar) == filePath.Length - 1)
            {
                throw new ArgumentNullException();
            }
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
        }

        /// <summary>
        /// 将一个值转换成字符串
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static string ConvertToString(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            else if (obj is string)
            {
                return (string)obj;
            }
            else if (obj is DateTime)
            {
                return ((DateTime)obj).ToString("yyyy-MM-dd HH:mm:ss.ffff");
            }
            else if (obj is byte[])
            {
                return ((byte[])obj).Length + string.Empty;
            }
            else if (obj is Exception || obj is Enum || obj is int || obj is long || obj is short || obj is double || obj is float || obj is uint || obj is ulong)
            {
                return obj.ToString();
            }
            else if (obj is IEnumerable)
            {
                StringBuilder builder = new StringBuilder();
                if (obj is IDictionary)
                {
                    IDictionary dict = (IDictionary)obj;
                    foreach (object key in dict.Keys)
                    {
                        object dictValue = dict[key];
                        string s_key = ConvertToString(key);
                        string s_value = ConvertToString(dictValue);
                        builder.Append(s_key + ": " + s_value + Environment.NewLine);
                    }
                    return builder.ToString();
                }
                else
                {
                    int index = 0;
                    foreach (object item in (IEnumerable)obj)
                    {
                        string s_item = ConvertToString(item);
                        builder.Append(index + ": " + s_item + Environment.NewLine);
                        index++;
                    }
                    return builder.ToString();
                }
            }
            else if (obj is DataColumn)
            {
                return ((DataColumn)obj).ColumnName;
            }
            else if (obj is DataRow row)
            {
                return ConvertToString(row.ItemArray);
            }
            else if (obj is DataTable table)
            {
                return ConvertToString(table.Columns) + ": " + ConvertToString(table.Rows);
            }
            else
            {
                return JsonConvert.SerializeObject(obj);
            }
        }

        /// <summary>
        /// 获取本机ip
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIp()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                var ip = addressList.FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork)?.ToString();
                return ip;
            }
            return NetworkInterface.GetAllNetworkInterfaces()
                .Select(p => p.GetIPProperties())
                .SelectMany(p => p.UnicastAddresses)
                .FirstOrDefault(p => p.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(p.Address))?.Address.ToString();
        }
    }
}
