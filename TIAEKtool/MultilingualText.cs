using System;
using System.Collections.Generic;



namespace TIAEKtool
{
    public class MultilingualText
    {
        Dictionary<string,string> texts = new Dictionary<string, string>();
        
        public MultilingualText()
        {
        }

        public MultilingualText(string culture, string text)
        {
            AddText(culture, text);
        }

        public void AddText(string culture, string text)
        {
            texts.Add(culture, text);
        }

        public string this[string culture]
        {
            get
            {
                string value;
                if (culture == null) return "";
                if (texts.TryGetValue(culture, out value)) return value;
                if (texts.TryGetValue("default", out value)) return value; 
                return "";
            }
            set
            {
                texts[culture] = value;
            }
        }

        public bool TryGetText(string culture, out string text)
        {
            return texts.TryGetValue(culture, out text);
        }

        public string [] Cultures { get
            {
                Dictionary<string, string>.KeyCollection keys = texts.Keys;
                string[] cultures = new string[keys.Count];
                keys.CopyTo(cultures, 0);
                return cultures;
            } }


        public void AddMissingCultures(IEnumerable<String> cultures, string default_culture)
        {
            Dictionary<string, string> new_texts = new Dictionary<string, string>();
            foreach (string culture in cultures)
            {
                if (texts.TryGetValue(culture, out string value))
                {
                    new_texts.Add(culture, value);
                } else {
                    if (texts.TryGetValue(default_culture, out value))
                    {
                        new_texts.Add(culture, value);
                    }
                    else
                    {
                        // Use an arbitrary culture
                        IEnumerator<string> s = texts.Values.GetEnumerator();
                        if (s.MoveNext())
                        {
                            new_texts.Add(culture, s.Current);
                        }
                        else
                        {
                            new_texts.Add(culture, "");
                        }

                    }

                }
            }
            texts = new_texts;
        }
    }
}
