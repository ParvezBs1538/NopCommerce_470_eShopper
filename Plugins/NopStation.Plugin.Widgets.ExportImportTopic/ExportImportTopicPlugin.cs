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
using NopStation.Plugin.Widgets.PopupLogin.Components;

namespace NopStation.Plugin.Widgets.ExportImportTopic
{
    public class ExportImportTopicPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {

        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        public bool HideInWidgetList => false;

        #endregion

        #region Ctor

        public ExportImportTopicPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new ExportImportTopicSettings
            {
                CheckBodyMaximumLength = true,
                BodyMaximumLength = 32767
            });

            await this.InstallPluginAsync(new ExportImportTopicPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new ExportImportTopicPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.ExportImportTopic.Menu.ExportImportTopic", "Ex-Im topic"),
                new KeyValuePair<string, string>("Admin.NopStation.ExportImportTopic.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.ExportImportTopic.Topics.Import.StoresDontExist", "Stores with the following names and/or IDs don't exist: {0}"),
                new KeyValuePair<string, string>("Admin.NopStation.ExportImportTopic.Topics.Import.CustomerRolesDontExist", "Customer roles with the following names and/or IDs don't exist: {0}"),
                new KeyValuePair<string, string>("Admin.NopStation.ExportImportTopic.ImportFromExcelTip", "Imported topics are distinguished by System Name. If the System Name already exists, then its corresponding topic will be updated."),
                new KeyValuePair<string, string>("Admin.NopStation.ExportImportTopic.Imported", "Topics have been imported successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.ExportImportTopic.Configuration", "Topic export-import settings"),
                new KeyValuePair<string, string>("Admin.NopStation.ExportImportTopic.Configuration.Fields.CheckBodyMaximumLength", "Check body maximum length"),
                new KeyValuePair<string, string>("Admin.NopStation.ExportImportTopic.Configuration.Fields.CheckBodyMaximumLength.Hint", "Defines whether to check topic body maximum length."),
                new KeyValuePair<string, string>("Admin.NopStation.ExportImportTopic.Configuration.Fields.BodyMaximumLength", "Body maximum length"),
                new KeyValuePair<string, string>("Admin.NopStation.ExportImportTopic.Configuration.Fields.BodyMaximumLength.Hint", "Define topic body maximum length."),
            };

            return list;
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.TopicListButtons });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(AdminExportImportTopicViewComponent);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ExportImportTopic/Configure";
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.ExportImportTopic.Menu.ExportImportTopic"),
                Visible = true,
                IconClass = "far fa-dot-circle"
            };

            if (await _permissionService.AuthorizeAsync(ExportImportTopicPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.ExportImportTopic.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ExportImportTopic/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ExportImportTopic.Configure"
                };
                menuItem.ChildNodes.Add(configItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/export-import-topic-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=export-import-topic",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        #endregion
    }
}
