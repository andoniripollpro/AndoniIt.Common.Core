using AndoIt.Common.Common;
using AndoIt.Common.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;

namespace AndoIt.Common
{
	public class GoodConfigOnJsonFile : ConfigParser
	{
		private static IGoodConfig presetConfig = null;
		private ConnectData connecData = null;
		private string configFileName;
		private string configurationInJson;

		public static IGoodConfig CreateConfig
		{
			get
			{
				if (GoodConfigOnJsonFile.presetConfig is null)
					return new GoodConfigOnJsonFile();
				else
					return GoodConfigOnJsonFile.presetConfig;
			}
		}

		public GoodConfigOnJsonFile(ConnectData connectData = null, string configFileName = null)
		{
			this.connecData = connectData;
			this.configFileName = configFileName;
			ReloadConfig();
		}

		public override string ConnectionString => throw new NotImplementedException("ConnectionString en GoodConfigOnFiles no se usa");

		public override XmlNode GetXmlNodeByTagAddress(string tagAddress)
		{
			throw new NotImplementedException();
		}				

		public override JToken GetJNodeByTagAddress(string tagAddress)
		{
			JToken jTokenParsed = JToken.Parse(this.configurationInJson);
			JToken jTokenResult = GetJNodeByTagAddressOnJNode(jTokenParsed, tagAddress);
			return jTokenResult ?? throw new ConfigurationErrorsException($"El tagAddress '{tagAddress}' me da nulo en la configuración");
		}

		private JToken GetJNodeByTagAddressOnJNode(JToken jToken, string tagAddress)
		{
			if (tagAddress != null)
			{
				if (!tagAddress.Contains("."))
					return jToken[tagAddress];
				else
				{
					int indexOfDot = tagAddress.IndexOf('.');
					string prefix = tagAddress.Substring(0, indexOfDot);
					string sufix = tagAddress.Substring(indexOfDot + 1, tagAddress.Length - indexOfDot - 1);
					return GetJNodeByTagAddressOnJNode(jToken[prefix], sufix);
				}
			}
			else
				return jToken;
		}

		public override void AddUpdateFromJToken(JToken configuration)
		{
			throw new NotImplementedException();
		}

		public override void ReloadConfig()
		{
			this.configurationInJson = File.ReadAllText(this.configFileName);
		}

		public class ConnectData
		{
			public string Url { get; set; }
			public string User { get; set; }
			public string Pass { get; set; }
		}
	}
}
