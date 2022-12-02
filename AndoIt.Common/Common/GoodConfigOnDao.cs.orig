using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Dapper;
using AndoIt.Common.Common;
using AndoIt.Common.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;

namespace AndoIt.Common
{
	public class GoodConfigOnDao : ConfigParser
	{
		private readonly IIoCObjectContainer ioCObjectContainer;
		private readonly string connectionString;
		private readonly string appId;
		private readonly int secondsToRefreshConfig;
		private readonly ILog log;
		private string configurationInJson;
		private DateTime dataBaseLastRead;

		public GoodConfigOnDao(IIoCObjectContainer ioCObjectContainer, string connectionString, string appId, int secondsToRefreshConfig = 0)
		{
			this.ioCObjectContainer = ioCObjectContainer ?? throw new ArgumentNullException("ioCObjectContainer");
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentNullException("connectionString");
			this.connectionString = connectionString;
			if (string.IsNullOrWhiteSpace(appId))
				throw new ArgumentNullException("appId");
			this.appId = appId ?? throw new ArgumentNullException("appId");
			this.secondsToRefreshConfig = secondsToRefreshConfig;
			this.log = this.ioCObjectContainer.Get<ILog>();
			this.configurationInJson = new Insister(this.log).Insist<string>(() => GetRootJStringFromDB() , 2);
			this.dataBaseLastRead = DateTime.Now;
			this.WriteConfigSafeToLog();
		}

		public ILog Log => log;
		public override string ConnectionString => connectionString;

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
			if ((this.secondsToRefreshConfig != 0 && DateTime.Now >= this.dataBaseLastRead.AddSeconds(this.secondsToRefreshConfig))
				|| (this.secondsToRefreshConfig == 0 && DateTime.Now >= this.dataBaseLastRead.AddHours(1)))
			{
				ReloadConfig();
			}
			return this.configurationInJson;
		}

		public override void ReloadConfig()
		{
			this.configurationInJson = new Insister(this.log).Insist<string>(() => GetRootJStringFromDB(), 2);
			this.dataBaseLastRead = DateTime.Now;
			this.WriteConfigSafeToLog();
		}

		private const string SELECT_CONFIG = "Select CONFIGURACION from VOD_APP_CONFIG " +
			"where APP_ID = '{appId}' " +
			"and (F_I_VIGENCIA is null or SYSDATE >= F_I_VIGENCIA) " +
			"and (F_F_VIGENCIA is null or SYSDATE < F_F_VIGENCIA) ";

		private string GetRootJStringFromDB()
		{
			var connection = new OracleConnection(this.ConnectionString);
			connection.EnsureDatabaseConnection();
			string query = SELECT_CONFIG.Replace("{appId}", this.appId);
			this.log.Info(query, new StackTrace());
			var configurationList = connection.Query<dynamic>(query).ToList();

			string errorMessage = $"Existen {configurationList.Count()} registros de configuración en la base de datos el nombre {this.appId} y rango de fechas actual, y se espera 1 y sólo 1 ";
			if (configurationList.Count() == 0)
				throw new ConfigurationErrorsException(errorMessage);
			else
			{
				string configurationInJson = configurationList[0].CONFIGURACION.ToString();
				if (configurationList.Count() > 1) this.log.Error(errorMessage, null, new StackTrace());
				this.log.Info("Config leída de la BD.");
				return configurationInJson;
			}
		}

		private void WriteConfigSafeToLog()
		{
			string connectionString = this.connectionString;
			string configurationInJson = this.configurationInJson;
			try
			{
				var configLogForbiddenWords = this.GetAsStringList("log.forbiddenWords");
				if (configLogForbiddenWords != null)
				{
					configLogForbiddenWords.ForEach(x =>
					{
						connectionString = connectionString.Replace(x, "XXXXXXXXXXXXX");
						configurationInJson = configurationInJson.Replace(x, "XXXXXXXXXXXXX");
					});
				}
			} catch (ConfigurationErrorsException ex) {
				if (ex.Message.Contains("No existe"))
					this.log.Warn($"log.forbiddenWords no existe en la configuración. ¿Falta ocular paswords en el log?", ex, new StackTrace());
				else
					throw;
			}
			this.log.Debug($"connectionString: {connectionString}", new StackTrace());
			this.log.Debug($"Esta es la configuración leída de la BD: {Environment.NewLine}{configurationInJson}", new StackTrace());
		}
	}
}