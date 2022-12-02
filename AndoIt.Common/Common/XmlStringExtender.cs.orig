namespace AndoIt.Common
{
	public static class XmlStringExtender
	{
		public static string CleanSpacesXmlNMore(this string str)
		{
			return str.Replace("\t", string.Empty).Replace(@"<?xml version=""1.0"" encoding=""utf-8""?>", string.Empty).Replace("  ", " ").Replace(" <", "<").Replace(" />", "/>");															
		}
	}
}
