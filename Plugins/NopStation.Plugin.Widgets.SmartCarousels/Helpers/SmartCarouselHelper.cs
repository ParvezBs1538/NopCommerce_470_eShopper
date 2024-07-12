﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Widgets.SmartCarousels.Helpers;

public class SmartCarouselHelper
{
    public static string[] GetCustomWidgetZones()
    {
        try
        {
            var nopFileProvider = NopInstance.Load<INopFileProvider>();
            var filePath = nopFileProvider.Combine(nopFileProvider.MapPath("/Plugins/NopStation.Plugin.Widgets.SmartCarousels/"), "widgetZones.json");

            if (nopFileProvider.FileExists(filePath))
            {
                var jsonstr = nopFileProvider.ReadAllText(filePath, Encoding.UTF8);
                return JsonConvert.DeserializeObject<string[]>(jsonstr);
            }
        }
        catch (Exception ex)
        {
            NopInstance.Load<ILogger>().ErrorAsync(ex.Message, ex).Wait();
        }

        return new string[] { };
    }

    public static IList<SelectListItem> GetWidgetZoneSelectList()
    {
        var list = new List<SelectListItem>();
        var zones = GetCustomWidgetZones();

        if (zones != null && zones.Any())
        {
            foreach (var item in zones)
            {
                list.Add(new SelectListItem()
                {
                    Value = item,
                    Text = item
                });
            }
        }
        return list;
    }
}