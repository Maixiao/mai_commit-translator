using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace Framework
{
    public interface IAPIResponse
    {
        int Code { get; set; }
        string Data { get; set; }
        IDictionary<string, object> Tags { get; set; }
        string Message { get; set; }
    }


    public class ApiResponse : IAPIResponse
    {
        public int Code { get; set; }
        public string Data { get; set; }
        public IDictionary<string, object> Tags { get; set; } = new Dictionary<string, object>();
        public string Message { get; set; }
    }

    public abstract class ApiClient
    {
        private Dictionary<string, Task<IAPIResponse>> keyValuePairs;
        private string tmpfile;

        public async Task<IAPIResponse> GetResponse(string Msg)
        {
            var apiResult = new ApiResponse();
            apiResult.Code = (int)0;
            apiResult.Message = "";
            apiResult.Data = Msg;
            apiResult.Tags.Add("from-language", "en");
            apiResult.Tags.Add("to-language", "zh-CN");
            apiResult.Tags.Add("translate-success", "true");
            return apiResult;
        }

        public ApiClient()
        {
            keyValuePairs = new Dictionary<string, Task<IAPIResponse>>();
            try
            {
                string tmp = Environment.GetEnvironmentVariable("TEMP");
                DirectoryInfo info = new DirectoryInfo(tmp);
                tmpfile = info.FullName + "\\trans.data";
                if (File.Exists(tmpfile))
                {
                    FileStream fs = new FileStream(tmpfile, FileMode.Open);
                    BinaryFormatter bf = new BinaryFormatter();
                    var newKeyValue = bf.Deserialize(fs) as Dictionary<string, string>;
                    if (newKeyValue != null)
                    {
                        foreach (var item in newKeyValue)
                        {
                            keyValuePairs.Add(item.Key, Task.Run(() => GetResponse(item.Value)));
                        }
                    }
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("vs::" + ex.Message);
            }
        }
        ~ApiClient()
        {
            try
            {
                var KeyValue = new Dictionary<string, string>();
                foreach (var item in keyValuePairs)
                {
                    if (item.Value.IsCompleted == true)
                    {
                        KeyValue.Add(item.Key, item.Value.Result.Data);
                    }
                }
                Stream fStream = new FileStream(tmpfile, FileMode.Create, FileAccess.Write);
                BinaryFormatter binFormat = new BinaryFormatter();
                binFormat.Serialize(fStream, KeyValue);
                fStream.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("vs::" + ex.Message);
            }
        }

        protected virtual async Task<IAPIResponse> ExecuteAsync(IApiRequest request)
        {
            Task<IAPIResponse> asyncRes = null;
            string txt = Encoding.UTF8.GetString(request.Body);
            lock (keyValuePairs)
            {
                if (keyValuePairs.ContainsKey(txt))
                {
                    asyncRes = keyValuePairs[txt];
                }
                else
                {
                    asyncRes = Task.Run(() => Execute(request));
                    keyValuePairs.Add(txt, asyncRes);
                }
            }
            return await asyncRes;
        }

        protected virtual async Task<IAPIResponse> Execute(IApiRequest request)
        {
            string result = string.Empty;
            var apiResult = new ApiResponse();
            apiResult.Code = -1;
            apiResult.Message = "";
            apiResult.Data = "";

            string txt = Encoding.UTF8.GetString(request.Body);

            if (request.Url == "google")
            {
                try
                {
                    string mess = txt;
                    string tkk = request.TKK;
                    var tk = GetTKHelper.GetTK(txt, tkk);
                    string paramData = "client=webapp&sl=en&tl=zh-CN&hl=zh-CN&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&otf=1&ssel=3&tsel=3&kc=1&tk=" + tk + "&q=" + mess;

                    HttpWebRequest wbRequest = (HttpWebRequest)WebRequest.Create("https://translate.google.cn/translate_a/single");
                    wbRequest.Method = "POST";
                    wbRequest.ContentType = "application/x-www-form-urlencoded";
                    wbRequest.Accept = "*/*";
                    wbRequest.Referer = "https://translate.google.cn/";
                    wbRequest.Host = "translate.google.cn";
                    wbRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    wbRequest.Headers.Add("Accept-Language", "zh-Hans-CN, zh-Hans; q=0.5");
                    wbRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 UBrowser/6.2.4094.1 Safari/537.36";
                    wbRequest.ContentLength = Encoding.UTF8.GetByteCount(paramData);
                    using (Stream requestStream = wbRequest.GetRequestStream())
                    {
                        using (StreamWriter swrite = new StreamWriter(requestStream))
                        {
                            swrite.Write(paramData);
                        }
                    }

                    using (HttpWebResponse response = (HttpWebResponse)wbRequest.GetResponse())
                    {
                        string ContentEncoding = response.ContentEncoding.ToLower();
                        Stream stream = null;
                        if (ContentEncoding.Contains("gzip"))
                        {
                            stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                        }
                        else if (ContentEncoding.Contains("deflate"))
                        {
                            stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress);
                        }
                        else
                        {
                            stream = response.GetResponseStream();
                        }
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            result = reader.ReadToEnd();
                        }
                        dynamic tempResult = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                        //解决“企业\r\n学习”多行文本没有显示全部翻译
                        var resarry = Newtonsoft.Json.JsonConvert.DeserializeObject(tempResult[0].ToString());
                        var length = (resarry.Count) - 1;
                        StringBuilder str = new StringBuilder();
                        for (int i = 0; i < length; i++)
                        {
                            var res = Newtonsoft.Json.JsonConvert.DeserializeObject(resarry[i].ToString());
                            str.Append(res[0].ToString());
                        }

                        apiResult.Code = (int)response.StatusCode;
                        apiResult.Message = response.StatusCode.ToString();
                        apiResult.Data = str.ToString();
                        apiResult.Tags.Add("from-language", "en");
                        apiResult.Tags.Add("to-language", "zh-CN");
                        apiResult.Tags.Add("translate-success", "true");
                    }
                }
                catch (Exception ex)
                {
                    apiResult.Code = -1;
                    apiResult.Message = ex.Message;
                    apiResult.Data = "";
                }
            }
            else if(request.Url == "baidu")
            {
                try
                {
                    Request r = new Request();
                    r.from = "en";
                    r.to = "zh";
                    r.q = txt;
                    r.appid = request.APPID;
                    r.key = request.KEY;
                    Result res = r.Post<Result>();
                    int length = res.trans_result.Count;

                    StringBuilder str = new StringBuilder();
                    for (int i = 0; i < length; i++)
                    {
                        str.Append(res.trans_result[i].dst);
                        if(i < length - 1)
                        {
                            str.Append("\r\n");
                        }
                    }

                    apiResult.Code = (int)0;
                    apiResult.Message = "";
                    apiResult.Data = str.ToString();
                    apiResult.Tags.Add("from-language", "en");
                    apiResult.Tags.Add("to-language", "zh-CN");
                    apiResult.Tags.Add("translate-success", "true");
                }
                catch (Exception ex)
                {
                    apiResult.Code = -1;
                    apiResult.Message = ex.Message;
                    apiResult.Data = "";
                }
            }
            return apiResult;
        }
    }
}
