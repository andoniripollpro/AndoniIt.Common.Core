using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks; // Necesario para Task

namespace AndoIt.Common.Interface
{
    public interface IHttpClientAdapter
    {
        AuthenticationHeaderValue AuthenticationHeaderValue { get; set; }
        HttpClientAdapter.ILog LogListener { get; set; }
        int? TimeoutSeconds { get; set; }

        Task<string> AllCookedUpDelete(string url, string urn, NetworkCredential credentials = null);
        Task<string> AllCookedUpGet(string url, NetworkCredential credentials = null);
        Task<T> AllCookedUpGet<T>(string url, NetworkCredential credentials = null);
        Task AllCookedUpMoveFile(string url, string completeFileAddress, NetworkCredential credentials = null);
        Task AllCookedUpPatch(string url, string body, NetworkCredential credentials = null);
        Task<string> AllCookedUpPost(string url, object body, NetworkCredential credentials = null);
        Task<string> AllCookedUpPost(string url, string body, NetworkCredential credentials = null);
        Task<T> AllCookedUpPost<T>(string url, object body, NetworkCredential credentials = null);
        Task<string> AllCookedUpPut(string url, object body, NetworkCredential credentials = null);
        Task<T> AllCookedUpPut<T>(string url, object body, NetworkCredential credentials = null);
        Task<string> AllCookedUpSoap(string url, string body, NetworkCredential credentials = null);
        Task AllCookedUpUploadFile(string url, string completeFileAddress, NetworkCredential credentials = null);
        HttpClient GetDisposableHttpClient(string url, NetworkCredential credentials = null, string mediaTypeHeaderValue = null);
        Task<HttpResponseMessage> StandardDelete(string url, string urn, NetworkCredential credentials = null);
        Task<HttpResponseMessage> StandardGet(string url, NetworkCredential credentials = null);
        Task<HttpResponseMessage> StandardPatch(string url, object body, NetworkCredential credentials = null);
        Task<HttpResponseMessage> StandardPost(string url, string body, NetworkCredential credentials = null);
        Task<HttpResponseMessage> StandardPut(string url, object body, NetworkCredential credentials = null);
    }
}
