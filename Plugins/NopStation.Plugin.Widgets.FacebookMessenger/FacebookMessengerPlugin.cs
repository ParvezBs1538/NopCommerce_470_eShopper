using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using System;
using NopStation.Plugin.Widgets.FacebookMessenger.Components;

namespace NopStation.Plugin.Widgets.FacebookMessenger
{
    public class FacebookMessengerPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public FacebookMessengerPlugin(ILocalizationService localizationService,
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

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.Footer
            });
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/FacebookMessenger/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(FacebookMessengerViewComponent);
        }

        public bool HideInWidgetList => false;
        
        public override async Task InstallAsync()
        {
            //settings
            var fbMessengerSetting = new FacebookMessengerSettings
            {
                PageId = "136656911884",
                ThemeColor = "#0084ff",
                EnableScript = false,
                Script = @"<!-- Load Facebook SDK for JavaScript -->
                    <div id='fb-root'></div>
                        <script>
                                  window.fbAsyncInit = function() {
                                    FB.init({
                                      xfbml            : true,
                                      version          : 'v4.0'
                                    });
                                  };
                                  (function(d, s, id) {
                                  var js, fjs = d.getElementsByTagName(s)[0];
                                  if (d.getElementById(id)) return;
                                  js = d.createElement(s); js.id = id;
                                  js.src = 'https://connect.facebook.net/en_US/sdk/xfbml.customerchat.js';
                                  fjs.parentNode.insertBefore(js, fjs);
                                }(document, 'script', 'facebook-jssdk'));
                        </script>
                    <!-- Your customer chat code -->
                    <div class='fb-customerchat'
                    attribution = setup_tool
                    page_id = '136656911884'
                    theme_color = '#0084ff' >
                    </div>"
            };

            await _settingService.SaveSettingAsync(fbMessengerSetting);

            await this.InstallPluginAsync(new FacebookMessengerPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<FacebookMessengerSettings>();

            await this.UninstallPluginAsync(new FacebookMessengerPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Configuration.Instructions", "<p>Put the pageid and the theme color in the input box or enable script and put the script in the script box.</p>"),

                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Configuration.Fields.PageId", "Page ID"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Configuration.Fields.PageId.Hint", "Enter page id."),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Configuration.Fields.ThemeColor.Hint", "To determine theme color."),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Configuration.Fields.ThemeColor", "Theme color"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Configuration.Fields.Script", "Script"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Configuration.Fields.Script.Hint", "This field for script."),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Configuration.Fields.EnableScript", "Enable script"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Configuration.Fields.EnableScript.Hint", "To active script."),

                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Configuration", "Facebook messenger settings"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookMessenger.Menu.FacebookMessenger", "Facebook messenger"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.FacebookMessenger.Menu.FacebookMessenger"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(FacebookMessengerPermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.FacebookMessenger.Menu.Configuration"),
                    Url = "~/Admin/FacebookMessenger/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "FacebookMessenger.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/fb-messenger-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=fb-messenger",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }
                
            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }
    }
}
