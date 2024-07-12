using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.WidgetPush.Components;
using NopStation.Plugin.Widgets.WidgetPush.Services;

namespace NopStation.Plugin.Widgets.WidgetPush
{
    public class WidgetPushPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        public bool HideInWidgetList => false;

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IWidgetPushItemService _widgetPushItemService;

        #endregion

        #region Ctor

        public WidgetPushPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IWidgetPushItemService widgetPushItemService)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _widgetPushItemService = widgetPushItemService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/WidgetPush/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(WidgetPushViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(_widgetPushItemService.GetAllWidgetZonesAsync().Result);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.WidgetPush.Menu.WidgetPush"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(WidgetPushPermissionProvider.ManageWidgetPush))
            {
                var configure = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.WidgetPush.Menu.Configuration"),
                    Url = "~/Admin/WidgetPush/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "WidgetPush.Configure"
                };
                menuItem.ChildNodes.Add(configure);
                var widgetPushItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.WidgetPush.Menu.WidgetPushItems"),
                    Url = "~/Admin/WidgetPush/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "WidgetPush.WidgetPushItems"
                };
                menuItem.ChildNodes.Add(widgetPushItem);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/widget-push-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=widget-push",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new WidgetPushPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new WidgetPushPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.Menu.WidgetPush", "Widget push"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.Menu.WidgetPushItems", "Widget push items"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.Configuration", "Widget push settings"),

                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.DisplayOrder.Hint", "Display order of the item. 1 represents the top of the list."),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Content", "Content"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Content.Hint", "Enter HTML content."),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Name.Hint", "Enter Name."),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.WidgetZone", "Widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.WidgetZone.Hint", "Enter widget zone name."),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Active.Hint", "Check to mark as active."),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.DisplayEndDate", "Display end date"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.DisplayEndDate.Hint", "Enter display end date."),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.DisplayStartDate", "Display start date"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.DisplayStartDate.Hint", "Enter display start date."),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.LimitedToStores", "Limited to stores"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.LimitedToStores.Hint", "Option to limit this widget item to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),

                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Name.Required", "Name is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Content.Required", "HTML content is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.WidgetZone.Required", "Widget zone is required."),

                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.List", "Widget push items"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.AddNew", "Add new item"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.EditWidgetPushItemDetails", "Edit item details"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.BackToList", "back to item list"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Deleted", "Widget push item deleted successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Updated", "Widget push item updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.WidgetPushItems.Created", "Widget push item created successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.Configuration.Fields.HideInPublicStore", "Hide in public store"),
                new KeyValuePair<string, string>("Admin.NopStation.WidgetPush.Configuration.Fields.HideInPublicStore.Hint", "Check to hide widgte contents in public store.")
            };

            return list;
        }

        #endregion
    }
}
