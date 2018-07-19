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
    }
}
