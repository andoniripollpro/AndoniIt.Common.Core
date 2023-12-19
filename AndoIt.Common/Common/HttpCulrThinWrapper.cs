//using AndoIt.Common.Interface;
//using CurlThin;
//using CurlThin.Enums;
//using CurlThin.SafeHandles;
//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Runtime.InteropServices;
//using System.Text;

///// <summary>
///// TODO: No funciona. Lo dejo para retomarlo en algún momento. Aparentemente conecta pero no se obtiene el resultado del GET
///// </summary>
//namespace AndoIt.Common.Common
//{
//    public class HttpCulrThinWrapper
//    {
//        private ILog log;

//        public HttpCulrThinWrapper (ILog log)
//        {
//            this.log = log;
//        }

//        public void CookedUpGet(string url)
//        {
//            prepareNPerform(url, (url, easy) =>
//            {
//                CurlNative.Easy.SetOpt(easy, CURLoption.URL, url);

//                return "Ok";
//            });
//        }

//        public void CookedUpPost(string url, string postData)
//        {
//            prepareNPerform(url, (url, easy) =>
//            {
//                CurlNative.Easy.SetOpt(easy, CURLoption.URL, url);

//                // This one has to be called before setting COPYPOSTFIELDS.
//                CurlNative.Easy.SetOpt(easy, CURLoption.POSTFIELDSIZE, Encoding.ASCII.GetByteCount(postData));
//                CurlNative.Easy.SetOpt(easy, CURLoption.COPYPOSTFIELDS, postData);

//                return "Ok";
//            });
//        }

//        private void prepareNPerform(string url, Func<string, SafeEasyHandle, string> funcion)
//        {
//            // curl_global_init() with default flags.
//            var global = CurlNative.Init();

//            // curl_easy_init() to create easy handle.
//            var easy = CurlNative.Easy.Init();
//            try
//            {
//                funcion.Invoke(url, easy);

//                var stream = new MemoryStream();
//                CurlNative.Easy.SetOpt(easy, CURLoption.WRITEFUNCTION, (data, size, nmemb, user) =>
//                {
//                    var length = (int)size * (int)nmemb;
//                    var buffer = new byte[length];
//                    Marshal.Copy(data, buffer, 0, length);
//                    stream.Write(buffer, 0, length);
//                    return (UIntPtr)length;
//                });

//                var result = CurlNative.Easy.Perform(easy);

//                this.log?.Info($"Result code: {result}.", new StackTrace());
//                this.log?.Info($"Response body: {Encoding.UTF8.GetString(stream.ToArray())}", new StackTrace());
//            }
//            finally
//            {
//                easy.Dispose();

//                if (global == CURLcode.OK)
//                {
//                    CurlNative.Cleanup();
//                }
//            }
//        }
//    }
//}
