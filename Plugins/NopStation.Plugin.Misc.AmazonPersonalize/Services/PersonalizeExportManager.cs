using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;
using NopStation.Plugin.Misc.AmazonPersonalize.Helpers;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Services
{
    public class PersonalizeExportManager : IPersonalizeExportManager
    {
        private readonly ILocalizationService _localizationService;

        public PersonalizeExportManager(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public async Task<string> ExportInteractionsTxtAsync(IList<EventReportLine> events)
        {
            const char separator = ',';
            var sb = new StringBuilder();

            sb.Append(await _localizationService.GetResourceAsync("NopStation.AmazonPersonalize.Interactions.Fields.UserId"));
            sb.Append(separator);
            sb.Append(await _localizationService.GetResourceAsync("NopStation.AmazonPersonalize.Interactions.Fields.ItemId"));
            sb.Append(separator);
            sb.Append(await _localizationService.GetResourceAsync("NopStation.AmazonPersonalize.Interactions.Fields.EventType"));
            sb.Append(separator);
            sb.Append(await _localizationService.GetResourceAsync("NopStation.AmazonPersonalize.Interactions.Fields.TimeStamp"));
            sb.Append(Environment.NewLine);

            foreach (var anevent in events)
            {
                sb.Append(anevent.UserId.ToString());
                sb.Append(separator);
                sb.Append(anevent.ItemId.ToString());
                sb.Append(separator);
                sb.Append(EventTypeEnum.Purchase.ToString());
                sb.Append(separator);
                sb.Append(RecommendationHelper.ConvertDatetimeToUnixTimeStamp(anevent.CreatedOnUtc));
                sb.Append(Environment.NewLine);

                //View Event
                sb.Append(anevent.UserId.ToString());
                sb.Append(separator);
                sb.Append(anevent.ItemId.ToString());
                sb.Append(separator);
                sb.Append(EventTypeEnum.View.ToString());
                sb.Append(separator);
                sb.Append(RecommendationHelper.ConvertDatetimeToUnixTimeStamp(anevent.CreatedOnUtc.AddMinutes(-1)));
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}