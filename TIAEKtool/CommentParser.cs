using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIAEKtool
{
    public static class CommentParser
    {
        public delegate void AnnotationHandler(string type, string data);
        static public void Parse(string comment, AnnotationHandler handler)
        {
            int len = comment.Length;
            int pos = 0;
            while (true)
            {

                pos = comment.IndexOf("@{", pos);
                if (pos == -1) break;
                pos += 2;
                int start = pos;
                while (pos < len)
                {
                    char c = comment[pos];
                    if (!Char.IsLetterOrDigit(c) && c != '_') break;
                    pos++;
                }
                if (pos == len) break;

                StringBuilder data = new StringBuilder();
                string type = comment.Substring(start, pos - start);
                while (true)
                {
                    start = pos;
                    pos = comment.IndexOfAny(new char[] { '}', '\\' },pos);
                    if (pos == -1) break;
                    data.Append(comment.Substring(start, pos - start));
                    if (comment[pos] == '\\')
                    {
                        pos++;
                        if (pos >= len) break;
                        data.Append(comment[pos]);
                        pos++;
                    }
                    else
                    {
                        break;
                    }
                }
                pos++;
                handler(type, data.ToString());
            }
        }
    }
}
