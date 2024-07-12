using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.PinterestAnalytics.Components;
using NopStation.Plugin.Widgets.PinterestAnalytics.Services;

namespace NopStation.Plugin.Widgets.PinterestAnalytics
{
    public class PinterestAnalyticsPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPinterestAnalyticsService _pinterestAnalyticsService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public PinterestAnalyticsPlugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            IPinterestAnalyticsService pinterestAnalyticsService,
            ISettingService settingService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _pinterestAnalyticsService = pinterestAnalyticsService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        #region Methods

        public async Task<IList<string>> GetWidgetZonesAsync()
        {
            var widgetZones = new List<string> { PublicWidgetZones.HeadHtmlTag };
            var widgetZonesList = await _pinterestAnalyticsService.GetWidgetZonesAsync();
            widgetZones.AddRange(widgetZonesList);
            return widgetZones;
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/PinterestAnalytics/Configure";
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Menu.PinterestAnalytics")
            };

            if (await _permissionService.AuthorizeAsync(PinterestAnalyticsPermissionProvider.ManagePinterestAnalytics))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/PinterestAnalytics/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "PinterestAnalytics.Configuration"
                };
                menu.ChildNodes.Add(configItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/pinterest-analytics-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=pinterest-analytics",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(WidgetsPinterestAnalyticsViewComponent);
        }

        public override async Task InstallAsync()
        {
            var settings = new PinterestAnalyticsSettings
            {
                PinterestId = "00000000",
                TrackingScript = @"<!-- Pinterest Tag -->
                    <script>
                    !function(e){if(!window.pintrk){window.pintrk = function () {
                    window.pintrk.queue.push(Array.prototype.slice.call(arguments))};var
                      n=window.pintrk;n.queue=[],n.version=""3.0"";var
                      t=document.createElement(""script"");t.async=!0,t.src=e;var
                      r=document.getElementsByTagName(""script"")[0];
                      r.parentNode.insertBefore(t,r)}}(""https://s.pinimg.com/ct/core.js"");
                    pintrk('load', 'PINTERESTID', {em: '<USER_EMAIL_ADDRESS>'});
                    pintrk('page');
                    {EVENT_SCRIPT}
                    </script>
                    <noscript>
                    <img height=""1"" width=""1"" style=""display:none;"" alt=""""
                      src=""https://ct.pinterest.com/v3/?event=init&tid=2613380611427&pd[em]=<HASHED_EMAIL_ADDRESS>&noscript=1"" />
                    </noscript>
                    
                    <!-- end Pinterest Tag -->",
                SaveLog = true,
                AdAccountId = "0000000000",
                AccessToken = "",
                ApiUrl = $"https://api.pinterest.com/v5/ad_accounts/AdAccountId/conversion_tags"
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new PinterestAnalyticsPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new PinterestAnalyticsPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Menu.PinterestAnalytics", "Pinterest Analytics"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Menu.Configuration", "Configuration"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.PinterestId", "ID"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.PinterestId.Hint", "Enter pinterest tag ID."),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.TrackingScript", "Tracking code"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.TrackingScript.Hint", "Paste the tracking code generated by Pinterest Analytics here. {PINTERESTID} and {USER_EMAIL_ADDRESS} will be dynamically replaced."),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Instructions", "<p>Pinterest Analytics is a free website stats tool from Pinterest. It keeps track of statistics about the visitors and eCommerce conversion on your website.<br /><br />Follow the next steps to enable Pinterest Analytics integration:<br /><ul><li><a href=\"http://www.google.com/analytics/\" target=\"_blank\">Create a Pinterest Analytics account</a> and follow the wizard to add your website</li><li>Copy the Tracking ID into the 'ID' box below</li><li>Click the 'Save' button below and Pinterest Analytics will be integrated into your store</li></ul><br />If you would like to switch between Pinterest Analytics (used by default) and Universal Analytics, then please use the buttons below:</p>"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.AdAccountId", "Ad account Id"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.AdAccountId.Hint", "Enter your pinterest ad account id"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Token", "Access token"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Token.Hint", "Enter your pinterest conversion access token"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.SaveLog", "Save log"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.SaveLog.Hint", "Check this to log"),

                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Configuration", "Pinterest configuration"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Configuration.CustomEvents", "Configure custom events"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Configuration.CustomEvents.SaveBeforeEdit", "You need to save the configuration before edit custom events."),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Configuration.CustomEvents.Fields.EventName", "Event name"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Configuration.CustomEvents.Fields.EventName.Hint", "Enter a name of the custom event (e.g. BlogView)."),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Configuration.CustomEvents.Fields.WidgetZones", "Widget zones"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Configuration.CustomEvents.Fields.WidgetZones.Hint", "Choose widget zones in which the custom event will be tracked (e.g. blogpost_page_top)."),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Configuration.CustomEvents.Search.WidgetZone", "Widget zone"),
                 new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.PinterestAnalytics.Configuration.CustomEvents.Search.WidgetZone.Hint", "Search custom events by the widget zone."),
            };

            return list;
        }

        #endregion

        public bool HideInWidgetList => false;
    }
}