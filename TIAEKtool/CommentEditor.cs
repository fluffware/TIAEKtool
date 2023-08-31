using System;
using System.Collections.Generic;
using System.Text;

namespace TIAEKtool
{
    public static class CommentEditor
    {
        /// <summary>
        /// Callabck for an annotation, allowing editing the data
        /// </summary>
        /// <param name="type">Annotation type</param>
        /// <param name="data">Data. May be changed</param>
        /// <returns>True to keep annotation, false to delete</returns>
        public delegate bool EditHandler(string type, ref string data);

        
       
        static string EscapeData(string data)
        {
            if (data.Length == 0) return "";
            // First character of data must not be allowed in type names. If it is, add a space.
            char c = data[0];
            return ((Char.IsLetterOrDigit(c) || c == '_') ? " " : "") + data.Replace("}", "\\}").Replace("\\", "\\\\");
        }
        static public void Edit(StringBuilder comment, ICollection<string> types, EditHandler handler)
        {
            int len = comment.Length;
            int pos = 0;
            while (true)
            {

                pos = CommentParser.ParseSingleAnnotation(comment.ToString(), pos, out CommentParser.AnnotationInfo info);
                if (pos < 0) break;
                if (types.Contains(info.type))
                {
                    types.Remove(info.type);
                    if (handler(info.type, ref info.data))
                    {
                        string escaped = EscapeData(info.data);
                        comment.Remove(info.data_start, info.data_len);
                        comment.Insert(info.data_start, escaped);
                        pos = info.data_start + escaped.Length;
                    } else
                    {
                        comment.Remove(info.start, info.len);
                        pos = info.start;
                    }
                }
            }
            foreach (string type in types)
            {
                string data = "";
                if (handler(type, ref data))
                {
                    string escaped = EscapeData(data);
                    comment.Append("@{");
                    comment.Append(type);
                    comment.Append(escaped);
                    comment.Append("}");
                }
            }
        }
    }
}
