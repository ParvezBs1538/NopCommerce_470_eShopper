using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;

namespace NopStation.Plugin.Misc.IpFilter
{
    public class IpFilterPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;

        public IpFilterPlugin(IWebHelper webHelper,
            ISettingService settingService,
            ILocalizationService localizationService,
            INopStationCoreService nopStationCoreServicee,
            IPermissionService permissionService)
        {
            _webHelper = webHelper;
            _settingService = settingService;
            _localizationService = localizationService;
            _nopStationCoreService = nopStationCoreServicee;
            _permissionService = permissionService;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.Menu.IpFilter"),
                SystemName = "NopIpFilter",
                IconClass = "far fa-dot-circle",
                Visible = true
            };

            if (await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.Menu.Configuration"),
                    Url = _webHelper.GetStoreLocation() + "Admin/IpFilter/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "IpFilter.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }

            if (await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageIpBlockRules))
            {
                var ipBlockRuleItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.Menu.IpBlockRules"),
                    Url = "~/Admin/IpBlockRule/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "IpFilter.IpBlockRules"
                };
                menuItem.ChildNodes.Add(ipBlockRuleItem);
            }

            if (await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageCountryBlockRules))
            {
                var ipRangeBlockRuleItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.Menu.IpRangeBlockRules"),
                    Url = "~/Admin/IpRangeBlockRule/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "IpFilter.IpRangeBlockRules"
                };
                menuItem.ChildNodes.Add(ipRangeBlockRuleItem);
            }

            if (await _permissionService.AuthorizeAsync(IpFilterPermissionProvider.ManageCountryBlockRules))
            {
                var countryBlockRuleItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.IpFilter.Menu.CountryBlockRules"),
                    Url = "~/Admin/CountryBlockRule/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "IpFilter.CountryBlockRules"
                };
                menuItem.ChildNodes.Add(countryBlockRuleItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/ip-filter-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=ip-filter",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/IpFilter/Configure";
        }

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new IpFilterSettings()
            {
                IsEnabled = true
            });

            await this.InstallPluginAsync(new IpFilterPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new IpFilterPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.Menu.IpFilter", "Ip filter"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.Menu.IpBlockRules", "Ip block rules"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.Menu.IpRangeBlockRules", "Ip range block rules"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.Menu.CountryBlockRules", "Country block rules"),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.Configuration", "Ip filter settings"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.Configuration.Instructions", "When you update <b>settings</b> of iP filter then you also have to <b>Restart application</b>."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.Configuration.Fields.IsEnabled", "Is enabled"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.Configuration.Fields.IsEnabled.Hint", "Determines whether plugin is enabled or not. Please restart application after changing this value."),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Fields.IpAddress", "Ip address"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Fields.Location", "Location"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Fields.Comment", "Comment"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Fields.IsAllowed", "Is allowed"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Fields.IpAddress.Hint", "Define ip address."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Fields.Location.Hint", "Define location of ip address."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Fields.Comment.Hint", "Enter comment."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Fields.CreatedOn.Hint", "The create date of this ip block rule."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Fields.IsAllowed.Hint", "Defines ip address is allowed or not."),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Fields.IpAddress.Required", "Ip address is required."),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.List.CreatedFrom", "Created from"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.List.CreatedTo", "Created to"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.List.CreatedFrom.Hint", "Search created from date."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.List.CreatedTo.Hint", "Search created to date."),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Created", "Ip block rule has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Updated", "Ip block rule has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.Deleted", "Ip block rule has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.List", "Ip block rules"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.EditDetails", "Edit ip block rule details"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.AddNew", "Add new ip block rule"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpBlockRules.BackToList", "back to ip block rule list"),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.FromIpAddress", "From ip address"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.ToIpAddress", "To ip address"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.Comment", "Comment"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.FromIpAddress.Hint", "Define from ip address."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.ToIpAddress.Hint", "Define to ip address."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.Comment.Hint", "Enter comment."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.CreatedOn.Hint", "The create date of this ip block rule."),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.FromIpAddress.Required", "From ip address is required."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.Fields.ToIpAddress.Required", "To ip address is required."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.InvalidIpAddress", "Invalid ip address."),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.List.CreatedFrom", "Created from"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.List.CreatedTo", "Created to"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.List.CreatedFrom.Hint", "Search created from date."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.List.CreatedTo.Hint", "Search created to date."),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.Created", "Ip range block rule has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.Updated", "Ip range block rule has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.Deleted", "Ip range block rule has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.List", "Ip range block rules"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.EditDetails", "Edit ip range block rule details"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.AddNew", "Add new ip range block rule"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.IpRangeBlockRules.BackToList", "back to ip range block rule list"),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.Fields.Country", "Country"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.Fields.Comment", "Comment"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.Fields.Country.Hint", "Select country."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.Fields.Comment.Hint", "Enter comment."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.Fields.CreatedOn.Hint", "The create date of this ip block rule."),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.Fields.Country.Required", "Country is required."),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.List.CreatedFrom", "Created from"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.List.CreatedTo", "Created to"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.List.CreatedFrom.Hint", "Search created from date."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.List.CreatedTo.Hint", "Search created to date."),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.Created", "Country block rule has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.Updated", "Country block rule has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.Deleted", "Country block rule has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.List", "Country block rules"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.EditDetails", "Edit country block rule details"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.AddNew", "Add new country block rule"),
                new KeyValuePair<string, string>("Admin.NopStation.IpFilter.CountryBlockRules.BackToList", "back to country block rule list")
            };

            return list;
        }
    }
}