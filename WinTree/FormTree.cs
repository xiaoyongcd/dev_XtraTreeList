using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


//https://blog.csdn.net/laoyezha/article/details/79302679

//https://blog.csdn.net/zcc0618/article/details/45059317

namespace WinTree
{
    public partial class FormTree : Form
    {
        public FormTree()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitRightTree();
        }


        DevExpress.XtraTreeList.TreeList treeList;
        private void InitRightTree()
        {
            var panelRight = this.panelNode;
            panelRight.Controls.Clear();
            treeList = new DevExpress.XtraTreeList.TreeList();
            treeList.Dock = DockStyle.Fill;
            panelRight.Controls.Add(treeList);

            //置这两个属性之后就实现了TreeList带有CheckBox,并且节点有三种状态
            var showCheckbox = false;
            treeList.OptionsView.ShowCheckBoxes = showCheckbox;
            //treeList.OptionsBehavior.AllowIndeterminateCheckState = true;  
            if (showCheckbox)
            {
                treeList.AfterCheckNode += call_afterCheckNode;
                treeList.BeforeCheckNode += call_beforeCheckNode;
            }

            treeList.Click += call_treeList_Click;
            treeList.MouseMove += tcall_MouseMove;
            treeList.DoubleClick += call_DoubleClick;



            //增加数据源
            DataTable dt = GetNodeData();
            treeList.DataSource = dt;


            //设置树的ParentFieldName KeyFieldName属性
            treeList.ParentFieldName = "ParentFieldName";
            treeList.KeyFieldName = "KeyFieldName";
            

            treeList.OptionsBehavior.Editable = false;

            //选中节点：加粗。
            //Node的显示（包括窗口的切换导致Node的显示）和状态的改变都会触发该事件。该事件主要用来改变Node的显示样式。
            treeList.NodeCellStyle += call_NodeCellStyle;

            treeList.SelectImageList = imageList1;
            treeList.CustomDrawNodeImages += call_CustomDrawNodeImages;

            //显示行号
            //treeList.CustomDrawNodeIndicator += call_CustomDrawNodeIndicator;


            //目前只能改变同级的前后关系
            treeList.OptionsDragAndDrop.DragNodesMode = DragNodesMode.Single;
            this.treeList.CalcNodeDragImageIndex += new CalcNodeDragImageIndexEventHandler(this.call_CalcNodeDragImageIndex);
            this.treeList.DragOver += new System.Windows.Forms.DragEventHandler(this.call_DragOver);
            this.treeList.DragDrop += new System.Windows.Forms.DragEventHandler(this.call_DragDrop);

            //线条模式(预设显示下拉三角形样式)
            treeList.TreeLineStyle = LineStyle.Solid;//线型
            treeList.LookAndFeel.UseDefaultLookAndFeel = false;
            treeList.LookAndFeel.UseWindowsXPTheme = true;

            // 设置选中时节点的背景色
            treeList.OptionsSelection.EnableAppearanceFocusedCell = true; //选中的Cell的Appearance设置是否可用
            this.treeList.Appearance.FocusedCell.Options.UseBackColor = true;
            this.treeList.Appearance.FocusedCell.BackColor = System.Drawing.Color.LightSteelBlue;
            this.treeList.Appearance.FocusedCell.BackColor2 = System.Drawing.Color.SteelBlue;

            //颜色设置
            //treeList.Appearance.Row.BackColor = Color.Transparent;//节点默认背景色
            //treeList.Appearance.FocusedRow.BackColor = Color.Orange; // 选中节点的背景色
            //treeList.Appearance.HideSelectionRow.BackColor = Color.LightYellow;//选中节点失去焦点时的背景色

            //为了和系统界面一致改成透明色
            treeList.BackColor = Color.Transparent;
            treeList.Appearance.Empty.BackColor = Color.Transparent;
            treeList.Appearance.Row.BackColor = Color.Transparent;
            

            // 去掉选中节点时的虚框
            this.treeList.OptionsView.FocusRectStyle = DrawFocusRectStyle.None;
            //是否显示Node的指示符面板，就是最左边有个三角箭头。默认为True；
            treeList.OptionsView.ShowIndicator = false;

            //换行（在显示宽度不够时才会）
            treeList.OptionsBehavior.AutoNodeHeight = true;
            treeList.Columns[0].ColumnEdit = new RepositoryItemMemoEdit();

            //显示内置的过滤行
            //treeList.OptionsView.ShowAutoFilterRow = true;//显示过滤行
            //treeList.OptionsBehavior.EnableFiltering = true;//开启过滤功能
            //treeList.ColumnFilterChanged += call_ColumnFilterChanged;//定义TreeList列过滤事件

            //设置显示水平滚动条
            //treeList.OptionsView.AutoWidth = false;


            //展开
            treeList.ExpandAll();
            //treeList.ExpandToLevel(2); //展开的层级
        }
         private void call_ColumnFilterChanged(object sender, EventArgs e)
        {
            if (treeList.ActiveEditor != null)
            {
                string newKey = treeList.ActiveEditor.EditValue.ToString();
                treeList.FilterNodes();
                var operation = new FilterNodeOperation((!System.String.IsNullOrEmpty(newKey)) ? newKey : "");
                treeList.NodesIterator.DoOperation(operation);
            }
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

        private void call_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            TreeListNode dragNode = e.Data.GetData(typeof(TreeListNode)) as TreeListNode;
            e.Effect = GetDragDropEffect(sender as TreeList, dragNode);
        }

        private void call_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            TreeListNode dragNode, targetNode;
            TreeList tl = sender as TreeList;
            Point p = tl.PointToClient(new Point(e.X, e.Y));

            dragNode = e.Data.GetData(typeof(TreeListNode)) as TreeListNode;
            targetNode = tl.CalcHitInfo(p).Node;

            tl.SetNodeIndex(dragNode, tl.GetNodeIndex(targetNode));
            e.Effect = DragDropEffects.None;
        }

        private void call_CalcNodeDragImageIndex(object sender, DevExpress.XtraTreeList.CalcNodeDragImageIndexEventArgs e)
        {
            TreeList tl = sender as TreeList;
            if (GetDragDropEffect(tl, tl.FocusedNode) == DragDropEffects.None)
                e.ImageIndex = -1;  // no icon
            else
                e.ImageIndex = 1;  // the reorder icon (a curved arrow)
        }

       private void call_CustomDrawNodeIndicator(object sender, DevExpress.XtraTreeList.CustomDrawNodeIndicatorEventArgs e)
        {
            DevExpress.XtraTreeList.TreeList tmpTree = sender as DevExpress.XtraTreeList.TreeList;
            DevExpress.Utils.Drawing.IndicatorObjectInfoArgs args = e.ObjectArgs as DevExpress.Utils.Drawing.IndicatorObjectInfoArgs;
            if (args != null)
            {
                int rowNum = tmpTree.GetVisibleIndexByNode(e.Node) + 1;
                //设置宽度
                this.treeList.IndicatorWidth = rowNum.ToString().Length * 10 + 20;
                args.DisplayText = rowNum.ToString();
            }
        }


        private void call_CustomDrawNodeImages(object sender, DevExpress.XtraTreeList.CustomDrawNodeImagesEventArgs e)
        {

            if (e.Node.Nodes.Count > 0)
            {
                if (e.Node.Expanded)
                {
                    e.SelectImageIndex = 1;
                    return;
                }
                e.SelectImageIndex = 0;
            }
            else
            {
                e.SelectImageIndex = 2;
            }
        }

        private void call_NodeCellStyle(object sender, DevExpress.XtraTreeList.GetCustomNodeCellStyleEventArgs e)
        {
            if (e.Node.CheckState == CheckState.Checked)
            {
                e.Appearance.Font = new Font(DevExpress.Utils.AppearanceObject.DefaultFont, FontStyle.Bold);
                e.Appearance.ForeColor = Color.Gray;
            }
        }


        //双击Node时触发，但要注意的是要在TreeList.OptionsBehavior.Editable = false的情况下，双击Node才能触发该事件
        private void call_DoubleClick(object sender, EventArgs e)
        {
            TreeListNode clickedNode = this.treeList.FocusedNode;
            string disPlayText = clickedNode.GetDisplayText("test");
            textBox1.AppendText("You clicked " + disPlayText);
        }

         private void call_afterCheckNode(object sender, DevExpress.XtraTreeList.NodeEventArgs e)
        {
            SetCheckedChildNodes(e.Node, e.Node.CheckState);
            SetCheckedParentNodes(e.Node, e.Node.CheckState);
        }

         private void SetCheckedParentNodes(TreeListNode node, CheckState check)
        {
            if (node.ParentNode != null)
            {
                bool b = false;
                CheckState state;
                for (int i = 0; i < node.ParentNode.Nodes.Count; i++)
                {
                    state = (CheckState)node.ParentNode.Nodes[i].CheckState;
                    if (!check.Equals(state))
                    {
                        b = !b;
                        break;
                    }
                }
                node.ParentNode.CheckState = b ? CheckState.Indeterminate : check;
                SetCheckedParentNodes(node.ParentNode, check);
            }
        }

        private DataTable GetNodeData()
        {
            DataTable dt = new DataTable();
            DataColumn dc = new DataColumn("KeyFieldName");//KeyFieldName ParentFieldName  name
            DataColumn dc3 = new DataColumn("ParentFieldName");
            DataColumn dc2 = new DataColumn("name");

            //数据上有_id列（作为update用)，但是不要显示出来。
            //说明：目前没有找到解决方法，所以将Node_id设置为unique_index,从而可以根据这个值进行操作了。
            DataColumn dc4 = new DataColumn("_id");
            dc2.Caption = "XXX树";
            //DataColumn dc4 = new DataColumn("IMAGEINDEX");
            dt.Columns.Add(dc);
            dt.Columns.Add(dc2);
            dt.Columns.Add(dc3);
            dt.Columns.Add(dc4);

            DataRow dr1 = dt.NewRow();
            var i = 1;
            dr1["KeyFieldName"] = 1;
            dr1["ParentFieldName"] = 0;
            dr1["name"] = "系统11有人有人有人有人有人有人有人有人有人有人有人有人的";
            dr1["_id"] = i++;
            dt.Rows.Add(dr1);

            DataRow dr2 = dt.NewRow();
            dr2["KeyFieldName"] = 2;
            dr2["ParentFieldName"] = 1;
            dr2["name"] = "模块1";
            dr2["_id"] = i++;
            dt.Rows.Add(dr2);

            DataRow dr3 = dt.NewRow();
            dr3["KeyFieldName"] = 3;
            dr3["ParentFieldName"] = 1;
            dr3["name"] = "模块2";
            dr3["_id"] = i++;
            dt.Rows.Add(dr3);

            DataRow dr4 = dt.NewRow();
            dr4["KeyFieldName"] = 4;
            dr4["ParentFieldName"] = 1;
            dr4["name"] = "我块22";
            dr4["_id"] = i++;
            dt.Rows.Add(dr4);

            DataRow dr5 = dt.NewRow();
            dr5["KeyFieldName"] = 5;
            dr5["ParentFieldName"] = 4;
            dr5["name"] = "我的32";
            dr5["_id"] = i++;
            dt.Rows.Add(dr5);

            DataRow dr6 = dt.NewRow();
            dr6["KeyFieldName"] = 6;
            dr6["ParentFieldName"] = 5;
            dr6["name"] = "我块22";
            dr6["_id"] = i++;
            dt.Rows.Add(dr6);

            DataRow dr7 = dt.NewRow();
            dr7["KeyFieldName"] = 7;
            dr7["ParentFieldName"] = 6;
            dr7["name"] = "我块77";
            dr7["_id"] = i++;
            dt.Rows.Add(dr7);

            DataRow dr8 = dt.NewRow();
            dr8["KeyFieldName"] = 8;
            dr8["ParentFieldName"] = 7;
            dr8["name"] = "我块28";
            dr8["_id"] = i++;
            dt.Rows.Add(dr8);

            return dt;
        }


        private void call_beforeCheckNode(object sender, DevExpress.XtraTreeList.CheckNodeEventArgs e)
        {
            e.State = (e.PrevState == CheckState.Checked ? CheckState.Unchecked : CheckState.Checked);
        }
        /// <summary>
        /// 设置子节点的状态
        /// </summary>
        /// <param name="node"></param>
        /// <param name="check"></param>
        private void SetCheckedChildNodes(TreeListNode node, CheckState check)
        {
            for (int i = 0; i < node.Nodes.Count; i++)
            {
                node.Nodes[i].CheckState = check;
                SetCheckedChildNodes(node.Nodes[i], check);
            }
        }

        private void tcall_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = treeList.PointToClient(Cursor.Position);
            TreeListHitInfo hitInfo = treeList.CalcHitInfo(point);
            switch (hitInfo.HitInfoType)
            {
                case HitInfoType.Cell:
                    this.Cursor = Cursors.Hand;
                    break;
                case HitInfoType.NodeCheckBox:
                    this.Cursor = Cursors.PanEast;
                    break;
                default:
                    this.Cursor = Cursors.Default;
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void call_treeList_Click(object sender, EventArgs e)
        {
            TreeListNode clickedNode = this.treeList.FocusedNode;
            string disPlayText = clickedNode.GetDisplayText("name");
            textBox1.AppendText("You clicked " + disPlayText);
        }

        //向下
        private void btnMoveDown_Click(object sender, EventArgs e)
        {
 
        }

        private void buttonEdit1_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {

        }

        private void buttonEdit1_EditValueChanged(object sender, EventArgs e)
        {
            var operation = new FilterNodeOperation(buttonEdit1.EditValue != null ? buttonEdit1.EditValue.ToString() : "");
            treeList.NodesIterator.DoOperation(operation);
        }

        private void buttonEdit1_DoubleClick(object sender, EventArgs e)
        {
            buttonEdit1.Text = "";
        }

        private void treeList1_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var node = treeList.FindNodeByFieldValue("name", "我块77");
            treeList.SetFocusedNode(node);
        }
    }
}
