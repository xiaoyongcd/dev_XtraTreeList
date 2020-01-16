using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommTreeView
{
    public class DBTreeNodeTag
    {
        //除id,pid,caption3个属性之外的其它固定属性
        public string OrderNum;

        public string NodeDesc; //节点描述

        public string Disable; //禁用标志

        public string Owner; //所有者(名称可与字段名不一致)

        public string Tester;//测试者


        //业务数据(动态)
        public Dictionary<string, string> TagDict;
        
        
    }
}
