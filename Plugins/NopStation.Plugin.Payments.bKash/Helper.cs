using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NopStation.Plugin.Payments.bKash.Models;
using RestSharp;

namespace NopStation.Plugin.Payments.bKash
{
    public static class Helper
    {
        public static BaseHttpResponse<T> Post<T>(this Uri uri, IDictionary<string, string> headers = null, object model = null)
        {
            var request = new RestRequest(Method.POST)
            {
                RequestFormat = DataFormat.Json
            };

            foreach (var header in headers)
                request.AddHeader(header.Key, header.Value);

            var client = new RestClient(uri);
            if (model != null)
                request.AddParameter("application/json", JsonConvert.SerializeObject(model), ParameterType.RequestBody);

            var response = client.Execute(request);

            var responseModel = new BaseHttpResponse<T>
            {
                Success = response.IsSuccessful,
                Model = JsonConvert.DeserializeObject<T>(response.Content),
                Response = response
            };

            return responseModel;
        }
    }
}
