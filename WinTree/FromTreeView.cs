using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinTree
{


    public partial class FromTreeView : Form
    {
        public FromTreeView()
        {
            InitializeComponent();
        }

        private Point Position = new Point(0, 0);
        private void FromTreeView_Load(object sender, EventArgs e)
        {
        }

        private void call_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

            //是否单击右键
            if (e.Button == MouseButtons.Right)
            {
                //初始化菜单
                ContextMenuStrip cm = new ContextMenuStrip();
                //菜单选项
                var curr_Node = string.Format("{0}", e.Node.Text);
                string[] menuItem = { curr_Node, "-", "增加", "修改", "删除" };
                //循环添加菜单选项
                for (int i = 0; i < menuItem.Length; i++)
                {
                    cm.Items.Add(menuItem[i]);
                }
                cm.Items[0].Enabled = true;//为false则红色不生效
                cm.Items[0].Font = new System.Drawing.Font(cm.Items[0].Font, FontStyle.Bold);
                cm.Items[0].ForeColor = System.Drawing.Color.Red;
                //菜单选项单击事件
                cm.ItemClicked += new ToolStripItemClickedEventHandler(cm_ItemClicked);
                //绑定菜单选项到树形菜单
                this.treeView1.ContextMenuStrip = cm;

                treeView1.SelectedNode = e.Node;
            }
        }

        void cm_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //判断是否选项为空（没试过，也许多余）
            if (e.ClickedItem == null) return;
            //判断选择项值
            if (e.ClickedItem.Text == "增加")
            {
                MessageBox.Show("这里是添加事件，比如弹出窗口");
            }
            else if (e.ClickedItem.Text == "修改")
            {
                MessageBox.Show("这里是修改事件，比如弹出窗口");
            }
            else if (e.ClickedItem.Text == "删除")
            {
                MessageBox.Show("这里是删除事件，比如弹出窗口");
            }
        }

        private void Bind(TreeNode parNode, List<UrlTypes> list, int nodeId)
        {
            var childList = list.FindAll(t => t.ParentId == nodeId).OrderBy(t => t.Id);
            foreach (var urlTypese in childList)
            {
                var node = new TreeNode();
                node.Name = urlTypese.Id.ToString();
                node.Text = urlTypese.Name;
                parNode.Nodes.Add(node);
                Bind(node, list, urlTypese.Id);
            }
        }


        private void call_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void call_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void call_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode myNode = null;
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                myNode = (TreeNode)(e.Data.GetData(typeof(TreeNode)));
            }
            else
            {
                MessageBox.Show("error");
            }
            Position.X = e.X;
            Position.Y = e.Y;
            Position = treeView1.PointToClient(Position);
            TreeNode DropNode = this.treeView1.GetNodeAt(Position);
            // 1.目标节点不是空。2.目标节点不是被拖拽接点的字节点。3.目标节点不是被拖拽节点本身
            if (DropNode != null && DropNode.Parent != myNode && DropNode != myNode)
            {
                TreeNode DragNode = myNode;
                // 将被拖拽节点从原来位置删除。
                myNode.Remove();
                // 在目标节点下增加被拖拽节点
                DropNode.Nodes.Add(DragNode);
            }
            // 如果目标节点不存在，即拖拽的位置不存在节点，那么就将被拖拽节点放在根节点之下
            if (DropNode == null)
            {
                TreeNode DragNode = myNode;
                myNode.Remove();
                treeView1.Nodes.Add(DragNode);
            }

            //选中来源节点
            treeView1.SelectedNode = myNode;
            //选中目标节点
            treeView1.SelectedNode = DropNode;

            treeView1.ExpandAll();
        }

        private void 遍历ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetTreeViewList(treeView1.Nodes[0]);
        }

        public void SetTreeViewList(TreeNode nodes)
        {
            //有子节点
            foreach (TreeNode newNode in nodes.Nodes)
            {
                var msg = string.Format("{0} -->{1}", newNode.Text, newNode.Parent.Text);
                textBox1.AppendText(msg + "\r\n");
                if (newNode.Nodes.Count > 0)
                {
                    SetTreeViewList(newNode);
                }
            }
        }

        TreeNode tn;
        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tn = (TreeNode)this.treeView1.SelectedNode.Clone();

        }

        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.treeView1.SelectedNode.Nodes.Add(tn);

            PorcessNewNode(tn);//处理被拖动的节点(递归)

            //从当前选中的节点开始展开
            this.treeView1.SelectedNode.ExpandAll();

        }

        public void SetTreeViewNewNode(TreeNode nodes)
        {
            //有子节点
            foreach (TreeNode newNode in nodes.Nodes)
            {
                PorcessNewNode(newNode);
            }
        }

        private void PorcessNewNode(TreeNode newNode)
        {
            var msg = string.Format("{0}***{1}", newNode.Text, newNode.Parent.Text);
            textBox1.AppendText(msg + "\r\n");
            if (newNode.Nodes.Count > 0)
            {
                SetTreeViewNewNode(newNode);//进入递归
            }
        }

        private void 上移ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode Node = this.treeView1.SelectedNode;
            TreeNode PrevNode = Node.PrevNode;
            if (PrevNode != null)
            {

                TreeNode NewNode = (TreeNode)Node.Clone();
                if (Node.Parent == null)
                {
                    treeView1.Nodes.Insert(PrevNode.Index, NewNode);
                }
                else
                {
                    Node.Parent.Nodes.Insert(PrevNode.Index, NewNode);
                }
                Node.Remove();
                treeView1.SelectedNode = NewNode;
            }
        }

        private void 下移ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode Node = treeView1.SelectedNode;
            TreeNode NextNode = Node.NextNode;
            if (NextNode != null)
            {

                TreeNode NewNode = (TreeNode)Node.Clone();
                if (Node.Parent == null)
                {
                    treeView1.Nodes.Insert(NextNode.Index + 1, NewNode);
                }
                else
                {
                    Node.Parent.Nodes.Insert(NextNode.Index + 1, NewNode);
                }
                Node.Remove();
                treeView1.SelectedNode = NewNode;
            }
        }

        private void 剪切ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tn = (TreeNode)this.treeView1.SelectedNode.Clone();
            treeView1.Nodes.Remove(this.treeView1.SelectedNode);
        }

        private void 加载ListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void 加载DataTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        #region 绑定TreeView

        private DataTable Test_Table()
        {
            DataTable dt = new DataTable();
            DataRow dr;
            dt.Columns.Add(new DataColumn("id", typeof(Guid)));//id列 类型guid  
            dt.Columns.Add(new DataColumn("parent_id", typeof(Guid)));//父id列 类型guid  
            dt.Columns.Add(new DataColumn("name", typeof(string)));//名称列 类型string  
            //构造 公司 根节点  
            dr = dt.NewRow();
            var node0 = dr[0] = Guid.NewGuid();
            dr[1] = DBNull.Value;
            dr[2] = "** 公司";
            dt.Rows.Add(dr);
            //构造 部门 节点  
            string[] department = { "A部门", "B部门", "C部门" };
            for (int i = 0; i < department.Length; i++)
            {
                dr = dt.NewRow();
                var node1 = dr[0] = Guid.NewGuid();
                dr[1] = node0;//（部门节点）属于公司根节点  
                dr[2] = department[i];
                dt.Rows.Add(dr);
                //构造 班组 节点  
                for (int j = 1; j < 4; j++)
                {
                    dr = dt.NewRow();
                    dr[0] = Guid.NewGuid();
                    dr[1] = node1;
                    dr[2] = j + "班组";
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        /// <summary>
        /// 绑定TreeView（利用TreeNode）
        /// </summary>
        /// <param name="p_Node">TreeNode（TreeView的一个节点）</param>
        /// <param name="pid_val">父id的值</param>
        /// <param name="id">数据库 id 字段名</param>
        /// <param name="pid">数据库 父id 字段名</param>
        /// <param name="text">数据库 文本 字段值</param>
        protected void Bind_Tv(DataTable dt, TreeNode p_Node, string pid_val, string id, string pid, string text)
        {
            DataView dv = new DataView(dt);//将DataTable存到DataView中，以便于筛选数据
            TreeNode tn;//建立TreeView的节点（TreeNode），以便将取出的数据添加到节点中
            //以下为三元运算符，如果父id为空，则为构建“父id字段 is null”的查询条件，否则构建“父id字段=父id字段值”的查询条件
            string filter = string.IsNullOrEmpty(pid_val) ? pid + " is null" : string.Format(pid + "='{0}'", pid_val);
            dv.RowFilter = filter;//利用DataView将数据进行筛选，选出相同 父id值 的数据
            foreach (DataRowView row in dv)
            {
                tn = new TreeNode();//建立一个新节点（学名叫：一个实例）
                if (p_Node == null)//如果为根节点
                {
                    tn.Name = row[id].ToString();//节点的Value值，一般为数据库的id值
                    tn.Text = row[text].ToString();//节点的Text，节点的文本显示
                    treeView1.Nodes.Add(tn);//将该节点加入到TreeView中
                    Bind_Tv(dt, tn, tn.Name, id, pid, text);//递归（反复调用这个方法，直到把数据取完为止）
                }
                else//如果不是根节点
                {
                    tn.Name = row[id].ToString();//节点Value值
                    tn.Text = row[text].ToString();//节点Text值
                    p_Node.Nodes.Add(tn);//该节点加入到上级节点中
                    Bind_Tv(dt, tn, tn.Name, id, pid, text);//递归
                }
            }
        }

        /// <summary>
        /// 绑定TreeView（利用TreeNodeCollection）
        /// </summary>
        /// <param name="tnc">TreeNodeCollection（TreeView的节点集合）</param>
        /// <param name="pid_val">父id的值</param>
        /// <param name="id">数据库 id 字段名</param>
        /// <param name="pid">数据库 父id 字段名</param>
        /// <param name="text">数据库 文本 字段值</param>
        private void Bind_Tv(DataTable dt, TreeNodeCollection tnc, string pid_val, string id, string pid, string text)
        {
            DataView dv = new DataView(dt);//将DataTable存到DataView中，以便于筛选数据
            TreeNode tn;//建立TreeView的节点（TreeNode），以便将取出的数据添加到节点中
            //以下为三元运算符，如果父id为空，则为构建“父id字段 is null”的查询条件，否则构建“父id字段=父id字段值”的查询条件
            string filter = string.IsNullOrEmpty(pid_val) ? pid + " is null" : string.Format(pid + "='{0}'", pid_val);
            dv.RowFilter = filter;//利用DataView将数据进行筛选，选出相同 父id值 的数据
            foreach (DataRowView drv in dv)
            {
                tn = new TreeNode();//建立一个新节点（学名叫：一个实例）
                tn.Name = drv[id].ToString();//节点的Value值，一般为数据库的id值
                tn.Text = drv[text].ToString();//节点的Text，节点的文本显示
                tnc.Add(tn);//将该节点加入到TreeNodeCollection（节点集合）中
                Bind_Tv(dt, tn.Nodes, tn.Name, id, pid, text);//递归（反复调用这个方法，直到把数据取完为止）
            }
        }
        #endregion

        private void 隐藏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
          
        }

        private void 加载ListToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var dataList = new List<UrlTypes>()
            {
                new UrlTypes() {Id = 1, Name = "中国", Value = "0", ParentId = 0},
                new UrlTypes() {Id = 2, Name = "河南", Value = "0", ParentId = 1},
                new UrlTypes() {Id = 3, Name = "河北", Value = "0", ParentId = 1},
                new UrlTypes() {Id = 4, Name = "南阳", Value = "0", ParentId = 2},
                new UrlTypes() {Id = 5, Name = "信阳", Value = "0", ParentId = 2},
                new UrlTypes() {Id = 6, Name = "新野", Value = "0", ParentId = 4},
                new UrlTypes() {Id = 7, Name = "石家庄", Value = "0", ParentId = 3}
            };

            var topNode = new TreeNode();
            topNode.Name = "0";
            topNode.Text = "世界";
            treeView1.Nodes.Add(topNode);
            Bind(topNode, dataList, 0);

            treeView1.AllowDrop = true;
            treeView1.ShowLines = true;
            //treeView1.ShowRootLines = true;

            //treeView1.HideSelection = false;//可让选中节点保持高亮。默认为True

            treeView1.ItemDrag += new ItemDragEventHandler(call_ItemDrag);
            treeView1.DragEnter += new DragEventHandler(call_DragEnter);
            treeView1.DragDrop += new DragEventHandler(call_DragDrop);

            treeView1.ExpandAll();

            this.treeView1.NodeMouseClick += new TreeNodeMouseClickEventHandler(call_NodeMouseClick);
        }

        private void 加载DataTableToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var datatable = Test_Table();
            Bind_Tv(datatable, treeView1.Nodes, null, "id", "parent_id", "name");
            treeView1.ExpandAll();
        }

        private void 设置节点JSON数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Hashtable hash = new Hashtable();
            //hash.Add("key1", "val1");
            //hash.Add("key2", "val2");
            //string json = JsonConvert.SerializeObject(hash);//{"key1":"val1","key2":"val2"}


            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("key1", "val1");
            dict.Add("key2", "val2");
            //JObject[] jo = (from p in dic select new JObject { new JProperty("key", p.Key), new JProperty("val", p.Value) }).ToArray<JObject>();
            string json = JsonConvert.SerializeObject(dict);

            treeView1.SelectedNode.Tag = json;
            textBox1.AppendText(json + "\r\n");
        }

        private void 获取节点数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag != null)
            {
                var json = treeView1.SelectedNode.Tag.ToString();
                textBox1.AppendText(json + "\r\n");

                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                textBox1.AppendText(dict["key1"] + "\r\n");
                textBox1.AppendText(dict["key2"] + "\r\n");
            }
        }

    }
}
