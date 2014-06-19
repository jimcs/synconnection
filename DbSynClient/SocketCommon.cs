using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DbSynClient
{
    public static class Static
    {
        public static Socket CurrentSocket = null;
        public static bool? IsConnecting = null;
    }
    /// <summary>
    /// Socket状态类
    /// </summary>
    public class ObjState
    {
        public Socket s;
        public byte[] b;
    }
    /// <summary>
    /// 通讯数据
    /// </summary>
    public struct ConData
    {
        public int AppId;   //客户端Id
        public string ServerIp; //服务器IP
        public string DbKey;  //Db关键码
        public string DbName;//数据库名称
        public string Uid; //帐号
        public string Pwd;//密码
        public int Port;//端口
        public bool Result;  //操作结果
        public string Message;//消息    
        public string ToJson()
        {
            try
            {
                JObject obj = JObject.FromObject(this);
                if (obj != null)
                {
                    return obj.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
            return "";
        }
    }
}
