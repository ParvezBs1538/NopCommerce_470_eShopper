using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.NopChat.Components;

namespace NopStation.Plugin.Widgets.NopChat
{
    public class NopChatPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;
        private readonly INopFileProvider _fileProvider;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        #endregion

        public NopChatPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ISettingService settingService,
            ICustomerService customerService,
            IWorkContext workContext,
            IPictureService pictureService,
            INopFileProvider fileProvider,
            ILocalizationService localizationService,
            IPermissionService permissionService)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _settingService = settingService;
            _customerService = customerService;
            _workContext = workContext;
            _pictureService = pictureService;
            _fileProvider = fileProvider;
            _localizationService = localizationService;
            _permissionService = permissionService;
        }

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/NopChatAdmin/Configure";
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.Footer,
                PublicWidgetZones.VendorDetailsTop, PublicWidgetZones.ProductDetailsInsideOverviewButtonsAfter });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(WidgetsNopChatViewComponent);
        }

        public override async Task InstallAsync()
        {
            var sampleImagesPath = _fileProvider.MapPath("~/Plugins/NopStation.Plugin.Widgets.NopChat/");
            var pictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(
                _fileProvider.Combine(sampleImagesPath, "logo.png")), MimeTypes.ImagePng, "logo")).Id;

            var nopChatSettings = new NopChatSettings()
            {
                OpenOnMonday = true,
                OpenOnTuesday = true,
                OpenOnWednesday = true,
                OpenOnThursday = true,
                OpenOnFriday = true,
                OpenOnSaturday = true,
                OpenOnSunday = true,
                Logo = pictureId
            };

            await _settingService.SaveSettingAsync(nopChatSettings);
            await this.InstallPluginAsync();
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync();
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var menuItem = new SiteMapNode()
            {
                Title = "Nop Chat",
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            var configure = new SiteMapNode()
            {
                Title = "Configure",
                Url = "/Admin/NopChatAdmin/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "NopChat.Configure"
            };

            menuItem.ChildNodes.Add(configure);

            var chatBox = new SiteMapNode()
            {
                Title = "Inbox",
                Url = "/Admin/NopChatAdmin/AdminChatBox",
                Visible = await _customerService.IsAdminAsync(customer) ? false : true,
                IconClass = "far fa-circle",
                SystemName = "NopChat.ChatBox"
            };

            menuItem.ChildNodes.Add(chatBox);

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/nopchat-plugin-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=nopchat-plugin-documentation",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.Logo", "Logo"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.Logo.Hint", "This logo will appear in the inbox left corner"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnMonday", "Monday"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnTuesday", "Tuesday"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnWednesday", "Wednesday"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnThursday", "Thursday"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnFriday", "Friday"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnSaturday", "Saturday"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Fields.OpenOnSunday", "Sunday"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Settings", "Settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.OpenOnDays", "Open on days"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.ConnectionStatus.Hint", "Check your connection with SignalR"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.CheckSignalR.ConnectionStatus", "Connection Status"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.CheckSignalR.Connecting", "Connecting ..."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.CheckSignalR.Button", "Test Connection"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Button.ChatWithVendor", "Chat With Vendor"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.NopChat.Configuration.Title", "Nop Chat Configuration")
            };

            return list;
        }

        #endregion

        public bool HideInWidgetList => false;
    }
}