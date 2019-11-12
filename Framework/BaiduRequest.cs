using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Web.Script.Serialization;

namespace Framework
{
    class Result
    {
        /// <summary>
        /// 翻译源语言
        /// </summary>
        public string from { get; set; }

        /// <summary>
        /// 译文语言
        /// </summary>
        public string to { get; set; }

        public List<TransResult> trans_result { get; set; }
    }

    class TransResult
    {
        /// <summary>
        /// 原文
        /// </summary>
        public string src { get; set; }
        /// <summary>
        /// 译文
        /// </summary>
        public string dst { get; set; }
    }

    public static class PostRequestJson
    {
        private static JavaScriptSerializer jss = new JavaScriptSerializer();

        /// <summary>
        /// 执行请求,请求接口失败时抛出异常
        /// </summary>
        /// <typeparam name="T">请求的类型</typeparam>
        /// <typeparam name="S">返回的类型</typeparam>
        /// <param name="requestType">接口名称</param>
        /// <param name="obj">请求的参数Json对象</param>
        /// <returns>返回的类型的对象</returns>
        public static S Request<T, S>(T obj)
        {
            string str = jss.Serialize(obj);
            string paraUrlCoded = KeyValueCombination(str);

            string strURL = @"http://api.fanyi.baidu.com/api/trans/vip/translate";

            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)WebRequest.Create(strURL);
            request.UserAgent = "User-Agent:Mozilla/5.0 (Windows NT 5.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "POST";
            request.Proxy = null;
            request.Timeout = 60000;
            byte[] payload = System.Text.Encoding.UTF8.GetBytes(paraUrlCoded);
            request.ContentLength = payload.Length;
            System.IO.Stream writer = request.GetRequestStream();
            writer.Write(payload, 0, payload.Length);
            writer.Close();
            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string responseText = myreader.ReadToEnd();
            myreader.Close();

            return jss.Deserialize<S>(responseText);
        }


        private static string KeyValueCombination(string str)
        {
            string paraUrlCoded = "";
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> json = (Dictionary<string, object>)serializer.DeserializeObject(str);
            foreach (var j in json)
            {
                paraUrlCoded += System.Web.HttpUtility.UrlEncode(j.Key);
                if (j.Value.GetType() == typeof(object[]))
                {
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(jss.Serialize(j.Value)) + "&";
                }
                else
                {
                    paraUrlCoded += "=" + System.Web.HttpUtility.UrlEncode(j.Value.ToString()) + "&";
                }
            }

            if (!string.IsNullOrWhiteSpace(paraUrlCoded))
            {
                paraUrlCoded = paraUrlCoded.Substring(0, paraUrlCoded.Length - 1);
            }

            return paraUrlCoded;
        }

    }


    class Request
    {
        public string q { get; set; }

        public string from { get; set; }

        public string to { get; set; }

        public long appid { get; set; }
        public string key { get; set; }


        public long salt
        {
            get
            {
                return (long)ConvertDateTimeInt(DateTime.Now);
            }
        }
        public string sign
        {
            get
            {
                //#error 在这里需要填写你的应用密钥
                return UserMd5(string.Format("{0}{1}{2}{3}", new object[] { appid, q, salt, key }));
            }

        }

        /// <summary>
        /// 执行请求
        /// </summary>
        /// <typeparam name="S">返回的序列化类型</typeparam>
        /// <returns></returns>
        /// <exception cref="Exception">存在请求异常，需要捕捉</exception>
        public S Post<S>()
        {
            return PostRequestJson.Request<Request, S>(this);
        }

        /// <summary>
        /// MD5　32位加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected string UserMd5(string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符
                pwd = pwd + s[i].ToString("x2");
            }
            return pwd;
        }

        /// <summary>
        /// 时间转时间戳
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        protected double ConvertDateTimeInt(System.DateTime time)
        {
            double intResult = 0;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            intResult = (time - startTime).TotalSeconds;
            return intResult;
        }
    }
}
