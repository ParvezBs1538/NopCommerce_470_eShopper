﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Widgets.ProductTabs.Helpers
{
    public class ProductTabHelper
    {
        public static List<string> GetCustomWidgetZones()
        {
            var zones = new List<string>();
            try
            {
                var list = GetCustomWidgetZoneNameValues();
                if (list != null && list.Any())
                    zones.AddRange(list.Select(x => x.Name).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct());
            }
            catch (Exception ex)
            {
                NopInstance.Load<ILogger>().ErrorAsync(ex.Message, ex).Wait();
            }
            return zones;
        }

        public static List<ProductTabWidgetZoneModel> GetCustomWidgetZoneNameValues()
        {
            var zones = new List<ProductTabWidgetZoneModel>();
            try
            {
                var nopFileProvider = NopInstance.Load<INopFileProvider>();
                var filePath = nopFileProvider.Combine(nopFileProvider.MapPath("/Plugins/NopStation.Plugin.Widgets.ProductTabs/"), "widgetZones.json");

                if (nopFileProvider.FileExists(filePath))
                {
                    var jsonstr = nopFileProvider.ReadAllText(filePath, System.Text.Encoding.UTF8);
                    var list = JsonConvert.DeserializeObject<List<ProductTabWidgetZoneModel>>(jsonstr);
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

        public static bool TryGetWidgetZoneId(string widgetZone, out int widgetZoneId)
        {
            widgetZoneId = -1;
            var zones = GetCustomWidgetZoneNameValues();
            if (zones != null && zones.Any(x => x.Name.Equals(widgetZone)))
            {
                widgetZoneId = zones.FirstOrDefault(x => x.Name.Equals(widgetZone)).Value;
                return true;
            }
            return false;
        }

        public static string GetCustomWidgetZone(int widgetZoneId)
        {
            var zones = GetCustomWidgetZoneNameValues();
            if (zones != null && zones.Any(x => x.Value == widgetZoneId))
            {
                return zones.FirstOrDefault(x => x.Value == widgetZoneId).Name;
            }
            return null;
        }

        public static IList<SelectListItem> GetCustomWidgetZoneSelectList()
        {
            var list = new List<SelectListItem>();
            var zones = GetCustomWidgetZoneNameValues();
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
