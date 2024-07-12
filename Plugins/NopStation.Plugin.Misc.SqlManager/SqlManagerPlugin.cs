using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.SqlManager
{
    public class SqlManagerPlugin : BasePlugin, INopStationPlugin, IAdminMenuPlugin, IMiscPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public SqlManagerPlugin(ILocalizationService localizationService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new SqlManagerPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new SqlManagerPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.Menu.SqlManager")
            };

            if (await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageReports))
            {
                var campaign = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.Menu.SqlReports"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/SqlReport/List",
                    SystemName = "SqlReports"
                };
                menu.ChildNodes.Add(campaign);
            }

            if (await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ViewReports))
            {
                var campaign = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.Menu.ViewReports"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/SqlReport/ViewList",
                    SystemName = "ViewSqlReports"
                };
                menu.ChildNodes.Add(campaign);
            }

            if (await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ManageParameters))
            {
                var campaign = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.Menu.SqlParameters"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/SqlParameter/List",
                    SystemName = "SqlParameters"
                };
                menu.ChildNodes.Add(campaign);
            }

            if (await _permissionService.AuthorizeAsync(SqlManagerPermissionProvider.ViewReports))
            {
                var campaign = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.SqlManager.Menu.InstantQuery"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/InstantQuery/RunQuery",
                    SystemName = "RunQuery"
                };
                menu.ChildNodes.Add(campaign);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/sql-manager-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=sql-manager",
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
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.Menu.SqlManager", "Sql manager"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.Menu.SqlReports", "Reports"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.Menu.SqlParameters", "Parameters"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.Menu.ViewReports", "View reports"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.Menu.InstantQuery", "Instant query"),


                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Created", "Sql parameter created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Updated", "Sql parameter updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Deleted", "Sql parameter deleted successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.AlreadyExists", "Sql parameter system name already exists."),

                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.BackToList", "back to sql parameter list"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters", "Sql parameters"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.AddNew", "Add new sql parameter"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.EditDetails", "Add new sql parameter"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Info", "Info"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.DefaultValues", "Default values"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Values.SaveBeforeEdit", "You need to save the sql parameter before you can add values for this sql parameter page."),

                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Fields.SystemName", "System name"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Fields.DataType", "Data type"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Fields.Name.Hint", "Sql parameter name."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Fields.SystemName.Hint", "Sql parameter system name."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Fields.DataType.Hint", "Sql parameter data type."),

                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Fields.Name.Required", "The sql parameter name is required."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Fields.SystemName.Required", "The sql parameter system name is required."),

                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Values.Button.AddNew", "Add new value"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Values.AddNew", "Add new value"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Values", "Values"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Values.Alert.AddNew", "Insert value first."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameters.Values.Alert.ValueAdd", "Failed to add parameter value."),

                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameterValues.Fields.Value", "Value"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameterValues.Fields.Value.IsValid", "Is Valid"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameterValues.Fields.Value.Hint", "Sql parameter value."),

                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlParameterValues.Fields.Value.Required", "The sql parameter value is required."),

                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Created", "Sql report created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Updated", "Sql report updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Deleted", "Sql report deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.BackToList", "back to sql report list"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports", "Sql reports"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.AddNew", "Add new sql report"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.EditDetails", "Add new sql report"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Generate", "Generate report"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Export", "Export to excel"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Viewer", "Report viewer"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Common.Name", "Report name"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Select.ParameterValues", "Parameter values"),

                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.Description", "Description"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.Query", "Query"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.AclCustomerRoles", "Limited to customer roles"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.AvailableParameters", "Available parameters"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.UpdatedOn", "Updated on"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.Name.Hint", "Sql query name."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.Description.Hint", "Sql query description."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.Query.Hint", "Sql query text."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.AclCustomerRoles.Hint", "Select customer roles for which the report will be shown. Leave empty if you want this report to be visible to all users."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.AvailableParameters.Hint", "Available sql parameters."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.CreatedOn.Hint", "Created on."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.UpdatedOn.Hint", "Updated on."),

                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.ViewReports", "View reports"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.ViewReport", "View report"),

                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.InstantQuery.Run", "Run query"),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.InstantQuery.Output", "Output"),

                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.Name.Required", "The sql query name is required."),
                new KeyValuePair<string, string>("Admin.NopStation.SqlManager.SqlReports.Fields.Query.Required", "The sql query text is required."),
            };

            return list;
        }

        #endregion
    }
}