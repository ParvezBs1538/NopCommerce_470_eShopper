using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.PinterestAnalytics.Api;
using NopStation.Plugin.Widgets.PinterestAnalytics.Domain;

namespace NopStation.Plugin.Widgets.PinterestAnalytics.Services
{
    public interface IPinterestAnalyticsService
    {
        public Task SendAsync(EventData eventData);
        public Task SaveCustomEventsAsync(string eventName, IList<string> widgetZones);
        public Task<List<string>> GetWidgetZonesAsync();
        public Task<string> PrepareCustomEventsScriptAsync(string widgetZone);
        public Task<string> PrepareScriptAsync(string widgetZone);
        public Task<string> PrepareEventScriptsAsync(string widgetZone);
        public Task<IList<CustomEvent>> GetCustomEventsAsync(string widgetZone);
    }
}
