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

        public static string ConvertUnicodeToAscii(this string unicodeString)
        {
            if (unicodeString == null)
            {
                return null;
            }

            byte[] bytes = Encoding.Unicode.GetBytes(unicodeString);
            byte[] asciiBytes = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, bytes);
            char[] asciiChars = new char[asciiBytes.Length];
            int charCount = Encoding.ASCII.GetDecoder().GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            return new string(asciiChars, 0, charCount);
        }

        public static string ConvertUnicodeToUtf8WithoutBom(this string unicodeString)
        {
            if (unicodeString == null)
            {
                return null;
            }

            // Crear una instancia de UTF8Encoding sin incluir el BOM
            Encoding utf8WithoutBom = new UTF8Encoding(false);

            // Convertir la cadena Unicode a un array de bytes UTF-8
            byte[] utf8Bytes = utf8WithoutBom.GetBytes(unicodeString);

            return utf8WithoutBom.GetString(utf8Bytes);
        }
    }
}
