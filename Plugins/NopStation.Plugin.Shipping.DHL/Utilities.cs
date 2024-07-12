using Nop.Core;
using System.IO;
using System.Net;
using System.Text;

namespace NopStation.Plugin.Shipping.DHL
{
    public static class Utilities
    {
        public static string DoRequest(string url, string requestBody)
        {
            var bytes = Encoding.ASCII.GetBytes(requestBody);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = MimeTypes.ApplicationXWwwFormUrlencoded;
            request.ContentLength = bytes.Length;

            using (var requestStream = request.GetRequestStream())
                requestStream.Write(bytes, 0, bytes.Length);

            using var response = request.GetResponse();
            using var reader = new StreamReader(response.GetResponseStream());
            return reader.ReadToEnd();
        }
    }
}
