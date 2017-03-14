using System;
using System.Text;

namespace MGM.Localization
{
    public class LS : ILocalizedString
    {
        private readonly string[] _strs;

        public LS(string[] strs)
        {
            _strs = strs;
        }

        public string GetLocalizedString()
        {
            return _strs[LocalizedStrings.Language].Replace("_","__");
        }

        public override string ToString()
        {
            return GetLocalizedString();
        }

        public static implicit operator string(LS ls)
        {
            return ls.GetLocalizedString();
        }

        public static string Escape(string str)
        {
            var sb = new StringBuilder(str);
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == '_')
                {
                    if (i + 1 == sb.Length || sb[i + 1] != '_')
                    {
                        sb.Insert(i, @"\");
                    }
                    i++;
                }
            }

            return sb.ToString().Replace("__", "_");
        }
    }
}