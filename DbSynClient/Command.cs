using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
namespace DbSynClient
{

    /// <summary>
    /// 指令集
    /// </summary>
    public class Command
    {
        public enum CType
        {
            Send,
            Connect,
            SendFailed
        }
        #region 字段
        public CType CommandType;
        public string CommandText;
        #endregion
        public static Queue<Command> commands = new Queue<Command>();
        /// <summary>
        /// 获取下一个指令
        /// </summary>
        /// <returns></returns>
        public static Command Next()
        {
            if (commands.Count > 0)
                return commands.Dequeue();
            return null;
        }
        public static void AddCommand(Command cmd)
        {
            commands.Enqueue(cmd);
        }
        /// <summary>
        /// 解析指令
        /// </summary>
        public static void Resolve(byte[] buff, int len)
        {
            if (len - 1 < 0) { return; }
            Command cmd = new Command();
            byte[] data = new byte[len - 1];
            Array.Copy(buff, 1, data, 0, data.Length);
            cmd.CommandText = Crypto.Encoding.GetString(data);
            try
            {
                if (cmd.CommandText.EndsWith("}"))
                {
                    JObject obj = JObject.Parse(cmd.CommandText);
                    ConData cdata = obj.ToObject<ConData>();
                    if (buff[0] == GetCode(data))
                    {
                        if (cdata.Result)
                        {
                            Log.WriteLine("收到新消息：" + cdata.ToJson());
                            Node n = Config.Nodes[cdata.ServerIp + cdata.DbKey];
                            if (n != null)
                            {
                                if (string.IsNullOrEmpty(n.Uid))
                                {
                                    n.Changed = true;

                                }
                                else
                                {
                                    if (n.Port != cdata.Port || n.Pwd != cdata.Pwd || n.Uid != cdata.Uid)
                                    {
                                        n.Changed = true;
                                    }
                                }
                                n.Port = cdata.Port;
                                n.DbName = cdata.DbName;
                                n.Pwd = cdata.Pwd;
                                n.Uid = cdata.Uid;
                            }
                            else
                            {
                                Log.WriteLine("未在Nodes中找到对应存储:" + cdata.ToJson());
                            }
                        }
                        else
                        {
                            Log.WriteLine("收到失败数据：" + cdata.ToJson());
                        }
                    }
                    else
                    {
                        Log.WriteLine("通讯数据被篡改", LogType.Error);
                    }
                }
                else
                {
                    Log.WriteLine("通讯数据格式错误：" + cmd.CommandText);
                }
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
            buff = null;
        }
        public static byte GetCode(byte[] buff)
        {
            byte code = 0;
            foreach (byte c in buff)
            {
                code += c;
            }
            code += code;
            return code;
        }
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="cmd"></param>
        public static void ProcessCommand(Command cmd)
        {
            switch (cmd.CommandType)
            {
                case Command.CType.Connect:
                    if (Static.IsConnecting == null)
                    {
                        Sockets.ConnectServer();
                    }
                    break;
                case Command.CType.Send:
                    byte[] buff = System.Text.Encoding.Default.GetBytes(cmd.CommandText);
                    byte code = Command.GetCode(buff);
                    byte[] send_buff = new byte[buff.Length + 1];
                    Array.Copy(buff, 0, send_buff, 1, buff.Length);
                    send_buff[0] = code;
                    Sockets.SendData(Static.CurrentSocket, send_buff);
                    buff = null;
                    send_buff = null;
                    break;
                case Command.CType.SendFailed:
                    Sockets.SendData(Static.CurrentSocket, System.Text.Encoding.Default.GetBytes(cmd.CommandText));
                    break;
            }
        }
    }
}
