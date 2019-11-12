using System;
using System.Collections.Generic;

namespace CommentTranslator.Parsers
{
    public class PythonCommentParser : CommentParser
    {
        public PythonCommentParser()
        {
            Tags = new List<ParseTag>
            {
                //Singleline comment
                new ParseTag()
                {
                    Start = "#",
                    End = Environment.NewLine,
                    Name = "singleline"
                },

                new ParseTag()
                {
                    Start = "#",
                    End = "\n",
                    Name = "singleline_ms"
                },

                //Multi line comment
                new ParseTag(){
                    Start = "'''",
                    End = "'''",
                    Name = "multiline"
                },

                new ParseTag()
                {
                    Start = "#",
                    End = "",
                    Name = "singlelineend"
                },
            };
        }
    }
}
