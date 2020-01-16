using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using Newtonsoft.Json;
using System;
using System.Collections;
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

namespace DbTree
{
    public partial class FormDBTree : Form
    {
        public FormDBTree()
        {
            InitializeComponent();
        }

        DevExpress.XtraTreeList.TreeList treeList;
        private void FormDBTree_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var jsonText = LoadByUrl();
            var nodeList = JsonConvert.DeserializeObject<List<TreeNode>>(jsonText);
            var dt = DataUtil.ListToDataTable<TreeNode>(nodeList);

            treeList = new DevExpress.XtraTreeList.TreeList();
            treeList.Dock = DockStyle.Fill;
            panelTree.Controls.Add(treeList);

            var caption = "xxx系统结构";
            set_treeList_Caption(dt, caption);

            treeList.ParentFieldName = "n_pid";
            treeList.KeyFieldName = "n_id";
            treeList.DataSource = dt;
            treeList.Refresh();


            //treeList.RootValue = "xxx"; //无用？
            set_treeList_face(treeList);

            set_treeList_drag(treeList);
            
            treeList.ExpandAll();

        }

        #region drap and drop
        private TreeListNode GetDragNode(IDataObject data)
        {
            return (TreeListNode)data.GetData(typeof(TreeListNode));
        }


        private void set_treeList_drag(TreeList treeList)
        {
            //目前只能改变同级的前后关系
            treeList.OptionsDragAndDrop.DragNodesMode = DragNodesMode.Single;
            this.treeList.CalcNodeDragImageIndex += new CalcNodeDragImageIndexEventHandler(this.call_CalcNodeDragImageIndex);
            this.treeList.DragOver += new DragEventHandler(this.call_DragOver);
            this.treeList.DragDrop += new DragEventHandler(this.call_DragDrop);
            this.treeList.DragEnter += new DragEventHandler(this.treeList_DragEnter);
        }

        private void treeList_DragEnter(object sender, DragEventArgs e)
        {
            TreeList list = (TreeList)sender;
            TreeListNode node = GetDragNode(e.Data);
            if (node != null && node.TreeList != list)
                e.Effect = DragDropEffects.Copy;
        }

        private void call_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            TreeListNode dragNode = e.Data.GetData(typeof(TreeListNode)) as TreeListNode;
            e.Effect = GetDragDropEffect(sender as TreeList, dragNode);
        }

        private void call_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            //TreeListNode dragNode = GetDragNode(e.Data);
            //TreeList list = (TreeList)sender;
            //if (dragNode == null) return;
            //TreeListHitInfo hitinfo = list.CalcHitInfo(list.PointToClient(new Point(e.X, e.Y)));
            //TreeListNode parentNode = hitinfo.Node;//当前需要拖拽到的节点
            //String inOID = dragNode.GetValue("n_id").ToString();
            //String ParentOID = parentNode.GetValue("n_pid").ToString();//目标节点OID
            //InsertBrush(ParentOID, inOID);
            //e.Effect = DragDropEffects.None;

            //可以完成上级级拖动，但是不灵活
            TreeListNode node = GetDragNode(e.Data);
            if (node == null) return;
            TreeList list = (TreeList)sender;
            if (list == node.TreeList) return;
            TreeListHitInfo info = list.CalcHitInfo(list.PointToClient(new Point(e.X, e.Y)));
            InsertBrush(list, node, info.Node == null ? -1 : info.Node.Id); 


            //只能改变节点前后顺序
            //TreeListNode dragNode, targetNode;
            //TreeList tl = sender as TreeList;
            //Point p = tl.PointToClient(new Point(e.X, e.Y));
            //dragNode = e.Data.GetData(typeof(TreeListNode)) as TreeListNode;
            //targetNode = tl.CalcHitInfo(p).Node;
            //tl.SetNodeIndex(dragNode, tl.GetNodeIndex(targetNode));
            //e.Effect = DragDropEffects.None;
        }

        private void InsertBrush(TreeList list, TreeListNode node, int parent)
        {
            var data = new ArrayList();
            foreach (TreeListColumn column in node.TreeList.Columns)
            {
                data.Add(node[column]);
            }
            parent = list.AppendNode(data.ToArray(), parent).Id;

            if (node.HasChildren)
                foreach (TreeListNode n in node.Nodes)
                    InsertBrush(list, n, parent);
        }



        private void call_CalcNodeDragImageIndex(object sender, DevExpress.XtraTreeList.CalcNodeDragImageIndexEventArgs e)
        {
            TreeList tl = sender as TreeList;
            if (GetDragDropEffect(tl, tl.FocusedNode) == DragDropEffects.None)
                e.ImageIndex = -1;  // no icon
            else
                e.ImageIndex = 1;  // the reorder icon (a curved arrow)
        }

        private DragDropEffects GetDragDropEffect(TreeList tl, TreeListNode dragNode)
        {
            TreeListNode targetNode;
            Point p = tl.PointToClient(MousePosition);
            targetNode = tl.CalcHitInfo(p).Node;

            if (dragNode != null && targetNode != null
                && dragNode != targetNode
                && dragNode.ParentNode == targetNode.ParentNode)
                return DragDropEffects.Move;
            else
                return DragDropEffects.None;
        }
        #endregion

        private void set_treeList_Caption(DataTable dt,string caption)
        {
            //因为动态产生的table，所以各列的顺序与json中各列的顺序并不一致
            int columnCount = dt.Columns.Count;
            for (var i = 0; i < columnCount; i++)
            {
                if (dt.Columns[i].ColumnName == "n_name")
                {
                    dt.Columns[i].Caption = caption;
                }
            }
        }

        private void set_treeList_face(TreeList myTreeList)
        {
            //只读
            myTreeList.OptionsBehavior.Editable = false;
            //线条模式(预设显示下拉三角形样式)
            myTreeList.TreeLineStyle = LineStyle.Solid;//线型
            myTreeList.LookAndFeel.UseDefaultLookAndFeel = false;
            myTreeList.LookAndFeel.UseWindowsXPTheme = true;

            // 设置选中时节点的背景色
            myTreeList.OptionsSelection.EnableAppearanceFocusedCell = true; //选中的Cell的Appearance设置是否可用
            this.treeList.Appearance.FocusedCell.Options.UseBackColor = true;
            this.treeList.Appearance.FocusedCell.BackColor = System.Drawing.Color.LightSteelBlue;
            this.treeList.Appearance.FocusedCell.BackColor2 = System.Drawing.Color.SteelBlue;
            // 去掉选中节点时的虚框
            this.treeList.OptionsView.FocusRectStyle = DrawFocusRectStyle.None;
            //是否显示Node的指示符面板，就是最左边有个三角箭头。默认为True；
            myTreeList.OptionsView.ShowIndicator = false;
        }

        private string LoadByUrl()
        {
            var url = this.txt_url.Text;
            var webClientObj = new WebClient();
            var postVars = new NameValueCollection {
                {"0", "select n_name,n_id,n_pid  from tree_1 where ifnull(n_status,'1') != '0' order by ifnull(n_order,_id)"}
            };

            byte[] byRemoteInfo = webClientObj.UploadValues(url, "POST", postVars);
            string json = Encoding.UTF8.GetString(byRemoteInfo);
            return json;
        }



        private void button2_Click(object sender, EventArgs e)
        {
            var node = treeList.FindNodeByFieldValue("n_name","模块22");
            treeList.SetFocusedNode(node);
        }

        TreeListNode tn;
        private void button3_Click(object sender, EventArgs e)
        {
            tn = (TreeListNode)this.treeList.FocusedNode.Clone();
            //treeList.Nodes.Remove(this.treeList.FocusedNode);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.treeList.FocusedNode.Nodes.Add(tn);
            treeList.Refresh();
        }

    }
}
