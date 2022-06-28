using AndoIt.Common.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;

namespace AndoIt.Common.Common
{
    public abstract class ConfigParser : IGoodConfig
    {
        public abstract string ConnectionString { get; }

        public abstract void AddUpdateFromJToken(JToken configuration);
        public abstract JToken GetJNodeByTagAddress(string tagAddress = null);
        public abstract XmlNode GetXmlNodeByTagAddress(string tagAddress);
        public abstract void ReloadConfig();
        
        public int GetAsInt(string tagAddress)
        {            
            try
            {
                return int.Parse(this.GetJNodeByTagAddress(tagAddress).ToString());
            }
            catch(Exception ex)  
            {
                throw new ConfigurationErrorsException($"No existe, o no está bien expresado (int), el valor con tagAddress '{tagAddress}' en la configuración", ex);
            }
        }
        public string GetAsString(string tagAddress, string defaultValue = null)
        {
            try
            {
                return this.GetJNodeByTagAddress(tagAddress).ToString();
            }
            catch (Exception ex)
            {
                if (defaultValue is null)
                    throw new ConfigurationErrorsException($"No existe, o no está bien expresado (string), el valor con tagAddress '{tagAddress}' en la configuración", ex);
                else
                    return defaultValue;
            }
        }
        public bool GetAsBool(string tagAddress)
        {
            try
            {
                return bool.Parse(this.GetAsString(tagAddress));
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException($"No existe, o no está bien expresado (bool), el valor con tagAddress '{tagAddress}' en la configuración", ex);
            }
        }        
        public List<string> GetAsStringList(string tagAddress, char separator = ',')
        {
            try
            {
                string value = this.GetAsString(tagAddress);
                value = value?.Replace("[", "").Replace("]", "");
                return new List<string> (value?.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries)).Select(x => x.Trim()).ToList();
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException($"No existe, o no está bien expresado (List<string> con separador '{separator}'), el valor con tagAddress '{tagAddress}' en la configuración", ex);
            }
        }
    }
}
