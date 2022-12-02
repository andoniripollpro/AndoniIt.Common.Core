using System.Text;
using System.Xml;

namespace AndoIt.Common
{
    public static class XmlStringExtender
    {
        public static string CleanSpacesXmlNMore(this string str)
        {
            return str.Replace("\t", string.Empty).Replace(@"<?xml version=""1.0"" encoding=""utf-8""?>", string.Empty).Replace("  ", " ").Replace(" <", "<").Replace(" />", "/>");
        }
        public static string Indent(this string str)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(str);
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            string result = sb.ToString();
            result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"no\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            result = result.Replace("xmlns:json=\"http://james.newtonking.com/projects/json\" />", "/>");
            return result;
        }
    }
}
