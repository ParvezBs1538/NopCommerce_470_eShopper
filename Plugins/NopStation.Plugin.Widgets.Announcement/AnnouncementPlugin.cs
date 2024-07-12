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
using NopStation.Plugin.Widgets.Announcement.Components;

namespace NopStation.Plugin.Widgets.Announcement;

public class AnnouncementPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
{
    #region Fields

    public bool HideInWidgetList => false;

    private readonly IWebHelper _webHelper;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly AnnouncementSettings _announcementSettings;

    #endregion

    #region Ctor

    public AnnouncementPlugin(IWebHelper webHelper,
        INopStationCoreService nopStationCoreService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        ISettingService settingService,
        AnnouncementSettings announcementSettings)
    {
        _webHelper = webHelper;
        _nopStationCoreService = nopStationCoreService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _announcementSettings = announcementSettings;
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/Announcement/Configure";
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> { _announcementSettings.WidgetZone });
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.Announcement.Menu.Announcement"),
            Visible = true,
            IconClass = "far fa-dot-circle"
        };

        if (await _permissionService.AuthorizeAsync(AnnouncementPermissionProvider.ManageAnnouncement))
        {
            var configure = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Announcement.Menu.Configuration"),
                Url = $"{_webHelper.GetStoreLocation()}Admin/Announcement/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "Announcement.Configure"
            };
            menuItem.ChildNodes.Add(configure);
            var announcementItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Announcement.Menu.AnnouncementItems"),
                Url = $"{_webHelper.GetStoreLocation()}Admin/Announcement/List",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "Announcement.AnnouncementItems"
            };
            menuItem.ChildNodes.Add(announcementItem);
        }
        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
        {
            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/announcement-documentation",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);
        }

        await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
    }

    public override async Task InstallAsync()
    {
        var settings = new AnnouncementSettings()
        {
            WidgetZone = PublicWidgetZones.HeaderMiddle,
            ItemSeparator = ">>"
        };
        await _settingService.SaveSettingAsync(settings);

        await this.InstallPluginAsync(new AnnouncementPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new AnnouncementPermissionProvider());
        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new List<KeyValuePair<string, string>>();

        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Menu.Announcement", "Announcement"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Menu.AnnouncementItems", "Announcement items"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Menu.Configuration", "Configuration"));

        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Configuration", "Announcement settings"));

        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.DisplayOrder", "Display order"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.DisplayOrder.Hint", "Display order of the item. 1 represents the top of the list."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.Title", "Title"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.Title.Hint", "The announcement text."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.Description", "Description"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.Description.Hint", "The announcement description."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.Name", "Name"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.Name.Hint", "Enter name."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.Color", "Color"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.Color.Hint", "Select item color."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.Active", "Active"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.Active.Hint", "Check to mark as active."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.DisplayEndDate", "Display end date"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.DisplayEndDate.Hint", "Enter display end date."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.DisplayStartDate", "Display start date"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.DisplayStartDate.Hint", "Enter display start date."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.LimitedToStores", "Limited to stores"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.LimitedToStores.Hint", "Option to limit this widget item to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.AclCustomerRoles", "Customer roles"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.AclCustomerRoles.Hint", "Select customer roles for which the announcement will be shown. Leave empty if you want this announcement to be visible to all users."));

        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.Title.Required", "The title is required."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Fields.Name.Required", "The name is required."));

        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.List", "Announcement items"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.List", "Announcement items"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.AddNew", "Add new item"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.EditDetails", "Edit item details"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.BackToList", "back to item list"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Deleted", "Announcement item deleted successfully."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Updated", "Announcement item updated successfully."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Created", "Announcement item created successfully."));

        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.AnnouncementItems.Tab.Info", "Info"));

        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Configuration.Fields.EnablePlugin", "Enable plugin"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Configuration.Fields.AllowCustomersToMinimize", "Allow customers to minimize"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Configuration.Fields.AllowCustomersToMinimize.Hint", "Determines whether allow customers to minimize the announcement bar."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Configuration.Fields.AllowCustomersToClose", "Allow customers to close"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Configuration.Fields.AllowCustomersToClose.Hint", "Determines whether allow customers to close the announcement bar."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Configuration.Fields.DisplayType", "Display type"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Configuration.Fields.DisplayType.Hint", "Announcement display type."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Configuration.Fields.ItemSeparator", "Announcement item separator"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Configuration.Fields.ItemSeparator.Hint", "Set announcement item separator."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Configuration.Fields.WidgetZone", "Widget zone"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.Announcement.Configuration.Fields.WidgetZone.Hint", "Enter widget zone where you want to display announcement in public store (i.e. header_middle)."));

        return list;
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(AnnouncementViewComponent);
    }

    #endregion
}
