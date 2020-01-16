using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommTreeView
{
    /// <summary>
    /// 与数据相关的Helper
    /// </summary>
    public class DataHelper
    {
        public static string DictToString(Dictionary<string, string> dict)
        {
            string result = "";
            if (dict!=null && dict.Count > 0)
            {
                result = JsonConvert.SerializeObject(dict);
            }            
            return result;
        }

        public static Dictionary<string, string> StringToDict(string jsonData)
        {
            Dictionary<string, string> dictTag = null;
            if (jsonData != "")
            {
                dictTag = new Dictionary<string, string>();
                try
                {
                    var dictData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
                    foreach (KeyValuePair<string, string> kvp in dictData)
                    {
                        dictTag[kvp.Key] = kvp.Value;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("n_data字段中的内容不是json格式，请检查:\r\n{0}",jsonData));
                }
            }
            return dictTag;
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
