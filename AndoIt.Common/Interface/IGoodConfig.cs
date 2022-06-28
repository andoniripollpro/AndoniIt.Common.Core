using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Xml;

namespace AndoIt.Common.Interface
{
	public interface IGoodConfig
	{
		string ConnectionString { get; }

		string GetAsString(string tagAddress, string defaultValue = null);
		int GetAsInt(string tagAddress);
		List<string> GetAsStringList(string tagAddress, char separator = ',');

		XmlNode GetXmlNodeByTagAddress(string tagAddress);
		JToken GetJNodeByTagAddress(string tagAddress = null);
		void AddUpdateFromJToken(JToken configuration);
        void ReloadConfig();
        bool GetAsBool(string v);
    }
}