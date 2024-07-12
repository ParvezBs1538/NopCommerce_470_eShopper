using System.Collections.Generic;
using RestSharp;

namespace NopStation.Plugin.Shipping.Redx.Models
{
    public class BaseHttpResponse<T>
    {
        public bool Success { get; set; }

        public T Model { get; set; }

        public IRestResponse Response { get; set; }

        public IDictionary<object, object> CustomProperties { get; set; }
    }
}