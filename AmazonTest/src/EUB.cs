using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace AmazonTest.src
{
    public sealed class EUB
    {
        private static readonly EUB instance = new EUB();
        private string validateUrl;
        private string orderUrl;


        private EUB()
        {
            validateUrl = "api/public/p/validate";
            orderUrl = "api/public/p/order/";
        }

        
        public static EUB Instance
        {
            get
            {
                return instance;
            }
        }

        public string doRequest()
        {
             return doGet("http://www.ems.com.cn/partner/api/public/p/order/LS081844720CN");
        }

        public string doGet(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "GET";
                request.Headers["version"] = "international_eub_us_1.1";
                request.Headers["authenticate"] = "Upright_74574a05d12638a1b62b09fdfe694579";
                WebResponse response = request.GetResponse();
                Stream respStream = response.GetResponseStream();
                using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (System.Exception ex)
            {
                //errorMsg = ex.Message;
            }
            return "";
        }
    }
}
