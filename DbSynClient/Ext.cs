using DbSynClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Ext
{
    /// <summary>
    /// 转换为32位整数，失败返回0
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static int ToInt32(this string s)
    {
        return ToInt32(s, 0);
    }
    /// <summary>
    /// 转换位32位整数，失败返回参数def的值
    /// </summary>
    /// <param name="s"></param>
    /// <param name="def"></param>
    /// <returns></returns>
    public static int ToInt32(this string s, int def)
    {
        if (string.IsNullOrEmpty(s)) { return def; }
        int v = 0;
        if (Int32.TryParse(s, out v))
        {
            return v;
        }
        return def;
    }
    /// <summary>
    /// 获取有变更的节点
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public static Dictionary<string, Node> GetChanged(this Dictionary<string, Node> n)
    {
        Dictionary<string, Node> changedNode = new Dictionary<string, Node>();
        foreach (KeyValuePair<string, Node> each in Config.Nodes)
        {
            if (each.Value.Changed)
            {
                changedNode.Add(each.Key, each.Value.Clone());
            }
        }
        return changedNode;
    }
}
