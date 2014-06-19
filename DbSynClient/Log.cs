using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSynClient
{
    public class Log
    {
        public string Message;
        public Exception Ex;
        public LogType Type;
        private static Queue<Log> logs = new Queue<Log>();
        private static readonly string logPath = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 处理日志
        /// </summary>
        public static void ProcessLog()
        {
            try
            {
                if (logs.Count == 0) { return; }
                Log l = logs.Dequeue();
                if (l != null)
                {
                    string msg = string.Format("{0} LogType：{1} Message:{2}{3}\r\n", DateTime.Now.ToString(), l.Type, l.Ex == null ? "" : l.Ex.Message + l.Ex.StackTrace, l.Message);
                    string path = logPath + "/logs";
                    if (!System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }
                    System.IO.File.AppendAllText(logPath + "/logs/" + DateTime.Now.ToString("yyyyMMdd") + ".log", msg);


                }
            }
            catch (Exception ex)
            {
                try
                {
                    string eventSourceName = "DbSynClient";
                    if (!EventLog.SourceExists(eventSourceName))
                    {
                        EventLog.CreateEventSource(eventSourceName, "Application");
                    }
                    using (EventLog el = new EventLog())
                    {
                        el.Source = eventSourceName;
                        el.WriteEntry("写入文件日志时出现错误：" + ex.Message, EventLogEntryType.Error);
                    }
                }
                catch { }
            }
        }
        
        private static void PushLog(Log log)
        {
            logs.Enqueue(log);
        }
        public static void WriteLine(string msg)
        {
            WriteLine(msg, LogType.Log);
        }
        public static void WriteLine(string msg, LogType type)
        {
            PushLog(new Log() { Message = msg, Type = type });
        }
        public static void WriteException(Exception ex)
        {
            PushLog(new Log() { Type = LogType.Exception, Ex = ex });
        }
    }
    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogType
    {
        Log,
        Error,
        Exception
    }
}
