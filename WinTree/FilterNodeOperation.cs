using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList.Nodes.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTree
{
    public class FilterNodeOperation : TreeListOperation
    {
        string pattern;


        public FilterNodeOperation(string _pattern)
        {
            pattern = _pattern;
        }


        public override void Execute(TreeListNode node)
        {
            if (NodeContainsPattern(node, pattern))
            {
                node.Visible = true;

                //if (node.ParentNode != null)
                //    node.ParentNode.Visible = true;

                //必须要递归查找其父节点全部设置为可见
                var pNode = node.ParentNode;
                while (pNode != null)
                {
                    pNode.Visible = true;
                    pNode = pNode.ParentNode;
                }
            }
            else
                node.Visible = false;
        }


        bool NodeContainsPattern(TreeListNode node, string pattern)
        {
            foreach (TreeListColumn col in node.TreeList.VisibleColumns)
            {
                if (node.GetDisplayText(col).Contains(pattern))
                    return true;
            }
            return false;
        }
    }
}
