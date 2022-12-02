using System;
using System.Text;

namespace AndoIt.Common
{
    public class StringAdapter
    {
        private string stringDecorated;
        public StringAdapter(string str)
        {
            if (str == null)
                str = string.Empty;

            this.stringDecorated = str;
        }

        public string SubStringTruncated(int start, int numberChar = int.MaxValue)
        {
            return this.stringDecorated.SubStringTruncated(start, numberChar);
        }

        public string FromPascalCaseToText()
        {
            return FromPascalCaseToText(this.stringDecorated);
        }

        public string FromPascalCaseToText(string processing)
        {
            for (int i = 1; i < processing.Length; i++)
            {
                if (char.IsUpper(processing[i]) && processing[i - 1] != ' ')
                    return FromPascalCaseToText(processing.Insert(i, " "));
            }
            return processing;
        }

        public string RemoveSpecialCharacters()
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in this.stringDecorated)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')
                    || c == '.' || c == '_' || c == '-' || c == ',' || c == ':' || c == ';'
                    || c == 'ñ' || c == 'Ñ'
                    || c == 'á' || c == 'é' || c == 'í' || c == 'ó' || c == 'ú'
                    || c == 'Á' || c == 'É' || c == 'Í' || c == 'Ó' || c == 'Ú'
                    || c == 'ü' || c == 'Ú')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public StringAdapter SubStringTruncatedDecorated(int start, int numberChar)
        {
            return new StringAdapter(SubStringTruncated(start, numberChar));
        }
        public string CutToSting(string cutToHere)
        {
            int cutToHereIndex = this.stringDecorated.IndexOf(cutToHere);
            string result = this.SubStringTruncated(cutToHereIndex + cutToHere.Length, int.MaxValue);
            return result;
        }
        public StringAdapter CutToStingDecorated(string cutToHere)
        {
            return new StringAdapter(this.CutToSting(cutToHere));
        }
        public string CutFromSting(string cutFromHere)
        {
            int cutFromHereIndex = this.stringDecorated.IndexOf(cutFromHere);
            if (cutFromHereIndex != -1)
                return this.SubStringTruncated(0, cutFromHereIndex);
            else
                return this.stringDecorated;
        }
        public StringAdapter CutFromStingDecorated(string cutFromHere)
        {
            return new StringAdapter(this.CutFromSting(cutFromHere));
        }
    }
}
