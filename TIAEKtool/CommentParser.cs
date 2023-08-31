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

        
        public class AnnotationInfo
        {
            public string type;
            public string data;
            public int start;
            public int len;
            public int data_start;
            public int data_len;
        }

        /// <summary>
        /// Find the first annotation starting at pos
        /// </summary>
        /// <param name="comment">String to search for annotation</param>
        /// <param name="pos">Start position<param>
        /// <param name="info">Annotation, if found<param>
        /// <returns>Position of first character following annotation or -1 if none found</returns>
        /// 
        static public int ParseSingleAnnotation(string comment, int pos, out AnnotationInfo info)
        {
            int len = comment.Length;
            info = null;
            pos = comment.IndexOf("@{", pos);
            if (pos == -1) return -1;
            int annotation_start = pos;
            pos += 2;
            int start = pos;
            while (pos < len)
            {
                char c = comment[pos];
                if (!Char.IsLetterOrDigit(c) && c != '_') break;
                pos++;
            }
            if (pos == len) return -1;

            int data_start = pos;
            StringBuilder data = new StringBuilder();
            string type = comment.Substring(start, pos - start);
            
            while (true)
            {
                start = pos;
                pos = comment.IndexOfAny(new char[] { '}', '\\' }, pos);
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
                    info = new AnnotationInfo
                    {
                        type = type,
                        data = data.ToString(),
                        data_start = data_start,
                        data_len = pos - data_start,
                        start = annotation_start,
                        len = pos + 1 - annotation_start
                    };
                    return pos + 1;
                }
            }
            return -1;
        }
        static public void Parse(string comment, AnnotationHandler handler)
        {
            int len = comment.Length;
            int pos = 0;
            while (true)
            {

                pos = ParseSingleAnnotation(comment, pos, out AnnotationInfo info);
                if (pos < 0) break;
                handler(info.type, info.data);
            }
        }
    }
}
