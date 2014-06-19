using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSynClient
{
    public class Node
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path;
        /// <summary>
        /// 节点类型appSettings connectionStrings
        /// </summary>
        public string Type;
        /// <summary>
        /// 客户端配置节点名称
        /// </summary>
        public string Name;
        /// <summary>
        /// DbKey
        /// </summary>
        public string DbKey;
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string ServerIp;
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DbName;
        /// <summary>
        /// 帐号
        /// </summary>
        public string Uid;
        /// <summary>
        /// 密码
        /// </summary>
        public string Pwd;
        /// <summary>
        /// 端口
        /// </summary>
        public int Port;
        /// <summary>
        /// 格式化串（含四个可用参数{0}sip  {1}sport {2}dbname  {3}uid    {4}pwd）
        /// </summary>
        public string FormatString;
        /// <summary>
        /// 字符串加密密钥
        /// </summary>
        public byte[] CryptoKey;
        /// <summary>
        /// 加密类型(aes des)
        /// </summary>
        public Crypto.CryptoType CryptoType;
        /// <summary>
        /// 字符串格式化类型(base64 encoding)
        /// </summary>
        public Crypto.CryptoStringFormat CryptoStringFormat;
        /// <summary>
        /// 初始化向量
        /// </summary>
        public string CryptoIV;
        /// <summary>
        /// 密文替换表(如@替换成+等等) 数据格式(@|+,%|-)
        /// </summary>
        public string CryptoReplace;
        /// <summary>
        /// 是否改变了
        /// </summary>
        public bool Changed;
        public Node Clone() {
            Node n = new Node();
            n.Changed = this.Changed;
            n.DbKey = this.DbKey;
            n.DbName = this.DbName;
            n.FormatString = this.FormatString;
            n.Name = this.Name;
            n.Path = this.Path;
            n.Port = this.Port;
            n.Pwd = this.Pwd;
            n.ServerIp = this.ServerIp;
            n.Type = this.Type;
            n.Uid = this.Uid;
            n.CryptoKey = this.CryptoKey;
            n.CryptoStringFormat = this.CryptoStringFormat;
            n.CryptoType = this.CryptoType;
            n.CryptoIV = this.CryptoIV;
            return n;
        }
    }
}
