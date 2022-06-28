using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndoIt.Common.Common
{
    public static class JsonExtender
    {
        public static bool IsValidJson(this string strInput, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    errorMessage = $"JsonReaderException: {jex.Message}";
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    errorMessage = $"JsonReaderException: {ex.Message}";                    
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static bool IsValidJson(this string strInput)
        {
            string errorMessageToDespise = string.Empty;
            return strInput.IsValidJson(out errorMessageToDespise);
        }
    }
}
