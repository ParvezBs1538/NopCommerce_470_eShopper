using RestSharp;

namespace NopStation.Plugin.Payments.bKash.Models
{
    public class BaseHttpResponse<T>
    {
        public bool Success { get; set; }

        public T Model { get; set; }

        public IRestResponse Response { get; set; }
    }
}
