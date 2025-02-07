using AndoIt.Common.Interface;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AndoIt.Common
{
    public class HttpClientAdapter : IHttpClientAdapter
    {
        public delegate void Log(string message);

        private HttpClientAdapter.ILog logListener = null;
        private AndoIt.Common.Interface.ILog log = null;
        private int? timeoutSeconds = null;
        private AuthenticationHeaderValue authenticationHeaderValue = null;

        public HttpClientAdapter()
        {
        }

        public HttpClientAdapter(HttpClientAdapter.ILog logListener = null, int? timeoutSeconds = null, AuthenticationHeaderValue authenticationHeaderValue = null)
        {
            this.logListener = logListener;
            this.timeoutSeconds = timeoutSeconds;
            this.authenticationHeaderValue = authenticationHeaderValue;
        }

        public ILog LogListener
        {
            get => this.logListener;
            set { this.logListener = value; }
        }
        public Interface.ILog StandardLog
        {
            get => this.log;
            set { this.log = value; }
        }
        public int? TimeoutSeconds
        {
            get => this.timeoutSeconds;
            set { this.timeoutSeconds = value; }
        }
        public AuthenticationHeaderValue AuthenticationHeaderValue
        {
            get => this.authenticationHeaderValue;
            set { this.authenticationHeaderValue = value; }
        }

        public T AllCookedUpPost<T>(string url, object body, NetworkCredential credentials = null)
        {
            string resultStr = AllCookedUpPost(url, JsonConvert.SerializeObject(body), credentials);
            T resultT = JsonConvert.DeserializeObject<T>(resultStr);
            return resultT;
        }
        public string AllCookedUpPost(string url, object body, NetworkCredential credentials = null)
        {
            return AllCookedUpPost(url, JsonConvert.SerializeObject(body), credentials);
        }
        public string AllCookedUpPost(string url, string body, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del POST {url}. Credentials: {credentials?.UserName} Headers: {this.AuthenticationHeaderValue?.ToString()} Body: {body}. ");

            var responseMessage = this.StandardPost(url, body, credentials);
            LogCallNResponse("AllCookedUpPost", url, body, credentials, responseMessage);
            if (!responseMessage.IsSuccessStatusCode)
                throw new Exception(ResponseToString(responseMessage));
            string response = responseMessage.Content.ReadAsStringAsync().Result;
            return response;

        }
        public string AllCookedUpSoap(string url, string body, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del POST SOAP {url}. Credentials: {credentials?.UserName} Body: {body}. Headers: {this.AuthenticationHeaderValue?.ToString()}. ");

            using (var webApiClient = this.GetDisposableHttpClient(url, credentials, "application/xml"))
            {
                webApiClient.DefaultRequestHeaders.Add("SOAPAction", url);
                var content = new StringContent(body, Encoding.UTF8, "text/xml");

                var responseMessage = webApiClient.PostAsync(url, content).Result;
                LogCallNResponse("AllCookedUpSoap", url, body, credentials, responseMessage);
                if (!responseMessage.IsSuccessStatusCode)
                    throw new Exception(ResponseToString(responseMessage));
                return ExtractFromTaskWithEncoding(responseMessage.Content).Result;
            }
        }

        private async Task<string> ExtractFromTaskWithEncoding(HttpContent responseContent)
        {
            var resultUncodified = await responseContent.ReadAsByteArrayAsync();
            string result = Encoding.UTF8.GetString(resultUncodified, 0, resultUncodified.Length);
            //	Limpiando si va encapsulado en un XML
            if (result.StartsWith("<string"))
                result = result.SubStringTruncated("<string", "</string>").SubStringTruncated(">").Replace("&lt;", "<").Replace("&gt;", ">");
            return result;
        }

        public virtual HttpClient GetDisposableHttpClient(string url, NetworkCredential credentials = null, string mediaTypeHeaderValue = null)
        {
            HttpClientHandler handler;
            if (credentials == null)
                handler = new HttpClientHandler();
            else
            {
                handler = new HttpClientHandler() { Credentials = credentials };
                handler.PreAuthenticate = true;
                handler.Proxy = WebRequest.DefaultWebProxy;
                handler.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            }

            HttpClient client = new HttpClient(handler)
            {
                BaseAddress = new Uri(url)
            };

            client.DefaultRequestHeaders.UserAgent.TryParseAdd(Assembly.GetExecutingAssembly().GetName().Name);
            if (mediaTypeHeaderValue == null)
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            else
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaTypeHeaderValue));

            if (this.AuthenticationHeaderValue != null)
                client.DefaultRequestHeaders.Authorization = this.AuthenticationHeaderValue;

            if (credentials != null)
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                string usuPassString = $"{credentials?.UserName}:{credentials?.Password}";
                byte[] bytes = Encoding.UTF8.GetBytes(usuPassString);
                string oauthToken = Convert.ToBase64String(bytes);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", oauthToken);
            }

            if (this.TimeoutSeconds.HasValue)
            {
                client.Timeout = new TimeSpan(0, 0, this.TimeoutSeconds.Value);
                this.LogListener?.Message($"Timeout establecido a {this.TimeoutSeconds.Value} segundos");
            }

            return client;
        }

        public string AllCookedUpDelete(string url, string urn, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del DELETE {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. Deleted file: {urn}");
            string response = "ERROR";

            var responseMessage = StandardDelete(url, urn, credentials);

            response = responseMessage.Content.ReadAsStringAsync().Result;

            return response;
        }

        public HttpResponseMessage StandardDelete(string url, string urn, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del DELETE {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. Deleted file: {urn}");

            using (var webApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                var responseMessage = webApiClient.DeleteAsync(urn).Result;
                LogCallNResponse("StandardDelete", url, null, credentials, responseMessage);
                return responseMessage;
            }
        }

        public void AllCookedUpPatch(string url, string body, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del PATCH {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. Body: {body}");

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PATCH";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Credentials = credentials ?? CredentialCache.DefaultCredentials;

            var bytes = Encoding.UTF8.GetBytes(body);
            httpWebRequest.GetRequestStream().Write(bytes, 0, bytes.Length);

            using (WebResponse wresp = httpWebRequest.GetResponse())
            {
                this.LogListener?.Message($"Response.StatusCode = {(int)((HttpWebResponse)wresp).StatusCode}");
            };

            httpWebRequest = null;
        }

        private void SetCredentials(HttpClient wepApiClient, NetworkCredential credentials)
        {
            if (credentials != null)
            {
                wepApiClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                        ASCIIEncoding.ASCII.GetBytes(
                        $"{credentials?.UserName}:{credentials?.Password}")));
            }
        }

        public string AllCookedUpPut(string url, object body, NetworkCredential credentials = null)
        {
            string response = "ERROR";

            var responseMessage = this.StandardPut(url, body, credentials);
            LogCallNResponse("AllCookedUpPut", url, body, credentials, responseMessage);
            if (!responseMessage.IsSuccessStatusCode)
                throw new Exception(ResponseToString(responseMessage));
            response = $"StatusCode: {responseMessage.StatusCode}, Status: {responseMessage.ReasonPhrase}, Body: {responseMessage.Content.ReadAsStringAsync().Result}";

            return response;
        }

        private void LogCallNResponse(string opetarionDesc, string url, object body, NetworkCredential credentials, HttpResponseMessage responseMessage)
        {
            string bodyMessage = (body != null)? $"{ Environment.NewLine}Body: {body}. " : string.Empty;
            this.LogListener?.Message(opetarionDesc + ": " + ResponseToString(responseMessage)
                    + $" {Environment.NewLine}Url: {url}. "
                    + $" {Environment.NewLine}Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. ");
            this.StandardLog?.InfoObject(
                new { 
                    OperationDesc = opetarionDesc,
                    Url = url,
                    Body = body,
                    Credentials = credentials,
                    ResponseMessage = responseMessage
                });
        }

        public HttpResponseMessage StandardPut(string url, object body, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del Standard PUT {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue.ToString()}. Body: {body}");
            HttpResponseMessage responseMessage;

            using (var wepApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                StringContent content = new StringContent(body is string ? body.ToString() : JsonConvert.SerializeObject(body));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                responseMessage = wepApiClient.PutAsync(string.Empty, content).Result;
            }
            return responseMessage;
        }
        public T AllCookedUpPut<T>(string url, object body, NetworkCredential credentials = null)
        {
            string strResult = AllCookedUpPut(url, body, credentials);
            return JsonConvert.DeserializeObject<T>(strResult);
        }
        public WebResponse StandardPatch(string url, object body, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($" Patching {body} to {url}");

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PATCH";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Credentials = credentials ?? CredentialCache.DefaultCredentials;

            this.LogListener?.Message($"Antes del PATCH {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. Body: {body}");
            using (WebResponse wresp = httpWebRequest.GetResponse())
            {
                this.LogListener?.Message($"Response.StatusCode = {(int)((HttpWebResponse)wresp).StatusCode}");
                httpWebRequest = null;
                return wresp;
            };
        }

        public void AllCookedUpUploadFile(string url, string completeFileAddress, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Uploading {completeFileAddress} to {url}");

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PUT";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Credentials = credentials ?? CredentialCache.DefaultCredentials;

            FromFileToStream(completeFileAddress, httpWebRequest.GetRequestStream());

            this.LogListener?.Message($"Antes del PUT(UploadFile) {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. File: {completeFileAddress}");
            using (WebResponse wresp = httpWebRequest.GetResponse())
            {
                this.LogListener?.Message($"Response.StatusCode = {(int)((HttpWebResponse)wresp).StatusCode}");
            };

            httpWebRequest = null;
        }

        public void AllCookedUpMoveFile(string url, string completeFileAddress, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Uploading {completeFileAddress} to {url}");

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PUT";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Credentials = credentials ?? CredentialCache.DefaultCredentials;

            FromFileToStream(completeFileAddress, httpWebRequest.GetRequestStream());

            this.LogListener?.Message($"Antes del PUT(MoveFile) {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. File: {completeFileAddress}");
            using (WebResponse wresp = httpWebRequest.GetResponse())
            {
                this.LogListener?.Message($"Response.StatusCode = {(int)((HttpWebResponse)wresp).StatusCode}");
            };

            httpWebRequest = null;
        }

        private void FromFileToStream(string completeFileAddress, Stream rs)
        {
            FileStream fileStream = new FileStream(completeFileAddress, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();
            rs.Close();
        }

        public HttpResponseMessage StandardGet(string url, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del Standard GET {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. ");
            HttpResponseMessage responseMessage;

            using (var wepApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                responseMessage = wepApiClient.GetAsync(string.Empty).Result;
                LogCallNResponse("StandardGet", url, null, credentials, responseMessage);
            }
            return responseMessage;
        }

        public HttpResponseMessage StandardPost(string url, string body, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del Standard POST {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. Body: {body}");

            using (var webApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                StringContent content = new StringContent(body);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var responseMessage = webApiClient.PostAsync(string.Empty, content).Result;
                string responseBody = responseMessage.Content.ReadAsStringAsync().Result;
                LogCallNResponse("StandardPost", url, body, credentials, responseMessage);
                return responseMessage;
            }
        }

        public string AllCookedUpGet(string url, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del GET {url}. Credentials: {credentials?.UserName}.Headers: {this.AuthenticationHeaderValue?.ToString()}. ");
            string response = "ERROR";

            var responseMessage = this.StandardGet(url, credentials);
            if (!responseMessage.IsSuccessStatusCode)
                throw new Exception(ResponseToString(responseMessage));
            response = responseMessage.Content.ReadAsStringAsync().Result;

            return response;
        }

        private static string ResponseToString(HttpResponseMessage responseMessage)
        {
            return $"ReasonPhrase = '{responseMessage.ReasonPhrase.ToString()}'. StatusCode={(int)responseMessage.StatusCode}. Headers = '{responseMessage.Headers}'.{Environment.NewLine}"
                + $"Content = '{JsonConvert.SerializeObject(responseMessage.Content)}'.";
        }

        public T AllCookedUpGet<T>(string url, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del GET {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. ");

            using (var wepApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                var responseMessage = wepApiClient.GetAsync(string.Empty).Result;
                LogCallNResponse("AllCookedUpGet<T>", url, null, credentials, responseMessage);
                if (!responseMessage.IsSuccessStatusCode)
                    throw new Exception(ResponseToString(responseMessage));
                var response = JsonConvert.DeserializeObject<T>(responseMessage.Content.ReadAsStringAsync().Result);
                return response;
            }
        }

        public interface ILog
        {
            void Message(string message);
        }
    }
}