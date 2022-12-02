using AndoIt.Common.Common;
using AndoIt.Common.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;

namespace AndoIt.Common
{
	public class GoodConfigOnFiles : ConfigParser
	{
		private static IGoodConfig presetConfig = null;
		private ConnectData connecData = null;
		private string configFileName;

		public static IGoodConfig CreateConfig
		{
			get
			{
				if (GoodConfigOnFiles.presetConfig is null)
					return new GoodConfigOnFiles();
				else
					return GoodConfigOnFiles.presetConfig;
			}
		}

		public GoodConfigOnFiles(ConnectData connectData = null, string configFileName = null)
		{
			this.connecData = connectData;
			this.configFileName = configFileName;
		}

		public override string ConnectionString => throw new NotImplementedException("ConnectionString en GoodConfigOnFiles no se usa");

		public override XmlNode GetXmlNodeByTagAddress(string tagAddress)
		{
			//var stackTrace = new StackTrace();
			var callingAssembly = Assembly.GetCallingAssembly();
			XmlDocument doc = GetXmlDocFromConfig(callingAssembly); //, stackTrace);

			//string callingClassFullName = new StackTrace().GetFrame(1).GetType().FullName;
			//string nameSpaceWithoutAssembly = new StringDecorator(callingClassFullName).SubStringTruncated(assembly.FullName.Length + 1, 9999); 

			XmlNode node = GetNodeFromAddress(doc["configuration"]["applicationSettings"], tagAddress);

			return node;
		}
				
		private XmlDocument GetXmlDocFromConfig(Assembly callingAssembly) //, StackTrace stackTrace)
		{
			string configFileContent = null;
			XmlDocument doc = new XmlDocument();			
			if (this.connecData == null)
			{
				if (this.configFileName == null)
				{
					if (callingAssembly.GetHashCode() == new StackTrace().GetFrame(new StackTrace().FrameCount - 1).GetType().Assembly.GetHashCode()
						&& Process.GetCurrentProcess().IsAsp())
					{
						//	TODO?
						throw new NotImplementedException("Con Core no se puede acceder a web.config de la manera que lo hacíamos antes");
						//configFileContent = System.Web.HttpContext.Current.Server.MapPath("~/web.config");
					}
					else
					{
						string configFileName = $"{Path.GetFileName(callingAssembly.Location)}.config";
						if (Process.GetCurrentProcess().IsAsp())
						{
							//	TODO?
							throw new NotImplementedException("Con Core no se puede acceder a la carpeta '~' de la manera que lo hacíamos antes");
							//configFileName = System.Web.HttpContext.Current.Server.MapPath($"~/bin/{configFileName}");
						}
						configFileContent = File.ReadAllText(configFileName);
					}
				}
				else
					configFileContent = File.ReadAllText(this.configFileName);
			}
			else
			{
				NetworkCredential credentials = null;
				if (!string.IsNullOrWhiteSpace(connecData.User))
				{
					credentials = new NetworkCredential()
					{
						UserName = connecData.User,
						Password = connecData.Pass
					};
				}
				string configFileName = this.configFileName == null ? $"{Path.GetFileName(callingAssembly.Location)}.config" : this.configFileName;
				configFileContent = new HttpClientAdapter().AllCookedUpGet($"{this.connecData.Url}/{configFileName}", credentials);				
			}
			var reader = XmlReader.Create(new StringReader(configFileContent), new XmlReaderSettings() { IgnoreComments = true });			
			doc.Load(reader);
			return doc;
		}

		private XmlNode GetNodeFromAddress(XmlNode node, string tagAddress)
		{
			return GetNodeFromAddress(node, tagAddress.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries).ToList());			
		}

		private XmlNode GetNodeFromAddress(XmlNode node, List<string> tagAddress)
		{
			XmlNode next;
			if (tagAddress.Count == 1)
				next = node[tagAddress[0]];
			else
				next = GetNodeFromAddress(node[tagAddress[0]], tagAddress.Skip(1).ToList());
			return next;
		}

		public override JToken GetJNodeByTagAddress(string tagAddress)
		{
			string json = JsonConvert.SerializeXmlNode(this.GetXmlNodeByTagAddress(tagAddress));
			return JToken.Parse(json);
		}

		public override void AddUpdateFromJToken(JToken configuration)
		{
			throw new NotImplementedException();
		}

		public override void ReloadConfig()
		{
			//	No hay necesidad de hacer esto: Cada vez que lee un valor vuelve a leer el fichero
		}

		public class ConnectData
		{
			public string Url { get; set; }
			public string User { get; set; }
			public string Pass { get; set; }
		}
	}
}
