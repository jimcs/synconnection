using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Windows.Forms;
namespace DbSynClient
{
    public partial class Main : Form//ServiceBase
    {
        public Main()
        {
            InitializeComponent();
        }



        #region 线程相关
        private static Thread commandThread = new Thread(Monitor.CommandStart);
        private static Thread logThread = new Thread(Monitor.LogStart);
        private static Thread configThread = new Thread(Monitor.AppConfigStart);
        private static Thread synThread = new Thread(Syn.SynClient);
        #endregion
        protected void OnStart(string[] args)
        {
            logThread.Start();
            Config.LoadClientConfig("client.config");            
            commandThread.Start();
            configThread.Start();
            Sockets.ConnectServer();
            synThread.Start();
        }


        protected void OnStop()
        {
            commandThread.Abort();
            configThread.Abort();
        }



        private void Main_Load(object sender, EventArgs e)
        {
            OnStart(null);
        }
    }
}
