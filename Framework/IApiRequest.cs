using System.Collections.Generic;

namespace Framework
{
    public interface IApiRequest
    {
        long APPID { get; set; }
        string KEY { get; set; }
        string Url { get; set; }
        string Method { get; set; }
        string TKK { get; set; }
        string ContentType { get; set; }
        string Charset { get; set; }
        IDictionary<string, string> Headers { get; set; }
        byte[] Body { get; set; }


    }
}
