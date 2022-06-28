using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace AndoIt.Common.Interface
{
	public interface IFileHelper
	{
		void ValidateFileStructure(List<string> stringList);
		void CopyFromToFixed(string origin, string destination);
		string ReadAllText(string origin);
		void Delete(string convertedToJsonTempDestination);
		void WriteAllText(string convertedToJsonTempDestination, string destinationContentJson, Encoding utf8WithoutBom);
		void WriteAllText(string tempFileName, string v);
		XmlDocument GetXmlDocumentCleaned(string origin);
	}
}
