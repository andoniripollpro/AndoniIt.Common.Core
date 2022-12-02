using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AndoIt.Common.Interface
{
    public interface IHttpClientAdapter
    {
        AuthenticationHeaderValue AuthenticationHeaderValue { get; set; }
        HttpClientAdapter.ILog LogListener { get; set; }
        int? TimeoutSeconds { get; set; }

        string AllCookedUpDelete(string url, string urn, NetworkCredential credentials = null);
        string AllCookedUpGet(string url, NetworkCredential credentials = null);
        T AllCookedUpGet<T>(string url, NetworkCredential credentials = null);
        void AllCookedUpMoveFile(string url, string completeFileAddress, NetworkCredential credentials = null);
        void AllCookedUpPatch(string url, string body, NetworkCredential credentials = null);
        string AllCookedUpPost(string url, object body, NetworkCredential credentials = null);
        string AllCookedUpPost(string url, string body, NetworkCredential credentials = null);
        T AllCookedUpPost<T>(string url, object body, NetworkCredential credentials = null);
        string AllCookedUpPut(string url, object body, NetworkCredential credentials = null);
        T AllCookedUpPut<T>(string url, object body, NetworkCredential credentials = null);
        string AllCookedUpSoap(string url, string body, NetworkCredential credentials = null);
        void AllCookedUpUploadFile(string url, string completeFileAddress, NetworkCredential credentials = null);
        HttpClient GetDisposableHttpClient(string url, NetworkCredential credentials = null, string mediaTypeHeaderValue = null);
        HttpResponseMessage StandardDelete(string url, string urn, NetworkCredential credentials = null);
        HttpResponseMessage StandardGet(string url, NetworkCredential credentials = null);
        WebResponse StandardPatch(string url, object body, NetworkCredential credentials = null);
        HttpResponseMessage StandardPost(string url, string body, NetworkCredential credentials = null);
        HttpResponseMessage StandardPut(string url, object body, NetworkCredential credentials = null);
    }
}