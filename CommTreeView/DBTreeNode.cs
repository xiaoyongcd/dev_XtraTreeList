using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommTreeView
{
    [Serializable]
    public class DBTreeNode
    {
        #region 数据字段

        //public string _id { get; set; }

        public string n_desc { get; set; }//根据id动态从db中查询
        public string n_id { get; set; }
        public string n_name { get; set; }

        public string n_order { get; set; }
        public string n_pid { get; set; }
        //public string n_status { get; set; }

        public string n_data { get; set; }

        public string n_disable { get; set; }

        public string n_owner { get; set; }//拥有者

        public string n_tester { get; set; } 

        public override string ToString()
        {
            return string.Format("id:{0} pid:{1} name:{2}",n_id,n_pid,n_name);
        }

        #endregion

    }
}
