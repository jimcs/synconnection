using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Configuration;
using System.Net;
using System.Threading;
namespace DbSynClient
{
    public class Sockets
    {
        #region 服务器配置
        /// <summary>
        /// 服务器地址
        /// </summary>
        private static string sHost = string.Empty;
        /// <summary>
        /// 服务器端口
        /// </summary>
        private static int sPort = 0;
        #endregion
        #region 通讯配置
        /// <summary>
        /// 缓冲字节长度
        /// </summary>
        private static int buffLen = 1024;
        #endregion
        /// <summary>
        /// 连接同步服务器
        /// <param name="host">服务器</param>
        /// <param name="port">端口</param>
        /// </summary>
        private static void ConnectServer(string host, int port)
        {
            if (Static.IsConnecting == true)
            {
                return;
            }
            Static.IsConnecting = true;
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipa;
            if (!IPAddress.TryParse(host, out ipa))
            {
                Log.WriteLine("服务器地址错误", LogType.Error);
                return;
            }
            Log.WriteLine(string.Format("准备连接服务器{0}:{1}", host, port));
            s.BeginConnect(ipa, port, new AsyncCallback(ConnectCallback), s);
        }

        /// <summary>
        /// 连接同步服务器
        /// </summary>
        public static void ConnectServer()
        {
            if (sPort == 0)
            {
                sHost = ConfigurationManager.AppSettings["ServerHost"];
                sPort = ConfigurationManager.AppSettings["ServerPort"].ToInt32();
            }
            if (string.IsNullOrEmpty(sHost) || sPort == 0)
            {
                Log.WriteLine("未找到配置节点ServerHost,ServerPort或配置节点数据错误", LogType.Error);
                return;
            }
            ConnectServer(sHost, sPort);
        }
        /// <summary>
        /// 连接回调
        /// </summary>
        /// <param name="ar"></param>
        private static void ConnectCallback(IAsyncResult ar)
        {
            Socket s = ar.AsyncState as Socket;
            if (s != null && s.Connected)
            {
                Static.IsConnecting = null;
                Static.CurrentSocket = s;
                Log.WriteLine("连接服务器成功，准备接收数据。", LogType.Log);
                Receive(s);
            }
            else
            {
                Static.IsConnecting = false;
                Log.WriteLine("连接服务器失败,30秒后重试。" + Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(30 * 1000);
                ConnectServer();
            }
        }
        private static void Receive(Socket s)
        {
            try
            {
                byte[] buff = new byte[buffLen];
                ObjState state = new ObjState() { b = buff, s = s };
                s.BeginReceive(buff, 0, buffLen, SocketFlags.None, ReceiveCallback, state);
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
        }
        /// <summary>
        /// 接收回调
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                ObjState state = ar.AsyncState as ObjState;
                if (state != null)
                {
                    Socket s = state.s;
                    int len = s.EndReceive(ar);
                    if (len > 0)
                    {
                        byte[] bytes = state.b;
                        state = null;
                        Command.Resolve(bytes, len);
                        bytes = null;
                        Receive(s);

                       
                    }
                }
                else
                {
                    Log.WriteLine("接收消息失败");
                }
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
        }

        public static void SendData(Socket s, byte[] buff)
        {
            try
            {
                if (s.Connected)
                {
                    s.BeginSend(buff, 0, buff.Length, SocketFlags.None, SendCallback, s);
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
            Command.AddCommand(new Command() { CommandText = System.Text.Encoding.Default.GetString(buff), CommandType = Command.CType.SendFailed });
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket s = ar.AsyncState as Socket;
                if (s != null && s.Connected)
                {
                    int len = s.EndSend(ar);
                }
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
        }
    }
}
