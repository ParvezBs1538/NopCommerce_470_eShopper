using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nop.Core;
using Nop.Core.Http.Extensions;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using NopStation.Plugin.Widgets.PinterestAnalytics.Api;
using NopStation.Plugin.Widgets.PinterestAnalytics.Domain;
using NopStation.Plugin.Widgets.PinteresteAnalytics;

namespace NopStation.Plugin.Widgets.PinterestAnalytics.Services
{
    public class PinterestAnalyticsService : IPinterestAnalyticsService
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PinterestAnalyticsService(ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            ILogger logger,
            IWidgetPluginManager widgetPluginManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _settingService = settingService;
            _storeContext = storeContext;
            _logger = logger;
            _workContext = workContext;
            _widgetPluginManager = widgetPluginManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SendAsync(EventData eventData)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var pinterestAnalyticsSettings = await _settingService.LoadSettingAsync<PinterestAnalyticsSettings>(store.Id);
            var url = pinterestAnalyticsSettings.ApiUrl;
            var accessToken = pinterestAnalyticsSettings.AccessToken;
            var adAccountId = pinterestAnalyticsSettings.AdAccountId;
            url = url.Replace("AdAccountId", adAccountId);

            var postData = new Events();
            postData.Data = new List<EventData> { eventData };
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new LowercasePropertyNamesContractResolver()
            };

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            var jsonData = JsonConvert.SerializeObject(postData, settings);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, content);

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var responseString = JsonConvert.SerializeObject(response);

                    if (pinterestAnalyticsSettings.SaveLog)
                        await _logger.InformationAsync(string.Format("URL: {0}, Body: {1}", url, JsonConvert.SerializeObject(postData)));
                }
                if (pinterestAnalyticsSettings.SaveLog)
                    await _logger.ErrorAsync(string.Format("Status code: {0}, URL: {1}, Body: {2}", response.StatusCode, url, JsonConvert.SerializeObject(postData)));
            }
            catch (Exception ex)
            {
                if (pinterestAnalyticsSettings.SaveLog)
                    await _logger.ErrorAsync(string.Format("Exception: {0}, URL: {1}, Body: {2}", ex.Message, url, JsonConvert.SerializeObject(postData)));
            }
        }

        public class LowercasePropertyNamesContractResolver : DefaultContractResolver
        {
            protected override string ResolvePropertyName(string propertyName)
            {
                return propertyName.ToLowerInvariant();
            }
        }

        public async Task SaveCustomEventsAsync(string eventName, IList<string> widgetZones)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var configuration = await _settingService.LoadSettingAsync<PinterestAnalyticsSettings>(storeScope);
            if (configuration == null)
                return;

            //load configuration custom events
            var customEventsValue = configuration.CustomEvents ?? string.Empty;
            var customEvents = JsonConvert.DeserializeObject<List<CustomEvent>>(customEventsValue) ?? new List<CustomEvent>();

            //try to get an event by the passed name
            var customEvent = customEvents
                .FirstOrDefault(customEvent => eventName.Equals(customEvent.EventName, StringComparison.InvariantCultureIgnoreCase));
            if (customEvent == null)
            {
                //create new one if not exist
                customEvent = new CustomEvent { EventName = eventName };
                customEvents.Add(customEvent);
            }

            //update widget zones of this event
            customEvent.WidgetZones = widgetZones ?? new List<string>();

            //or delete an event
            if (!customEvent.WidgetZones.Any())
                customEvents.Remove(customEvent);

            //update configuration 
            configuration.CustomEvents = JsonConvert.SerializeObject(customEvents);
            await _settingService.SaveSettingAsync(configuration, storeScope);
        }

        public async Task<List<CustomEvent>> GetEventsAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<PinterestAnalyticsSettings>(storeScope);
            var events = settings.CustomEvents ?? string.Empty;
            if (events == null)
                return new List<CustomEvent>();
            var customEvents = JsonConvert.DeserializeObject<List<CustomEvent>>(events) ?? new List<CustomEvent>();
            return customEvents;
        }

        public async Task<List<string>> GetWidgetZonesAsync()
        {
            var customEvents = await GetEventsAsync();
            var widgetZones = await customEvents.SelectMany(customEvent => customEvent.WidgetZones).Distinct().ToListAsync();

            return widgetZones;
        }

        private async Task<bool> PluginActiveAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            return await _widgetPluginManager
                .IsPluginActiveAsync(PinterestAnalyticsDefaults.SystemName, await _workContext.GetCurrentCustomerAsync(), store.Id);
        }

        private async Task<TResult> HandleFunctionAsync<TResult>(Func<Task<TResult>> function)
        {
            try
            {
                //check whether the plugin is active
                if (!await PluginActiveAsync())
                    return default;

                //invoke function
                return await function();
            }
            catch (Exception exception)
            {
                //get a short error message
                var detailedException = exception;
                do
                {
                    detailedException = detailedException.InnerException;
                } while (detailedException?.InnerException != null);

                //log errors
                var error = $"{PinterestAnalyticsDefaults.SystemName} error: {Environment.NewLine}{exception.Message}";
                await _logger.ErrorAsync(error, exception, await _workContext.GetCurrentCustomerAsync());

                return default;
            }
        }

        private async Task PrepareTrackedEventScriptAsync(string eventName, string eventObject,
           int? customerId = null, int? storeId = null, bool isCustomEvent = false)
        {
            //prepare script and store it into the session data, we use this later
            var customer = await _workContext.GetCurrentCustomerAsync();
            customerId ??= customer.Id;
            var store = await _storeContext.GetCurrentStoreAsync();
            storeId ??= store.Id;
            var events = await _httpContextAccessor.HttpContext.Session
                .GetAsync<IList<TrackedEvent>>(PinterestAnalyticsDefaults.TrackedEventsSessionValue) ?? new List<TrackedEvent>();
            var activeEvent = events.FirstOrDefault(trackedEvent =>
                trackedEvent.EventName == eventName && trackedEvent.CustomerId == customerId && trackedEvent.StoreId == storeId);
            if (activeEvent == null)
            {
                activeEvent = new TrackedEvent
                {
                    EventName = eventName,
                    CustomerId = customerId.Value,
                    StoreId = storeId.Value,
                    IsCustomEvent = isCustomEvent
                };
                events.Add(activeEvent);
            }
            activeEvent.EventObjects.Add(eventObject);
            await _httpContextAccessor.HttpContext.Session.SetAsync(PinterestAnalyticsDefaults.TrackedEventsSessionValue, events);
        }

        public async Task<string> PrepareCustomEventsScriptAsync(string widgetZone)
        {
            return await HandleFunctionAsync(async () =>
            {
                var customEvents = await GetEventsAsync();
                if (!string.IsNullOrEmpty(widgetZone))
                    customEvents = customEvents.Where(customEvent => customEvent.WidgetZones?.Contains(widgetZone) ?? false).ToList();
                foreach (var customEvent in customEvents)
                    await PrepareTrackedEventScriptAsync(customEvent.EventName, string.Empty, isCustomEvent: true);
                return string.Empty;
            });
        }

        public async Task<string> PrepareEventScriptsAsync(string widgetZone)
        {
            //get previously stored events and remove them from the session data
            var events = await _httpContextAccessor.HttpContext.Session
                .GetAsync<IList<TrackedEvent>>(PinterestAnalyticsDefaults.TrackedEventsSessionValue) ?? new List<TrackedEvent>();
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customEvents = await GetEventsAsync();
            if (!string.IsNullOrEmpty(widgetZone))
                customEvents = customEvents.Where(customEvent => customEvent.WidgetZones?.Contains(widgetZone) ?? false).ToList();
            var user_defined_script = "";
            foreach (var ev in customEvents)
            {
                var script = @"pintrk('track', '{EVENT_NAME}',{
                    customer_id: '{CUSTOMER_ID}',
                    store_id: '{STORE_ID}'
                });";
                script = script.Replace("{EVENT_NAME}", ev.EventName);
                script = script.Replace("{CUSTOMER_ID}", customer.CustomerGuid.ToString());
                script = script.Replace("{STORE_ID}", store.Id.ToString());
                user_defined_script += script;
            }
            return user_defined_script;
        }

        public async Task<string> PrepareScriptAsync(string widgetZone)
        {
            return await HandleFunctionAsync(async () =>
            {
                //get the enabled configurations
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var configuration = await _settingService.LoadSettingAsync<PinterestAnalyticsSettings>(storeScope);
                if (configuration != null)
                    return string.Empty;

                //base script
                var script = configuration.TrackingScript;
                var eventScript = await PrepareEventScriptsAsync(widgetZone);
                script = script.Replace("{EVENT_SCRIPT}", eventScript);
                return script;
            });
        }

        public async Task<IList<CustomEvent>> GetCustomEventsAsync(string widgetZone = null)
        {
            var customEvents = await GetEventsAsync();
            if (!string.IsNullOrEmpty(widgetZone))
                customEvents = customEvents.Where(customEvent => customEvent.WidgetZones?.Contains(widgetZone) ?? false).ToList();

            return customEvents;
        }
    }
}
