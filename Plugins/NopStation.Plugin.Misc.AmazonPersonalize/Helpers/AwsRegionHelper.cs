using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.Core.Helpers;
using Amazon;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Helpers
{
    public class AwsRegionHelper
    {
        public static List<string> GetAwsRegions()
        {
            var zones = new List<string>();
            try
            {
                var list = GetAwsRegionsNameValues();
                if (list != null && list.Any())
                    zones.AddRange(list.Select(x => x.Name).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct());
            }
            catch (Exception ex)
            {
                NopInstance.Load<ILogger>().ErrorAsync(ex.Message, ex).Wait();
            }
            return zones;
        }

        public static List<AwsRegionModel> GetAwsRegionsNameValues()
        {
            var zones = new List<AwsRegionModel>();
            try
            {
                var nopFileProvider = NopInstance.Load<INopFileProvider>();
                var filePath = nopFileProvider.Combine(nopFileProvider.MapPath("/Plugins/NopStation.Plugin.Misc.AmazonPersonalize/"), "awsRegions.json");

                if (nopFileProvider.FileExists(filePath))
                {
                    var jsonstr = nopFileProvider.ReadAllText(filePath, Encoding.UTF8);
                    var list = JsonConvert.DeserializeObject<List<AwsRegionModel>>(jsonstr);
                    if (list != null && list.Any())
                        return list;
                }
            }
            catch (Exception ex)
            {
                NopInstance.Load<ILogger>().ErrorAsync(ex.Message, ex).Wait();
            }
            return zones;
        }

        public static bool TryGetAwsRegionId(string awsRegion, out int regionId)
        {
            regionId = -1;
            var zones = GetAwsRegionsNameValues();
            if (zones != null && zones.Any(x => x.Name.Equals(awsRegion)))
            {
                regionId = zones.FirstOrDefault(x => x.Name.Equals(awsRegion)).Value;
                return true;
            }
            return false;
        }

        public static string GetAwsRegion(int regionId)
        {
            var zones = GetAwsRegionsNameValues();
            if (zones != null && zones.Any(x => x.Value == regionId))
            {
                return zones.FirstOrDefault(x => x.Value == regionId).Name;
            }
            return null;
        }

        public static RegionEndpoint GetRegionEndPoint(string regionEndPointName)
        {
            switch (regionEndPointName)
            {
                case "us-east-1":
                    return RegionEndpoint.USEast1;
                case "us-east-2":
                    return RegionEndpoint.USEast2;
                case "us-west-1":
                    return RegionEndpoint.USWest1;
                case "us-west-2":
                    return RegionEndpoint.USWest2;
                case "af-south-1":
                    return RegionEndpoint.AFSouth1;
                case "ap-east-1":
                    return RegionEndpoint.APEast1;
                case "ap-southeast-3":
                    return RegionEndpoint.APSoutheast3;
                case "ap-south-1":
                    return RegionEndpoint.APSouth1;
                case "ap-northeast-3":
                    return RegionEndpoint.APNortheast3;
                case "ap-northeast-2":
                    return RegionEndpoint.APNortheast2;
                case "ap-southeast-1":
                    return RegionEndpoint.APSoutheast1;
                case "ap-southeast-2":
                    return RegionEndpoint.APSoutheast2;
                case "ap-northeast-1":
                    return RegionEndpoint.APNortheast1;
                case "ca-central-1":
                    return RegionEndpoint.CACentral1;
                case "eu-central-1":
                    return RegionEndpoint.EUCentral1;
                case "eu-west-1":
                    return RegionEndpoint.EUWest1;
                case "eu-west-2":
                    return RegionEndpoint.EUWest2;
                case "eu-south-1":
                    return RegionEndpoint.EUSouth1;
                case "eu-west-3":
                    return RegionEndpoint.EUWest3;
                case "eu-north-1":
                    return RegionEndpoint.EUNorth1;
                case "me-south-1":
                    return RegionEndpoint.MESouth1;
                case "sa-east-1":
                    return RegionEndpoint.SAEast1;
            }
            return RegionEndpoint.APSoutheast1;
        }

        public static IList<SelectListItem> GetAwsRegionsSelectList()
        {
            var list = new List<SelectListItem>();
            var zones = GetAwsRegionsNameValues();
            if (zones != null && zones.Any())
            {
                foreach (var item in zones)
                {
                    list.Add(new SelectListItem()
                    {
                        Value = item.Value.ToString(),
                        Text = item.Name
                    });
                }
            }
            return list;
        }
    }
}