using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DbSynClient
{
    /// <summary>
    /// 监控类
    /// </summary>
    public class Monitor
    {
        private static void PushCommand(Command cmd)
        {
            Command.ProcessCommand(cmd);
        }
        /// <summary>
        /// 监控App.Config的改动
        /// </summary>
        public static void AppConfigStart()
        {
            Log.WriteLine("AppConfig监控启动");
            using (FileSystemWatcher fsw = new FileSystemWatcher(AppDomain.CurrentDomain.BaseDirectory))
            {
                fsw.Filter = "client.config";
                fsw.EnableRaisingEvents = true;
                fsw.Changed += fsw_Changed;
            }
        }

        static void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
                Config.LoadClientConfig("client.config");//重新加载配置文件
        }
        public static void LogStart()
        {
            Log.WriteLine("日志线程启动");
            while (true)
            {
                Log.ProcessLog();
                Thread.Sleep(10);
            }
        }
        /// <summary>
        /// 监控指令的变动
        /// </summary>
        public static void CommandStart()
        {
            Log.WriteLine("指令监控启动");
            while (true)
            {
                if (Static.CurrentSocket != null && Static.CurrentSocket.Connected)
                {
                    Command cmd = Command.Next();
                    if (cmd != null)
                        PushCommand(cmd);
                }
                else
                {
                    if (Static.IsConnecting == null)
                    {
                        Command cmd = new Command();
                        cmd.CommandType = Command.CType.Connect;
                        cmd.CommandText = "Connect";
                        PushCommand(cmd);
                    }
                }
                Thread.Sleep(500);
            }
        }
    }
}
