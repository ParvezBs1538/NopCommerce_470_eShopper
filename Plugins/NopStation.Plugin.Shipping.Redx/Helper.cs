using System.Collections.Generic;
using RestSharp;
using System;
using Newtonsoft.Json;
using NopStation.Plugin.Shipping.Redx.Models;

namespace NopStation.Plugin.Shipping.Redx
{
    public static class Helper
    {
        #region Utilities

        private static RestRequest PrepareRESTRequest(IDictionary<string, string> headers, Method method)
        {
            var request = new RestRequest(method)
            {
                RequestFormat = DataFormat.Json
            };

            foreach (var header in headers)
                request.AddHeader(header.Key, header.Value);

            return request;
        }

        #endregion

        #region Methods

        public static BaseHttpResponse<T> Get<T>(this Uri uri, IDictionary<string, string> headers = null)
        {
            var request = PrepareRESTRequest(headers, Method.POST);

            var client = new RestClient(uri);
            var response = client.Execute(request);

            var responseModel = new BaseHttpResponse<T>
            {
                Success = response.IsSuccessful,
                Model = JsonConvert.DeserializeObject<T>(response.Content),
                Response = response
            };

            return responseModel;
        }

        public static BaseHttpResponse<T> Post<T>(this Uri uri, IDictionary<string, string> headers = null, object model = null)
        {
            var request = PrepareRESTRequest(headers, Method.POST);

            var client = new RestClient(uri);
            var response = client.Execute(request);
            if (model != null)
                request.AddParameter("application/json", JsonConvert.SerializeObject(model), ParameterType.RequestBody);

            var responseModel = new BaseHttpResponse<T>
            {
                Success = response.IsSuccessful,
                Model = JsonConvert.DeserializeObject<T>(response.Content),
                Response = response
            };

            return responseModel;
        }

        #endregion
    }
}