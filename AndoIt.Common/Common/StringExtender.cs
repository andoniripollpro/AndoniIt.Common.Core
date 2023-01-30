using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace AndoIt.Common
{	
    public static class StringExtender
    {
        public static string SubStringTruncated(this string extended, int start, int lenthToCut = int.MaxValue)
        {
            int extendedLengh = extended.Length;
            int maxNumChar = extendedLengh >= (long)lenthToCut + start ? lenthToCut : extendedLengh - start;
            if (maxNumChar < 0)
                return string.Empty;
            else
                return extended.Substring(start, maxNumChar);
        }

        public static string SubStringTruncated(this string extended, string startWord, string endWord = null)
        {
            if (string.IsNullOrWhiteSpace(startWord)) startWord = string.Empty;
            int startIndex = extended.Contains(startWord) ? extended.IndexOf(startWord) + startWord.Length : 0;
            // OJO: No he hecho el test unitario
            string onProcess = extended.SubStringTruncated(startIndex);
            int resultLength = (endWord == null || !onProcess.Contains(endWord))
                ? int.MaxValue : onProcess.IndexOf(endWord);
            return onProcess.SubStringTruncated(0, resultLength);
        }

        public static string FromCamelCaseToPhrase(this string extended)
        {
            List<char> phrase = new List<char>();

            int i = 0;
            extended.ToCharArray().ToList().ForEach(x => {
                if (Char.IsLower(x) || i == 0)
                {
                    phrase.Add(x);
                }
                else
                {
                    phrase.Add(' ');
                    phrase.Add(Char.ToLower(x));
                }
                i++;
            });

            if (phrase.Count > 0)
                phrase[0] = Char.ToUpper(phrase[0]);

            return new string(phrase.ToArray());
        }

        public static string TextToJsonValidValue ( this string extended)
        {
            //var result = HttpUtility.JavaScriptStringEncode(extended);
            var result = new string(extended.Where(c => !char.IsControl(c)).ToArray());
            return result;
            //return extended.Replace("\"", "''");
        }

        public static Stream GenerateStream(this string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

    }
}
