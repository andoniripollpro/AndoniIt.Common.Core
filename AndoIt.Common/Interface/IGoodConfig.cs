using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Xml;

namespace AndoIt.Common.Interface
{
	public interface IGoodConfig
	{
		string ConnectionString { get; }
        List<string> ForbiddenWords { get; }

        string GetAsString(string tagAddress, string defaultValue = null);
		int GetAsInt(string tagAddress);
		List<string> GetAsStringList(string tagAddress, char separator = ',');
        bool GetAsBool(string tagAddress);

        XmlNode GetXmlNodeByTagAddress(string tagAddress);
		JToken GetJNodeByTagAddress(string tagAddress = null);
		void AddUpdateFromJToken(JToken configuration);
        void ReloadConfig();
        
        string ConfigurationInJson { get; }
    }
}