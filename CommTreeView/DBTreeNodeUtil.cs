using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace CommTreeView
{
    public class DBTreeNodeUtil
    {
        #region 与数据库打交道的方法

        public static DataTable GetNodeDataTable(string url)
        {
            var sql = @"
                select 
                n_name,
                n_id,
                n_pid,
                ifnull(n_data,'') n_data,
                ifnull(n_order,n_id) n_order,
                ifnull(n_desc,'') n_desc,
                ifnull(n_disable,'0') n_disable,
                ifnull(n_owner,'') n_owner,
                ifnull(n_tester,'') n_tester
                from tree_1
                where ifnull(n_status,'1') != '0' order by ifnull(n_order,_id),n_id
                ";

            var postVars = new NameValueCollection {
                {"0", sql}
            };
            string jsonStr = HttpServicerHelper.CallGoService(url, postVars);

            var nodeList = JsonConvert.DeserializeObject<List<DBTreeNode>>(jsonStr);
            var dt = DataHelper.ListToDataTable<DBTreeNode>(nodeList);

            return dt;
        }

        public static string DeleteCurrNode(string url, TreeNode node)
        {
            var nodeId = node.Name;
            var status = "0";//1显示0不显示（原因可以是删除标志，或禁用标志)
            var postVars = new NameValueCollection
            {
                //更新节点时，如果数据全部没有改变过，则更新行数为0,所以要通过create_time来保证一定能更新
                {"0", "update tree_1 set n_status=:status,create_time=now() where n_id=:id"},
                {"status",status},
                {"id",nodeId}
            };
            string json = HttpServicerHelper.CallGoService(url, postVars);
            return json;
        }

        public static string UpdateCurrNode(string url, TreeNode node)
        {
            var nodeId = node.Name;
            var nodeCaption = node.Text;            
            var nodePid = node.ToolTipText;
            var tagObj = node.Tag as DBTreeNodeTag;

            string json = "";
            if (tagObj != null)
            {
                string jsonDataStr = DataHelper.DictToString(tagObj.TagDict);
                var orderNum = tagObj.OrderNum;
                var nodeDesc = tagObj.NodeDesc;
                var n_disable = tagObj.Disable;
                var n_owner = tagObj.Owner;
                var n_tester = tagObj.Tester;

                var status = "1";//1显示0不显示（原因可以是删除标志，或禁用标志)
                var updateSql = @"
                    update tree_1 set 
                    n_name=:n_name,
                    n_pid=:pid,
                    n_order=:order,
                    n_status=:status,
                    n_data=:data,
                    n_desc=:n_desc,
                    n_disable=:n_disable,
                    n_owner=:n_owner,
                    n_tester=:n_tester,
                    create_time=now() 
                    where n_id=:id
                    ";
                var postVars = new NameValueCollection
                {
                    //更新节点时，如果数据全部没有改变过，则更新行数为0,所以要通过create_time来保证一定能更新
                    {"0", updateSql},
                    {"n_name",nodeCaption},
                    {"status",status},
                    {"id",nodeId},
                    {"pid",nodePid},  
                    {"order",orderNum},
                    {"n_desc",nodeDesc},
                    {"data",jsonDataStr},
                    {"n_disable",n_disable},
                    {"n_owner",n_owner},
                    {"n_tester",n_tester}
                };
                json = HttpServicerHelper.CallGoService(url, postVars);
            }
            
            

            return json;
        }

        public static string InsertNewNode(string url, TreeNode node, int orderNo)
        {
            var nodeId = node.Name;
            var nodeCaption = node.Text;
            var nodePid = node.ToolTipText;
            var tagObj = node.Tag as DBTreeNodeTag;

            var status = "1";//1显示0不显示（原因可以是删除标志，或禁用标志)
            string json = "";
            var orderNum = "";
            var nodeDesc = "";
            var n_disable = "";
            var n_owner = "";
            var n_tester = "";
            var jsonDataStr = "";

            if (tagObj != null)
            {
                jsonDataStr = DataHelper.DictToString(tagObj.TagDict);
                orderNum = tagObj.OrderNum;
                nodeDesc = tagObj.NodeDesc;
                n_disable = tagObj.Disable;
                n_owner = tagObj.Owner;
                n_tester = tagObj.Tester;
            }
            if (orderNum == "")
            {
                orderNum = orderNo.ToString();
            }

            var insertSql = @"
                    insert into tree_1(n_id,n_name,n_pid,n_order,n_status,n_data,n_desc,n_disable,n_owner,n_tester,create_time)
                    values(:id,:n_name,:pid,:order,:status,:data,:n_desc,:n_disable,:n_owner,:n_tester,now())";
            var postVars = new NameValueCollection
                {
                    //更新节点时，如果数据全部没有改变过，则更新行数为0,所以要通过create_time来保证一定能更新
                    {"0", insertSql},
                    {"n_name",nodeCaption},
                    {"status",status},
                    {"id",nodeId},
                    {"pid",nodePid},  
                    {"order",orderNum},
                    {"n_desc",nodeDesc},
                    {"data",jsonDataStr},
                    {"n_disable",n_disable},
                    {"n_owner",n_owner},
                    {"n_tester",n_tester}
                };
            json = HttpServicerHelper.CallGoService(url, postVars);

            return json;
        }


        public static string GetNodeNextId(string url,int addNum)
        {
            var postVars = new NameValueCollection {
                //在最大值+1的基础上再加
                {"0", "select max(n_id)+1+:addNum max_id from tree_1"},
                {"addNum",addNum.ToString()},
            };
            string jsonStr = HttpServicerHelper.CallGoService(url, postVars);
            var dt = JsonConvert.DeserializeObject<DataTable>(jsonStr);//json转datatable就一句话，fk
            var max_id = dt.Rows[0][0].ToString();
            return max_id;
        }


        /// <summary>
        /// 绑定TreeView（利用TreeNodeCollection）
        /// </summary>
        /// <param name="tnc">TreeNodeCollection（TreeView的节点集合）</param>
        /// <param name="pid_val">父id的值</param>
        /// <param name="id">数据库 id 字段名</param>
        /// <param name="pid">数据库 父id 字段名</param>
        /// <param name="text">数据库 文本 字段值</param>
        public static void Bind_Tv(DataTable dt, TreeNodeCollection tnc,
            string pid_val, string id, string pid, string text,
            string data,string order,string desc,string disable,
            string n_owner, string n_tester)
        {
            DataView dv = new DataView(dt);//将DataTable存到DataView中，以便于筛选数据
            

            //确保不会无限递归:以下为三元运算符，如果父id为空，则为构建“父id字段 is null”的查询条件，否则构建“父id字段=父id字段值”的查询条件
            string filter = string.IsNullOrEmpty(pid_val) ? pid + " is null" : string.Format(pid + "='{0}'", pid_val);
            dv.RowFilter = filter;//利用DataView将数据进行筛选，选出相同 父id值 的数据

            foreach (DataRowView drv in dv)
            {
                var newNode = new TreeNode();//建立一个新节点（学名叫：一个实例）
                //这3个属性必须单独设置
                newNode.Name = drv[id].ToString();//id
                newNode.ToolTipText = drv[pid].ToString();//pid
                newNode.Text = drv[text].ToString();//标题

                //顺序号+业务字典数据
                var tagObj = new DBTreeNodeTag();
                var jsonData = drv[data].ToString();
                tagObj.TagDict = DataHelper.StringToDict(jsonData);
                tagObj.OrderNum = drv[order].ToString();
                tagObj.NodeDesc = drv[desc].ToString();
                tagObj.Disable = drv[disable].ToString();
                if (tagObj.Disable == "1")
                {
                    newNode.ForeColor = Color.Gray;
                }

                tagObj.Owner = drv[n_owner].ToString();
                tagObj.Tester = drv[n_tester].ToString();
                newNode.Tag = tagObj;
                
                tnc.Add(newNode);//将该节点加入到TreeNodeCollection（节点集合）中

                Bind_Tv(dt, newNode.Nodes, newNode.Name, id, pid, text, data, order, desc, disable, n_owner, n_tester);//递归（反复调用这个方法，直到把数据取完为止）
            }
        }

        public static Dictionary<string, string> GetNodeDataDict(TreeNode node)
        {
            var tagObj = node.Tag as DBTreeNodeTag;
            if (tagObj == null)
            {
                return null;
            }
            else
            {
                return tagObj.TagDict;
            }
        }





        #endregion
    }
}
