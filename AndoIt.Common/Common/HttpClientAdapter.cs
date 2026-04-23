using AndoIt.Common.Interface;
using Microsoft.AspNetCore.WebUtilities;
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

        public async Task<T> AllCookedUpPost<T>(string url, object body, NetworkCredential credentials = null)
        {
            string resultStr = await AllCookedUpPost(url, JsonConvert.SerializeObject(body), credentials);
            T resultT = JsonConvert.DeserializeObject<T>(resultStr);
            return resultT;
        }
        public async Task<string> AllCookedUpPost(string url, object body, NetworkCredential credentials = null)
        {
            return await AllCookedUpPost(url, JsonConvert.SerializeObject(body), credentials);
        }
        public async Task<string> AllCookedUpPost(string url, string body, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del POST {url}. Credentials: {credentials?.UserName} Headers: {this.AuthenticationHeaderValue?.ToString()} Body: {body}. ");

            var responseMessage = await this.StandardPost(url, body, credentials);
            if (!responseMessage.IsSuccessStatusCode)
                throw new Exception(ResponseToString(responseMessage));
            string response = await responseMessage.Content.ReadAsStringAsync();
            this.LogListener?.Message("AllCookedUpPost end");
            return response;

        }
        public async Task<string> AllCookedUpSoap(string url, string body, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del POST SOAP {url}. Credentials: {credentials?.UserName} Body: {body}. Headers: {this.AuthenticationHeaderValue?.ToString()}. ");

            using (var webApiClient = this.GetDisposableHttpClient(url, credentials, "application/xml"))
            {
                webApiClient.DefaultRequestHeaders.Add("SOAPAction", url);
                var content = new StringContent(body, Encoding.UTF8, "text/xml");

                var responseMessage = await webApiClient.PostAsync(url, content);
                LogCallNResponse("AllCookedUpSoap", url, body, credentials, responseMessage);
                if (!responseMessage.IsSuccessStatusCode)
                    throw new Exception(ResponseToString(responseMessage));
                return await ExtractFromTaskWithEncoding(responseMessage.Content);
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

        public async Task<string> AllCookedUpDelete(string url, string urn, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del DELETE {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. Deleted file: {urn}");
            string response = "ERROR";

            var responseMessage = await StandardDelete(url, urn, credentials);

            response = await responseMessage.Content.ReadAsStringAsync();

            return response;
        }

        public async Task<HttpResponseMessage> StandardDelete(string url, string urn, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del DELETE {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. Deleted file: {urn}");

            using (var webApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                var responseMessage = await webApiClient.DeleteAsync(urn);
                this.logListener?.Message("AllCookedUpDelete end");
                return responseMessage;
            }
        }

        public async Task AllCookedUpPatch(string url, string body, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del PATCH {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. Body: {body}");

            using (var webApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = await webApiClient.PatchAsync(url, content);
                this.LogListener?.Message($"Response.StatusCode = {(int)response.StatusCode}");
            }
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

        public async Task<string> AllCookedUpPut(string url, object body, NetworkCredential credentials = null)
        {
            string response = "ERROR";

            var responseMessage = await this.StandardPut(url, body, credentials);
            
            if (!responseMessage.IsSuccessStatusCode)
                throw new Exception(ResponseToString(responseMessage));
            response = $"StatusCode: {responseMessage.StatusCode}, Status: {responseMessage.ReasonPhrase}, Body: {await responseMessage.Content.ReadAsStringAsync()}";
            this.logListener?.Message("AllCookedUpPut end");
            return response;
        }

        public async Task<HttpResponseMessage> StandardPut(string url, object body, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del Standard PUT {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue.ToString()}. Body: {body}");
            HttpResponseMessage responseMessage;

            using (var wepApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                StringContent content = new StringContent(body is string ? body.ToString() : JsonConvert.SerializeObject(body));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                responseMessage = await wepApiClient.PutAsync(string.Empty, content);
            }
            return responseMessage;
        }
        public async Task<T> AllCookedUpPut<T>(string url, object body, NetworkCredential credentials = null)
        {
            string strResult = await AllCookedUpPut(url, body, credentials);
            return JsonConvert.DeserializeObject<T>(strResult);
        }
        public async Task<HttpResponseMessage> StandardPatch(string url, object body, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($" Patching {body} to {url}");

            using (var webApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                this.LogListener?.Message($"Antes del PATCH {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. Body: {body}");
                StringContent content = new StringContent(body is string ? body.ToString() : JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var response = await webApiClient.PatchAsync(string.Empty, content);
                this.LogListener?.Message($"Response.StatusCode = {(int)response.StatusCode}");
                return response;
            }
        }

        public async Task AllCookedUpUploadFile(string url, string completeFileAddress, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Uploading {completeFileAddress} to {url}");

            using (var webApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                using (var fileStream = new FileStream(completeFileAddress, FileMode.Open, FileAccess.Read))
                {
                    var content = new StreamContent(fileStream);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    this.LogListener?.Message($"Antes del PUT(UploadFile) {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. File: {completeFileAddress}");
                    var response = await webApiClient.PutAsync(string.Empty, content);
                    this.LogListener?.Message($"Response.StatusCode = {(int)response.StatusCode}");
                }
            }
        }

        public async Task AllCookedUpMoveFile(string url, string completeFileAddress, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Uploading {completeFileAddress} to {url}");

            using (var webApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                using (var fileStream = new FileStream(completeFileAddress, FileMode.Open, FileAccess.Read))
                {
                    var content = new StreamContent(fileStream);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    this.LogListener?.Message($"Antes del PUT(MoveFile) {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. File: {completeFileAddress}");
                    var response = await webApiClient.PutAsync(string.Empty, content);
                    this.LogListener?.Message($"Response.StatusCode = {(int)response.StatusCode}");
                }
            }
        }

        private async Task FromFileToStream(string completeFileAddress, Stream rs)
        {
            using (FileStream fileStream = new FileStream(completeFileAddress, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                // Usamos ReadAsync y WriteAsync para no bloquear el hilo
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await rs.WriteAsync(buffer, 0, bytesRead);
                }
                fileStream.Close();
            }
            rs.Close();
        }
        
        public async Task<HttpResponseMessage> StandardGet(string url, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del Standard GET {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. ");
            HttpResponseMessage responseMessage;

            using (var wepApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                responseMessage = await wepApiClient.GetAsync(string.Empty);
                LogCallNResponse("StandardGet", url, null, credentials, responseMessage);
            }
            return responseMessage;
        }

        public async Task<HttpResponseMessage> StandardPost(string url, string body, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del Standard POST {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. Body: {body}");

            using (var webApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                StringContent content = new StringContent(body);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var responseMessage = await webApiClient.PostAsync(string.Empty, content);
                LogCallNResponse("StandardPost", url, body, credentials, responseMessage);
                return responseMessage;
            }
        }

        public async Task<string> AllCookedUpGet(string url, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del GET {url}. Credentials: {credentials?.UserName}.Headers: {this.AuthenticationHeaderValue?.ToString()}. ");

            using (var webApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                string response = await webApiClient.GetStringAsync(string.Empty);
                this.logListener?.Message("AllCookedUpGet end");
                return response;
            }
        }

        private static string ResponseToString(HttpResponseMessage responseMessage)
        {
            return $"ReasonPhrase = '{responseMessage.ReasonPhrase.ToString()}'. StatusCode={(int)responseMessage.StatusCode}. Headers = '{responseMessage.Headers}'.{Environment.NewLine}"
                + $"Content = '{JsonConvert.SerializeObject(responseMessage.Content)}'.";
        }

        public async Task<T> AllCookedUpGet<T>(string url, NetworkCredential credentials = null)
        {
            this.LogListener?.Message($"Antes del GET {url}. Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. ");

            using (var wepApiClient = this.GetDisposableHttpClient(url, credentials))
            {
                var responseMessage = await wepApiClient.GetAsync(string.Empty);
                LogCallNResponse("AllCookedUpGet<T>", url, null, credentials, responseMessage);
                if (!responseMessage.IsSuccessStatusCode)
                    throw new Exception(ResponseToString(responseMessage));
                var response = JsonConvert.DeserializeObject<T>(await responseMessage.Content.ReadAsStringAsync());
                return response;
            }
        }

        private void LogCallNResponse(string opetarionDesc, string url, object body, NetworkCredential credentials, HttpResponseMessage responseMessage)
        {
            this.LogListener?.Message("LogCallNResponse");
            string bodyMessage = (body != null) ? JsonConvert.SerializeObject(body) : "null";
            this.LogListener?.Message(opetarionDesc + ": " + ResponseToString(responseMessage)
                    + $" {Environment.NewLine}Url: {url}."
                    + $" {Environment.NewLine}Body: {bodyMessage}"
                    + $" {Environment.NewLine}Credentials: {credentials?.UserName}. Headers: {this.AuthenticationHeaderValue?.ToString()}. ");
            string statusCodeStr = responseMessage.StatusCode.ToString();
            this.StandardLog?.InfoObject(
                new
                {
                    Desciption = "HttpClientAdapter: operación finalizada",
                    Status = statusCodeStr,
                    StatusCode = (int)responseMessage.StatusCode,
                    ReasonPhrases = responseMessage.ReasonPhrase,
                    OperationDesc = opetarionDesc,
                    Url = url,
                    Body = body,
                    Credentials = credentials,
                    ResponseMessage = responseMessage
                });
            this.LogListener?.Message("LogCallNResponse");
        }

        public interface ILog
        {
            void Message(string message);
        }
    }
}
