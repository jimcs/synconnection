using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DbSynClient
{
    public class Config
    {
        /// <summary>
        /// 需要同步的节点列表
        /// </summary>
        public static Dictionary<string, Node> Nodes = new Dictionary<string, Node>();
        /// <summary>
        /// 客户端编号
        /// </summary>
        public static readonly int AppId = ConfigurationManager.AppSettings["AppId"].ToInt32();

        /// <summary>
        /// 客户端私钥
        /// </summary>
        public static readonly string _privateKey = Crypto.AES.Decrypt(ConfigurationManager.AppSettings["AppKey"]);
        public static byte[] PrivateKey
        {
            get
            {
                if (_privateKey != null) { return Crypto.Encoding.GetBytes(_privateKey); }
                return null;
            }
        }
        private XmlDocument doc = new XmlDocument();
        private string path;
        public Config(string path)
        {
            try
            {
                this.path = path;
                doc.Load(path);
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
        }
        public void Set(string type, string key, string value)
        {
            try
            {
                switch (type)
                {
                    case "appSettings":
                        XmlNode nodeAS = doc.SelectSingleNode("configuration/appSettings");
                        if (nodeAS != null)
                        {
                            XmlNode item = nodeAS.SelectSingleNode("add[@key='" + key + "']");
                            if (item == null)
                            {
                                item = doc.CreateElement("add");
                                XmlAttribute attKey = doc.CreateAttribute("key");
                                XmlAttribute attValue = doc.CreateAttribute("value");
                                attKey.Value = key;
                                attValue.Value = value;
                                item.Attributes.Append(attKey);
                                item.Attributes.Append(attValue);
                                nodeAS.AppendChild(item);
                            }
                            else
                            {
                                XmlAttribute att = item.Attributes["value"];
                                if (att != null)
                                {
                                    att.Value = value;
                                }
                            }
                        }
                        break;
                    case "connectionStrings":
                        XmlNode nodeCS = doc.SelectSingleNode("configuration/connectionStrings");
                        if (nodeCS != null)
                        {
                            XmlNode item = nodeCS.SelectSingleNode("add[@name='" + key + "']");
                            if (item == null)
                            {
                                item = doc.CreateElement("add");
                                XmlAttribute attKey = doc.CreateAttribute("name");
                                XmlAttribute attValue = doc.CreateAttribute("connectionString");
                                attKey.Value = key;
                                attValue.Value = value;
                                item.Attributes.Append(attKey);
                                item.Attributes.Append(attValue);
                                nodeCS.AppendChild(item);
                            }
                            else
                            {
                                XmlAttribute att = item.Attributes["connectionString"];
                                if (att != null)
                                {
                                    att.Value = value;
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
        }
        /// <summary>
        /// 获取相关节点数据
        /// </summary>
        /// <param name="type">appSettings connectionStrings</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string type, string key)
        {
            try
            {
                switch (type)
                {
                    case "appSettings":
                        XmlNode nodeAS = doc.SelectSingleNode("configuration/appSettings");
                        if (nodeAS != null)
                        {
                            XmlNode item = nodeAS.SelectSingleNode("add[@key='" + key + "']");
                            if (item == null)
                            {
                                return null;
                            }
                            else
                            {
                                XmlAttribute att = item.Attributes["value"];
                                if (att != null)
                                {
                                    return att.Value;
                                }
                            }
                        }
                        break;
                    case "connectionStrings":
                        XmlNode nodeCS = doc.SelectSingleNode("configuration/connectionStrings");
                        if (nodeCS != null)
                        {
                            XmlNode item = nodeCS.SelectSingleNode("add[@name='" + key + "']");
                            if (item == null)
                            {
                                return null;
                            }
                            else
                            {
                                XmlAttribute att = item.Attributes["connectionString"];
                                if (att != null)
                                {
                                    return att.Value;
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
            return null;
        }
        /// <summary>
        /// 保存文档
        /// </summary>
        public void Save()
        {
            try
            {
                FileInfo file = new FileInfo(this.path);
                bool readOnly = false;
                if (file.IsReadOnly)
                {
                    readOnly = true;
                    file.IsReadOnly = false;
                }
                doc.Save(this.path);
                if (readOnly)
                {
                    file.IsReadOnly = true;
                }
                file = null;
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
        }

        /// <summary>
        /// 加载客户端需同步的节点配置文件
        /// </summary>
        /// <param name="path"></param>
        public static void LoadClientConfig(string path)
        {

            Log.WriteLine("开始加载客户端配置文件");
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList xnl = doc.SelectNodes("/configuration/node");
                foreach (XmlNode node in xnl)
                {
                    Node nd = new Node();
                    nd.DbKey = GetNodeValue(node, "dbkey");
                    nd.FormatString = GetNodeValue(node, "format");
                    nd.Name = GetNodeValue(node, "name");
                    nd.ServerIp = GetNodeValue(node, "sip");
                    nd.Path = GetNodeValue(node, "path");
                    nd.Type = GetNodeValue(node, "type");
                    nd.CryptoReplace = GetNodeValue(node, "cryptoreplace");
                    nd.CryptoType = (GetNodeValue(node, "cryptotype") == "aes" ? Crypto.CryptoType.AES : Crypto.CryptoType.DES);
                    string cryptokey = GetNodeValue(node, "cryptokey");
                    cryptokey = Crypto.AES.Decrypt(cryptokey, Config.PrivateKey);
                    if (cryptokey != null)
                    {
                        nd.CryptoKey = Crypto.Encoding.GetBytes(cryptokey);
                    }
                    nd.CryptoIV = GetNodeValue(node, "cryptoiv");
                    nd.CryptoStringFormat = (GetNodeValue(node, "cryptostringformat") == "base64" ? Crypto.CryptoStringFormat.Base64 : Crypto.CryptoStringFormat.Encoding);
                    Nodes.Add(nd.ServerIp + nd.DbKey, nd);
                }
                Log.WriteLine("加载客户端配置文件完毕");
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
        }
        private static string GetNodeValue(XmlNode node, string item)
        {
            XmlNode tem = node.SelectSingleNode(item);
            if (tem != null)
            {
                if (tem.FirstChild != null)
                    return tem.FirstChild.Value;
            }
            return "";
        }
    }
}
