using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Widgets.DynamicSurvey
{
    public class SurveyHelper
    {
        public static string[] GetWidgetZones()
        {
            try
            {
                var nopFileProvider = NopInstance.Load<INopFileProvider>();
                var filePath = nopFileProvider.Combine(nopFileProvider.MapPath("/Plugins/NopStation.Plugin.Widgets.DynamicSurvey/"), "widgetZones.json");

                if (nopFileProvider.FileExists(filePath))
                {
                    var jsonstr = nopFileProvider.ReadAllText(filePath, Encoding.UTF8);
                    var list = JsonConvert.DeserializeObject<string[]>(jsonstr);
                    if (list != null && list.Any())
                        return list;
                }
            }
            catch (Exception ex)
            {
                NopInstance.Load<ILogger>().ErrorAsync(ex.Message, ex);
            }

            return new string[] { };
        }
    }
}
