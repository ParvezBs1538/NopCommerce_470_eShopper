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
using NopStation.Plugin.Widgets.GoogleAnalytics.Components;

namespace NopStation.Plugin.Widgets.GoogleAnalytics
{
    public class NopstationGoogleAnalyticsPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public NopstationGoogleAnalyticsPlugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            ISettingService settingService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        #region Methods

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.HeadHtmlTag });
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/NSGoogleAnalytics/Configure";
        }

        public override async Task InstallAsync()
        {
            var settings = new NopstationGoogleAnalyticsSettings
            {
                GoogleId = "G-XXXXXXXXXX",
                TrackingScript = @"<!-- Global site tag (gtag.js) - Nopstation Google Analytics -->
                <script async src='https://www.googletagmanager.com/gtag/js?id={GOOGLEID}'></script>
                <script>
                  window.dataLayer = window.dataLayer || [];
                  function gtag(){dataLayer.push(arguments);}
                  gtag('js', new Date());
                  
                  gtag('config', '{GOOGLEID}', {
                    'cookie_prefix': 'nopstation'
                 });
                    
                  gtag('config', '{GOOGLEID}');
                  {CUSTOMER_TRACKING}
                  {ECOMMERCE_TRACKING}
                </script>",
                EnableEcommerce = true,
                UseJsToSendEcommerceInfo = true,
                IncludeCustomerId = true,
                SaveLog = false
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new GoogleAnalyticsPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<NopstationGoogleAnalyticsSettings>();

            await this.UninstallPluginAsync(new GoogleAnalyticsPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.Menu.GoogleAnalytics")
            };

            if (await _permissionService.AuthorizeAsync(GoogleAnalyticsPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/NSGoogleAnalytics/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "GoogleAnalytics.Configuration"
                };
                menu.ChildNodes.Add(configItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/google-analytics-4-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=google-analytics-4",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.Menu.GoogleAnalytics", "Google Analytics 4"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.Configuration", "Google analytics 4 settings"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.GoogleId", "ID"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.GoogleId.Hint", "Enter Google Analytics ID."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.ApiSecret", "Enter Api Secret"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.ApiSecret.Hint", "Create your api secret from your google analytics account"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.TrackingScript", "Tracking code"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.TrackingScript.Hint", "Paste the tracking code generated by Google Analytics here. {GOOGLEID} and {CUSTOMER_TRACKING} will be dynamically replaced."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.EnableEcommerce", "Enable eCommerce"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.EnableEcommerce.Hint", "Check to pass information about orders to Google eCommerce feature."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.UseJsToSendEcommerceInfo", "Use JS to send eCommerce info"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.UseJsToSendEcommerceInfo.Hint", "Check to use JS code to send eCommerce info from the order completed page. But in case of redirection payment methods some customers may skip it. Otherwise, eCommerce information will be sent using HTTP request. Information is sent each time an order is paid but UTM is not supported in this mode."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.IncludeCustomerId", "Include customer ID"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.IncludeCustomerId.Hint", "Check to include customer identifier to script and check it from analytics dashboard."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.IncludingTax", "Include tax"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.IncludingTax.Hint", "Check to include tax when generating tracking code for eCommerce part."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.Instructions", "<p>Google Analytics is a free website stats tool from Google. It keeps track of statistics about the visitors and eCommerce conversion on your website.<br /><br />Follow the next steps to enable Google Analytics integration:<br /><ul><li><a href=\"http://www.google.com/analytics/\" target=\"_blank\">Create a Google Analytics account</a> and follow the wizard to add your website</li><li>Copy the Tracking ID into the 'ID' box below</li><li>Click the 'Save' button below and Google Analytics will be integrated into your store</li></ul></p>"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.SaveLog", "Save Log"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.SaveLog.Hint", "Save the request log to show in system log"),
            };
            return list;
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(NSGoogleAnalyticsViewComponent);
        }

        #endregion

        public bool HideInWidgetList => false;
    }
}