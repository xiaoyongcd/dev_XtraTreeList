using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CommTreeView
{
    public class HttpServicerHelper
    {
        public static string CallGoService(string url, NameValueCollection postVars)
        {
            var webClientObj = new WebClient();
            byte[] byRemoteInfo = webClientObj.UploadValues(url, "POST", postVars);
            string json = Encoding.UTF8.GetString(byRemoteInfo);
            return json;
        }
    }
}
