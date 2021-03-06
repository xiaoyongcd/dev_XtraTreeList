﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbTree
{
    [Serializable]
    public class TreeNode
    {
        //public string _id { get; set; }
        //public string n_desc { get; set; }
        public string n_id { get; set; }
        public string n_name { get; set; }
        //public string n_order { get; set; }
        public string n_pid { get; set; }
        //public string n_status { get; set; }

        public override string ToString()
        {
            return string.Format("id:{0} pid:{1} name:{2}",n_id,n_pid,n_name);
        }


    }
}
