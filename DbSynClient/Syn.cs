using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DbSynClient
{
    public class Syn
    {
        /// <summary>
        /// 每30分钟发起一次同步请求
        /// </summary>
        public static void SynClient()
        {
            foreach (KeyValuePair<string, Node> each in Config.Nodes)
            {
                Node n = each.Value;
                Command cmd = new Command();
                cmd.CommandType = Command.CType.Send;
                ConData data = new ConData() { Message = "", Port = 0, Pwd = "", Result = false, Uid = "" };
                data.AppId = Config.AppId;
                data.DbKey = n.DbKey;
                data.ServerIp = n.ServerIp;
                cmd.CommandText = data.ToJson();
                Command.AddCommand(cmd);
            }
            Thread.Sleep(5 * 60 * 1000);//暂停5分钟，给予时间处理下面的数据接收分析
            Dictionary<string, Node> nodes = Config.Nodes.GetChanged();
            if (nodes != null)
            {
                Log.WriteLine(string.Format("发现{0}个节点可以同步", nodes.Count));
                foreach (KeyValuePair<string, Node> each in nodes)
                {
                    HandleNode(each);
                }
            }
            nodes.Clear();
            Thread.Sleep(25 * 60 * 1000);
            SynClient();
        }

        /// <summary>
        /// 处理单个Node节点
        /// </summary>
        /// <param name="kv"></param>
        private static void HandleNode(KeyValuePair<string, Node> kv)
        {
            try
            {
                string path = kv.Value.Path;
                Config cf = new Config(path);
                string type = kv.Value.Type;
                string name = kv.Value.Name;
                string format = kv.Value.FormatString;
                string oldValue = cf.Get(type, name), newValue = "";
                if (kv.Value.CryptoType == Crypto.CryptoType.AES)
                {
                    newValue = Crypto.AES.Encrypt(string.Format(format, kv.Value.ServerIp, kv.Value.Port, kv.Value.DbName, Crypto.AES.Decrypt(kv.Value.Uid, Config.PrivateKey), Crypto.AES.Decrypt(kv.Value.Pwd, Config.PrivateKey)), kv.Value.CryptoKey, kv.Value.CryptoStringFormat, kv.Value.CryptoIV);
                }
                else if (kv.Value.CryptoType == Crypto.CryptoType.DES)
                {
                    newValue = Crypto.DES.Encrypt(string.Format(format, kv.Value.ServerIp, kv.Value.Port, kv.Value.DbName, Crypto.AES.Decrypt(kv.Value.Uid, Config.PrivateKey), Crypto.AES.Decrypt(kv.Value.Pwd, Config.PrivateKey)), kv.Value.CryptoKey, kv.Value.CryptoStringFormat, kv.Value.CryptoIV);
                }
                else
                {
                    newValue = null;
                }
                if (!string.IsNullOrEmpty(kv.Value.CryptoReplace))
                {
                    string[] strReplace = kv.Value.CryptoReplace.Split(',');
                    foreach (string each in strReplace)
                    {
                        string[] eachReplace = each.Split('|');
                        if (eachReplace.Length == 2) {
                            newValue = newValue.Replace(eachReplace[0], eachReplace[1]);
                        }
                    }
                }
                if (newValue != null && newValue != oldValue)
                {
                    cf.Set(type, name, newValue);
                    cf.Save();
                    Log.WriteLine(string.Format("路径{0}节点{1}同步成功", kv.Value.Path, kv.Value.Name));
                }
                else
                {                    
                    Log.WriteLine(string.Format("路径{0}节点{1}数据相同无需同步", kv.Value.Path, kv.Value.Name));
                }
                Node n = Config.Nodes[kv.Value.ServerIp + kv.Value.DbKey];
                if (n != null)
                {
                    n.Changed = false;//复原标志
                }
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
        }
    }
}
