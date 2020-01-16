using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinHTTP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string url = "http://192.168.33.87:8900/db_query?db_name=d127&format=json";

        private void button1_Click(object sender, EventArgs e)
        {
            var webClientObj = new WebClient();
            var postVars = new NameValueCollection {
                //{"0", "select 1+1 x"}
                {"0", "select * from tree_1 where ifnull(n_status,'1') != '0'"}
            };

            byte[] byRemoteInfo = webClientObj.UploadValues(url, "POST", postVars);
            string json = Encoding.UTF8.GetString(byRemoteInfo);
            textBox1.Text = json;
        }


        private void button2_Click_1(object sender, EventArgs e)
        {
            var jsonText = textBox1.Text;
            var nodeList = JsonConvert.DeserializeObject<List<TreeNode>>(jsonText);
            this.textBox2.AppendText("\r\n");
            foreach (var node in nodeList)
            {
                this.textBox2.AppendText(node.ToString() + "\r\n");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var jsonText = textBox1.Text;
            var nodeList = JsonConvert.DeserializeObject<List<TreeNode>>(jsonText);
            var dt = ListToDataTable<TreeNode>(nodeList);
            dataGridView1.DataSource = dt;
            dataGridView1.Refresh();

        }



        public static DataTable ListToDataTable<T>(IEnumerable<T> collection)
        {
            var props = typeof(T).GetProperties();
            var dt = new DataTable();
            dt.Columns.AddRange(props.Select(p => new DataColumn(p.Name, p.PropertyType)).ToArray());
            if (collection.Count() > 0)
            {
                for (int i = 0; i < collection.Count(); i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in props)
                    {
                        object obj = pi.GetValue(collection.ElementAt(i), null);
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    dt.LoadDataRow(array, true);
                }
            }
            return dt;
        }

   
    }
}

/*
 *go:建议打印所执行的sql语句，最好带参数。 
 * 
 * 
 */
