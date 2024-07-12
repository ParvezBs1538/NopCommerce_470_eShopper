using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Http.Extensions;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Widgets.StoreLocator.Domain;
using NopStation.Plugin.Widgets.StoreLocator.Models;

namespace NopStation.Plugin.Widgets.StoreLocator.Helpers
{
    public static class StoreLocatorHelper
    {
        private const string GEO_LOCATION_SESSION_KEY = "CUSTOMER_GEO_LOCATION";

        private static double GetDistance(GeoLocationModel oldModel, GeoLocationModel model)
        {
            return GetDistance(oldModel.Latitude, oldModel.Longitude, model.Latitude, model.Longitude);
        }

        private static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            var sCoord = new GeoCoordinate(lat1, lng1);
            var eCoord = new GeoCoordinate(lat2, lng2);

            return sCoord.GetDistanceTo(eCoord);
        }

        private static double GetDouble(string str)
        {
            return double.TryParse(str, out var x) ? x : 0;
        }

        public static async Task SetGeoLocationAsync(GeoLocationModel model)
        {
            var httpContextAccessor = NopInstance.Load<IHttpContextAccessor>();
            var oldModel = await httpContextAccessor.HttpContext.Session.GetAsync<GeoLocationModel>(GEO_LOCATION_SESSION_KEY);

            if (oldModel == null || GetDistance(oldModel, model) > 100)
                await httpContextAccessor.HttpContext.Session.SetAsync(GEO_LOCATION_SESSION_KEY, model);
        }

        public static async Task<GeoLocationModel> GetGeoLocationAsync()
        {
            var httpContextAccessor = NopInstance.Load<IHttpContextAccessor>();
            return await httpContextAccessor.HttpContext.Session.GetAsync<GeoLocationModel>(GEO_LOCATION_SESSION_KEY);
        }

        public static async Task<IList<StoreLocation>> OrderStoresAsync(this IPagedList<StoreLocation> stores, StoreLocatorSettings storeLocatorSettings)
        {
            if (!stores.Any())
                return stores;

            if (!storeLocatorSettings.SortPickupPointsByDistance)
                return stores;

            var userLocation = await GetGeoLocationAsync();
            if(userLocation == null || (userLocation.Longitude == 0 && userLocation.Latitude == 0))
                return stores;

            if (storeLocatorSettings.DistanceCalculationMethodId == (int)DistanceCalculationMethod.GeoCoordinate)
                return stores.OrderBy(x => GetDistance(GetDouble(x.Latitude), GetDouble(x.Longitude), userLocation.Latitude, userLocation.Longitude)).ToList();

            var sortedStores = new List<KeyValuePair<double, StoreLocation>>();

            var i = 0;
            while (true)
            {
                var pagedStores = stores.Skip(i * 25).Take(25).ToList();
                if (!pagedStores.Any())
                    break;

                var destinationStr = WebUtility.UrlEncode(string.Join("|", pagedStores.Select(x => x.Latitude + "," + x.Longitude)));

                var url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={userLocation.Latitude},{userLocation.Longitude}&destinations={destinationStr}&units=metric&language=en-US&key={storeLocatorSettings.GoogleDistanceMatrixApiKey}";
                var request = WebRequest.Create(url);

                using var response = request.GetResponse();
                using var dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);
                var rt = reader.ReadToEnd();
                reader.Close();
                response.Close();

                var responseModel = JsonConvert.DeserializeObject<DistanceMatrixResponseModel>(rt);
                if (responseModel.Status == "OK")
                {
                    var row = responseModel.Rows.FirstOrDefault();
                    var a = 0;
                    foreach (var element in row.Elements)
                    {
                        sortedStores.Add(new KeyValuePair<double, StoreLocation>(element.Status == "OK" ? element.Distance.Value : int.MaxValue, pagedStores[a++]));
                    }
                }
            }

            return sortedStores.OrderBy(x => x.Key).Select(x => x.Value).ToList();
        }
    }
}
