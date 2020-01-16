using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using CommTreeView;



namespace d_DBTreeView
{
    public partial class FormDBTree : Form
    {
        public FormDBTree()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //button1_Click(null, null);
            //button2_Click(null, null);
            //加载数据ToolStripMenuItem_Click_1(null, null);

            treeView1.HideSelection = false;//失去焦点时仍显示一个灰影
            this.treeView1.NodeMouseClick += new TreeNodeMouseClickEventHandler(this.call_NodeMouseClick);//单击节点时发生
            this.treeView1.BeforeSelect += new TreeViewCancelEventHandler(this.call_BeforeSelect);//让变灰节点不能被选中(或业务：不能选中不是自己的节点)


            set_drag_drop(treeView1);


        }



        #region drag

        private void set_drag_drop(TreeView treeView)
        {
            treeView.AllowDrop = true;
            treeView.ItemDrag += new ItemDragEventHandler(call_ItemDrag);
            treeView.DragEnter += new DragEventHandler(call_DragEnter);
            treeView.DragDrop += new DragEventHandler(call_DragDrop);
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


        private Point Position = new Point(0, 0);
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


        #endregion

        private void call_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                right_click(e.Node);
            }
            else
            {
                left_click(e.Node);
            }

        }

        private void right_click(TreeNode node)
        {
            //初始化菜单
            ContextMenuStrip cm = new ContextMenuStrip();
            //菜单选项
            var curr_Node = string.Format("{0}", node.Text);
            string[] menuItem = { curr_Node, "-", "增加同级节点", "增加子节点", "保存节点修改", "删除当前节点", "禁用当前节点" };
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

            treeView1.SelectedNode = node;

        }

        private void left_click(TreeNode node)
        {
            this.txtOrder.Clear();
            this.txtDisable.Clear();
            this.txtOwner.Clear();
            this.txtTester.Clear();
            this.txtDesc.Clear();
            this.txtTagData.Clear();

            txtLog2.AppendText("--------------------\r\n");
            //说明：使用TreeView的Click事件会出现上一次选中节点
            //如果用MouseDown则：treeView1.SelectedNode = treeView1.GetNodeAt(e.X, e.Y);
            if (node == null)
            {
                return;
            }

            if (node.ForeColor == Color.Gray)
            {
                txtLog2.AppendText("不响应:" + node.Text + " " + node.Name + "\r\n");
                return;
            }
            else
            {
                txtLog2.AppendText("响应:" + node.Text + "," + node.Name + "\r\n");
                //获取node data
                var tagObj = node.Tag as DBTreeNodeTag;
                if (tagObj != null)
                {
                    var dataDict = DBTreeNodeUtil.GetNodeDataDict(node);
                    var dictStr = DataHelper.DictToString(dataDict);

                    this.txtTagData.Text = dictStr;
                    this.txtOrder.Text = tagObj.OrderNum;
                    this.txtDesc.Text = tagObj.NodeDesc;
                    this.txtDisable.Text = tagObj.Disable;
                    this.txtOwner.Text = tagObj.Owner;
                    this.txtTester.Text = tagObj.Tester;
                }

            }
        }

        void cm_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //判断是否选项为空（没试过，也许多余）
            if (e.ClickedItem == null) return;

            var menuText = e.ClickedItem.Text;
            if (menuText.StartsWith("增加"))
            {
                addNewNode(menuText);//增加同级或子节点
            }
            else if (e.ClickedItem.Text.Contains("修改"))
            {
                orderNo = 0; //重算顺序号
                IterfatorTreeViewForOrder(treeView1.Nodes[0]);
                UpdateNode();
            }
            else if (e.ClickedItem.Text.Contains("删除"))
            {
                MessageBox.Show("这里是删除事件，比如弹出窗口");
            }
        }

        private void addNewNode(string menuText)
        {
            //判断选择项值
            var currNode = treeView1.SelectedNode;
            var newNode = new TreeNode();
            listId.Clear();
            setNodeNameByNewId(newNode);
            newNode.Text = "新节点";//todo:自动进入编辑状态

            if (menuText.Contains("同级"))
            {
                if (currNode.Parent != null)
                {
                    newNode.ToolTipText = currNode.Parent.Name;
                    currNode.Parent.Nodes.Add(newNode);

                }
                else
                {
                    //MessageBox.Show("没有找到上级节点，无法添加，请检查。");
                    MessageBox.Show("无法为根节点添加同级节点，请检查。");
                    return;
                }
            }
            else if (menuText.Contains("子节点"))
            {
                newNode.ToolTipText = currNode.Name;
                currNode.Nodes.Add(newNode);
            }
            //必须要入库，后面的新节点才能正确获取id

            //统一处理
            //newNode
            treeView1.SelectedNode = newNode;
            newNode.BeginEdit();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //test
            //var url = this.txt_url.Text;
            //var postVars = new NameValueCollection {
            //    {"0", "select n_name,n_id,n_pid,ifnull(n_data,'') n_data  from tree_1 where ifnull(n_status,'1') != '0' order by ifnull(n_order,_id),n_id"} //,n_desc,_id,n_order
            //};
            //string json = HttpServicerHelper.CallGoService(url, postVars);
            //txtJson.Text = json;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //test
            //var jsonText = txtJson.Text;
            //var nodeList = JsonConvert.DeserializeObject<List<DBTreeNode>>(jsonText);
            //var dt = TreeViewHelper.ListToDataTable<DBTreeNode>(nodeList);
            //dataGridView1.DataSource = dt;
            //dataGridView1.Refresh();
        }

        #region 绑定TreeView

       



        #endregion

        private void 加载数据ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();

            var datatable = this.dataGridView1.DataSource as DataTable;
            DBTreeNodeUtil.Bind_Tv(datatable, treeView1.Nodes, null, "n_id", "n_pid", "n_name", "n_data","n_order","n_desc","n_disable","n_owner","n_tester");
            treeView1.LabelEdit = true;
            treeView1.ExpandAll();
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
            if (Node != null)
            {
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

        }

        TreeNode tn;
        bool changId = false;
        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                changId = true;//复制节点，新的节点的ID要重新变化
                tn = (TreeNode)treeView1.SelectedNode.Clone();
            }
            else
            {
                MessageBox.Show("请选择要处理的节点");
            }
        }

        private void 剪切ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                changId = false;
                tn = (TreeNode)this.treeView1.SelectedNode.Clone();
                treeView1.Nodes.Remove(this.treeView1.SelectedNode);
            }
            else
            {
                MessageBox.Show("请选择要处理的节点");
            }

        }


        List<string> listId = new List<string>();
        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tn!=null)
            {
                if (changId)
                {
                    listId.Clear();
                    setNodeNameByNewId(tn);
                }

                this.treeView1.SelectedNode.Nodes.Add(tn);
                PorcessNewNode(tn);//处理被拖动的节点(递归)
                //从当前选中的节点开始展开
                this.treeView1.SelectedNode.ExpandAll();
                tn = null;
            }
            else
            {
                MessageBox.Show("请先复制或剪切要粘贴的节点");
            }
        }
        
        private void setNodeNameByNewId(TreeNode node)
        {
            var url = this.txt_url.Text;
            var nextId =DBTreeNodeUtil.GetNodeNextId(url,listId.Count())  ;
            var nextIdInt = Convert.ToInt32(nextId);
            listId.Add(nextId);
            node.Name = nextId;
        }

        private void PorcessNewNode(TreeNode node)
        {
            node.Tag = node.Parent.Name; ;//更改pid
            var msg = string.Format("id={0}\tpid={1}\ttext={2}\r\n", node.Name, node.Tag.ToString(), node.Text);
            txtLog2.AppendText(msg );
            if (node.Nodes.Count > 0)
            {
                SetTreeViewNewNode(node);//进入递归
            }
        }

        public void SetTreeViewNewNode(TreeNode nodes)
        {
            //有子节点
            foreach (TreeNode node in nodes.Nodes)
            {
                var pid = node.Parent.Name;
                node.Tag = pid;//更改pid
                CountTreeViewNode(treeView1.Nodes[0]);

                //更好的算法是找出其中最大的值，再加1（因为有可能删除）
                if (changId)
                {
                    setNodeNameByNewId(node);
                }

                PorcessNewNode(node);
            }
        }

        List<TreeNode> findList = new List<TreeNode>();
        int findCount = 0;
        int nodeCount = 0;
        bool isReturn = false;
        private void button3_Click(object sender, EventArgs e)
        {
            isReturn = false;
            if (nodeCount == 0)
            {
                CountTreeViewNode(treeView1.Nodes[0]);
            }
            findCount = 0;
            var findText = txtFind.Text;
            FindTreeViewList(treeView1.Nodes[0], findText);

        }

        public void CountTreeViewNode(TreeNode node)
        {
            foreach (TreeNode newNode in node.Nodes)
            {
                nodeCount++;
                if (newNode.Nodes.Count > 0)
                {
                    CountTreeViewNode(newNode);
                }
            }
        }

        
        public void FindTreeViewList(TreeNode nodes,string findText)
        {         
            foreach (TreeNode node in nodes.Nodes)
            {
                if (isReturn)
                {
                    break;
                }

                //有子节点
                findCount++;
                if (node.Text.Contains(findText) && findList.IndexOf(node)==-1)
                {
                    findList.Add(node);
                    //两句话同时才有获取焦点的效果
                    treeView1.Focus();
                    treeView1.SelectedNode = node;
                    txtLog.AppendText("找到:"+node.Text + "\r\n");
                    isReturn = true;
                    return;
                }
                else
                {
                    if (findCount >= nodeCount)
                    {
                        //var msg = string.Format("查找结束，一共遍历了{0}个节点。", findCount);
                        //MessageBox.Show(msg);
                        findList.Clear();
                        nodeCount = 0;
                        
                        return;
                    }
                    FindTreeViewList(node, findText);
                    
                }

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
        }

        private void 获取最大IDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var url = this.txt_url.Text;
            string max_id = DBTreeNodeUtil.GetNodeNextId(url,0);
            MessageBox.Show(max_id);
        }



        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //入库算法：首先全部打上禁用标志(原因：删除或其它）
            //遍历节点树，不存在则插入，同时带顺序号；存在则只更新顺序号。
        }

        private void 保存指定节点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateNode();
        }

        private void UpdateNode()
        {
            var url = get_exec_url();
            var node = treeView1.SelectedNode;

            SaveCurrentNodeTagData();

            var msg = DBTreeNodeUtil.UpdateCurrNode(url, node);
            if (msg.Contains("\"rows\":1"))
            {
                MessageBox.Show("更新成功");
            }
            else
            {
                MessageBox.Show(msg);
            }
        }

        private void 删除指定定点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var url = get_exec_url();
            var node = treeView1.SelectedNode;
            var msg = DBTreeNodeUtil.DeleteCurrNode(url,node);
            if (msg.Contains("\"rows\":1"))
            {
                treeView1.Nodes.Remove(node);
                MessageBox.Show("删除节点成功");
            }
            else
            {
                MessageBox.Show(msg);
            }
        }


        private void 禁用指定节点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            txtLog2.AppendText(node.Text+"已经设置变灰");
            node.ForeColor = Color.Gray;//待：保存到节点的tag中
        }

        private void button5_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            var url = this.txt_url.Text;//正式
            var datatable = DBTreeNodeUtil.GetNodeDataTable(url);
            DBTreeNodeUtil.Bind_Tv(datatable, treeView1.Nodes, null, "n_id", "n_pid", "n_name", "n_data", "n_order", "n_desc", "n_disable", "n_owner","n_tester");
            treeView1.LabelEdit = true;
            treeView1.ExpandAll();
        }

        private void SaveCurrentNodeTagData()
        {
            if (treeView1.SelectedNode != null && treeView1.SelectedNode.Tag!=null)
            {
                var tagObj = treeView1.SelectedNode.Tag as DBTreeNodeTag;
                if (txtTagData.Text != "")
                {
                    var jsonData = txtTagData.Text;
                    tagObj.TagDict = DataHelper.StringToDict(jsonData);
                }

                if (txtOrder.Text != "")
                {
                    tagObj.OrderNum = txtOrder.Text;
                }

                if (txtDesc.Text != "")
                {
                    tagObj.NodeDesc = txtDesc.Text;
                }

                if (txtDisable.Text != "")
                {
                    tagObj.Disable = txtDisable.Text;
                }

                if (txtOwner.Text != "")
                {
                    tagObj.Owner = txtOwner.Text;
                }

                if (txtTester.Text != "")
                {
                    tagObj.Tester = txtTester.Text;
                }

                treeView1.SelectedNode.Tag = tagObj;

                txtLog2.AppendText(string.Format("节点{0}：属性保存成功",treeView1.SelectedNode.Text));
            }
        }

        private void 增加同级节点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }


        private void call_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            //让变灰的节点不能被选中
            var tn = e.Node;
            if (tn.ForeColor == Color.Gray)
            {
                e.Cancel = true;
                return;
            }
        }

        private void txtLog2_DoubleClick(object sender, EventArgs e)
        {
            txtLog2.Clear();
        }

        int orderNo = 0;
        private void 遍历测试ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            orderNo = 0;
            IterfatorTreeView(treeView1.Nodes[0]);
        }


        public void IterfatorTreeView(TreeNode nodes)
        {
            //有子节点
            foreach (TreeNode node in nodes.Nodes)
            {
                txtLog2.AppendText("*********\r\n");
                var msg = string.Format("id={0}\tpid={1}\ttext={2}", node.Name, node.ToolTipText.ToString(), node.Text);
                txtLog2.AppendText(msg + "\r\n");
                var tagObj = node.Tag as DBTreeNodeTag;
                if (tagObj != null)
                {
                    var dataDict = DBTreeNodeUtil.GetNodeDataDict(node);
                    var dictStr = DataHelper.DictToString(dataDict);

                    txtLog2.AppendText("dictStr:" + dictStr + "\r\n");
                    //顺序号每次遍历要重新赋值
                    tagObj.OrderNum = (orderNo++).ToString();//一定要赋值
                    txtLog2.AppendText("OrderNum:" + tagObj.OrderNum + "\r\n");
                    txtLog2.AppendText("NodeDesc:" + tagObj.NodeDesc + "\r\n");
                    txtLog2.AppendText("Disable:" + tagObj.Disable + "\r\n");
                    txtLog2.AppendText("Owner:" + tagObj.Owner + "\r\n");
                    txtLog2.AppendText("Tester:" + tagObj.Tester + "\r\n");
                }

                if (node.Nodes.Count > 0)
                {
                    IterfatorTreeView(node);
                }
            }
        }

        private void 遍历重算序号ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            orderNo = 0;
            IterfatorTreeViewForOrder(treeView1.Nodes[0]);
        }

        public void IterfatorTreeViewForOrder(TreeNode nodes)
        {
            //有子节点
            foreach (TreeNode node in nodes.Nodes)
            {
                var tagObj = node.Tag as DBTreeNodeTag;
                if (tagObj != null)
                {
                    tagObj.OrderNum = (orderNo++).ToString();//一定要赋值
                }

                if (node.Nodes.Count > 0)
                {
                    IterfatorTreeViewForOrder(node);
                }
            }
        }

        private void 保存所有节点ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public void IterfatorTreeViewForSave(string url,TreeNode nodes)
        {
            //有子节点
            foreach (TreeNode node in nodes.Nodes)
            {
                //保存当前节点
                var msg = DBTreeNodeUtil.UpdateCurrNode(url, node);
                txtLog2.AppendText(string.Format("保存:{0}:返回:{1}\r\n", node.Text,msg));

                if (node.Nodes.Count > 0)
                {
                    IterfatorTreeViewForSave(url,node);
                }
            }
        }

        private void 保存所有节点ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            //先保存当前节点信息
            SaveCurrentNodeTagData();

            //重算顺序号
            orderNo = 0;
            IterfatorTreeViewForOrder(treeView1.Nodes[0]);

            var url = get_exec_url();

            IterfatorTreeViewForSave(url, treeView1.Nodes[0]);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //将txt中的内容保存到节点的tag中
            SaveCurrentNodeTagData();
        }


        private void 保存新节点增加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string url = get_exec_url();
            
            var node = treeView1.SelectedNode;

            SaveCurrentNodeTagData();

            //重算顺序号
            orderNo = 0;
            IterfatorTreeViewForOrder(treeView1.Nodes[0]);

            var msg = DBTreeNodeUtil.InsertNewNode(url, node, orderNo);
            if (msg.Contains("\"rows\":1"))
            {
                MessageBox.Show("保存成功");
            }
            else
            {
                MessageBox.Show(msg);
            }
        }

        private string get_exec_url()
        {
            var url = txt_url.Text.Replace("db_query", "db_exec");
            return url;
        }

        private void 智能保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var url = get_exec_url();
            var node = treeView1.SelectedNode;

            SaveCurrentNodeTagData();

            var msg = DBTreeNodeUtil.UpdateCurrNode(url, node);
            if (msg.Contains("\"rows\":1"))
            {
                MessageBox.Show("更新成功");
            }
            else
            {
                //指先更新，如果发现没有数据，则尝试插入
                SaveCurrentNodeTagData();

                //重算顺序号
                orderNo = 0;
                IterfatorTreeViewForOrder(treeView1.Nodes[0]);

                msg = DBTreeNodeUtil.InsertNewNode(url, node, orderNo);
                if (msg.Contains("\"rows\":1"))
                {
                    MessageBox.Show("保存成功");
                }
                else
                {
                    MessageBox.Show(msg);
                }
            }
        }


    }


}
