using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using AndoIt.Common.Common;
using AndoIt.Common.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AndoIt.Common
{
	public class GoodConfigOnResapi : ConfigParser
	{
		private readonly IIoCObjectContainer ioCObjectContainer;
		private readonly ILog log;
		private readonly string url;
		private readonly int secondsToRefreshConfig;
		private IHttpClientAdapter httpClient;
		private string configurationInJson;
		private DateTime dataBaseLastRead;
		private object toLock = new object();

		public GoodConfigOnResapi(IIoCObjectContainer ioCObjectContainer, string url, int secondsToRefreshConfig = 0, IHttpClientAdapter httpClient = null)
		{	
			this.ioCObjectContainer = ioCObjectContainer ?? throw new ArgumentNullException("ioCObjectContainer");
			this.log = this.ioCObjectContainer.Get<ILog>();
			this.log.Info("Start", new StackTrace());
			this.httpClient = httpClient ?? new HttpClientAdapter();
			if (string.IsNullOrWhiteSpace(url))
				throw new ArgumentNullException("url");			
			this.url = url;			
			this.secondsToRefreshConfig = secondsToRefreshConfig;
			this.log.Info($"secondsToRefreshConfig={secondsToRefreshConfig}");			
			this.configurationInJson = new Insister(this.log).Insist<string>(() => GetRootJStringFromRestApi() , 2);
			this.dataBaseLastRead = DateTime.Now;
			this.WriteConfigSafeToLog();
			this.log.Info("End", new StackTrace());
		}

		public ILog Log => log;

		public override string ConnectionString => throw new NotImplementedException("Esta configuración no usa ConnectionString");

		private bool NeedsToReload
		{
			get
			{
				lock (this.toLock)
				{
					return (this.secondsToRefreshConfig != 0 && DateTime.Now >= this.dataBaseLastRead.AddSeconds(this.secondsToRefreshConfig))
									|| (this.secondsToRefreshConfig == 0 && DateTime.Now >= this.dataBaseLastRead.AddHours(1));
				}
			}
		}

        public override void AddUpdateFromJToken(JToken configuration)
		{
			throw new NotImplementedException();
		}

		public override JToken GetJNodeByTagAddress(string tagAddress = null)
		{
			string json = GetRootJString();
			JToken jTokenParsed = JToken.Parse(json);
			JToken jTokenResult = GetJNodeByTagAddressOnJNode(jTokenParsed, tagAddress);
			return jTokenResult ?? throw new ConfigurationErrorsException($"El tagAddress '{tagAddress}' me da nulo en la configuración");
		}

		public JToken GetJNodeByTagAddressOnJNode(JToken jToken, string tagAddress)
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

		public override XmlNode GetXmlNodeByTagAddress(string tagAddress)
		{
			var content = JToken.Parse(GetRootJString());
			var jObject = new JObject();
			jObject["root"] = content;
			XmlDocument doc = JsonConvert.DeserializeXmlNode(jObject.ToString());
			return doc.ChildNodes[0][tagAddress];
		}

		private string GetRootJString()
        {
            //	Refresh config each this.secondsToRefreshConfig OR each hour by default 
            if (NeedsToReload)
            {
                ReloadConfig();
            }

            lock (this.toLock)
            {
                return this.configurationInJson;
            }
        }

        public override void ReloadConfig()
		{
			this.log.Info("Start", new StackTrace());
			Task.Factory.StartNew(() =>
			{
				this.log.Info("ReloadConfig.Action: Start. Antes de leer la config", new StackTrace());
				lock (this.toLock)
				{
					try
					{
						this.configurationInJson = new Insister(this.log).Insist<string>(() => GetRootJStringFromRestApi(), 2);
						this.dataBaseLastRead = DateTime.Now;
						this.WriteConfigSafeToLog();
					}
					catch (Exception ex)
					{
						this.log.Error($"No se ha podido cargar la configuración. Seguirá con la vieja", ex);
					}
				}
				this.log.Info("ReloadConfig.Action: End. Después de leer la config", new StackTrace());
			});			
			this.log.Info("End", new StackTrace());
		}

		private string GetRootJStringFromRestApi()
		{
			try
			{
				var httpResult = this.httpClient.AllCookedUpGet(this.url);
				return httpResult;
			} catch (Exception ex) {
				string message = $"Error inesperado al obtener configuración de restapi en {this.url}";
				this.log.Error(message, ex, new StackTrace());
				throw new ConfigurationErrorsException(message, ex);
			}
		}

		private void WriteConfigSafeToLog()
		{
			string url = this.url;
			string configurationInJson = this.configurationInJson;
			try
			{
				var configLogForbiddenWords = this.GetAsStringList("log.forbiddenWords");
				if (configLogForbiddenWords != null)
				{
					configLogForbiddenWords.ForEach(x =>
					{
						url = url.Replace(x, "XXXXXXXXXXXXX");
						configurationInJson = configurationInJson.Replace(x, "XXXXXXXXXXXXX");
					});
				}
			}
			catch (ConfigurationErrorsException ex)
			{
				if (ex.Message.Contains("No existe"))
					this.log.Warn($"log.forbiddenWords no existe en la configuración. ¿Falta ocular paswords en el log?", ex, new StackTrace());
				else
					throw;
			}
			this.log.Debug($"url: {url}", new StackTrace());
			this.log.Debug($"Esta es la configuración leída de la BD: {Environment.NewLine}{configurationInJson}", new StackTrace());
		}
	}
}