using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTree
{

    public class Data
    {
        public int ID { get; set; }	//数据ID，主键
        public string Name { get; set; }	//数据名称
        public int GroupId { get; set; }	//分组ID，当前位于树形菜单第几级的意思
        public int ParentID { get; set; }	//父标签ID，父标签的数据ID

    }
}
