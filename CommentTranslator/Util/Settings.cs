using CommentTranslator.Option;

namespace CommentTranslator.Util
{
    /// <summary>
    /// 获取工具菜单设置的值
    /// </summary>
    public class Settings
    {

        public long APPID { get; set; }
        public string KEY { get; set; }

        public string TKK { get; set; }
        public string TranslateUrl { get; set; }
        public string TranslateFrom { get; set; }
        public string TranslateTo { get; set; }
        public bool AutoDetect { get; set; }
        public bool AutoTranslateComment { get; set; }

        public bool AutoTextCopy { get; set; }

        /// <summary>
        /// 刷新设置的值
        /// </summary>
        /// <param name="page"></param>
        public void ReloadSetting(OptionPageGrid page)
        {
            TranslateUrl = page.TranslateUrl;//page.TranslateUrl;
            APPID = page.APPID;
            KEY = page.KEY;
            TranslateFrom = page.TranslateFrom;
            TranslateTo = page.TranslatetTo;
            AutoDetect = page.AutoDetect;
            AutoTranslateComment = page.AutoTranslateComment;
            TKK = page.TKK;
            AutoTextCopy = page.AutoTextCopy;
        }
    }
}
