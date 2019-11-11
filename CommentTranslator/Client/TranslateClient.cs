﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using CommentTranslator.Option;
using Framework;
using CommentTranslator.Util;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;

namespace CommentTranlsator.Client
{
    public class TranslateClient : ApiClient
    {
        private const string CONTENT_TYPE = "application/text";
        private const string CHARSET = "UTF-8";
        private const string METHOD = "POST";
        static private Dictionary<string, IAPIResponse> cache;
        private Dictionary<string, IAPIResponse> Cache
        {
            get
            {
                lock(cache)
                {
                    return cache;
                }
            }
            set
            {
                lock(cache)
                {
                    cache = value;
                }
            }
        }

        private Settings _settings;




        public TranslateClient(Settings settings)
        {
            _settings = settings;
            cache = new Dictionary<string, IAPIResponse>();
        }

        /// <summary>
        /// 传入待翻译文本，返回翻译内容
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task<IAPIResponse> Translate(string text)
        {
            if (cache.ContainsKey(text))
            {
                return Cache[text];
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_settings.TKK)) throw new ArgumentNullException("TKK", "TKK值不能为空");
                var request = new ApiRequest()
                {
                    ContentType = CONTENT_TYPE,
                    Charset = CHARSET,
                    Method = METHOD,
                    TKK = _settings.TKK,
                    Url = _settings.TranslateUrl,
                    Body = Encoding.UTF8.GetBytes(text),  //待翻译文本
                    Headers = new Dictionary<string, string>()
                };

                request.Headers.Add("from-language", _settings.AutoDetect ? "auto" : _settings.TranslateFrom);
                request.Headers.Add("to-language", _settings.TranslateTo);
                IAPIResponse aPIResponse = await Execute(request);
                // request.Headers.Add("auto-detect-language", _settings.AutoDetect.ToString());
                Cache.Add(text, aPIResponse);
                return aPIResponse;
            }
        }
    }
}
