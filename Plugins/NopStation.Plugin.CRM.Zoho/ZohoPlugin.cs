using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.CRM.Zoho
{
    public class ZohoPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public ZohoPlugin(ILocalizationService localizationService,
            IScheduleTaskService scheduleTaskService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _scheduleTaskService = scheduleTaskService;
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods 

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ZohoCRM/Configure";
        }

        public override async Task InstallAsync()
        {
            if (await _scheduleTaskService.GetTaskByTypeAsync(ZohoDefaults.DataSynchronizationTaskType) == null)
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
                {
                    Enabled = true,
                    Name = "NopStation - Synchronization with Zoho",
                    Seconds = 3600,
                    StopOnError = false,
                    Type = ZohoDefaults.DataSynchronizationTaskType
                });

            await this.InstallPluginAsync(new ZohoPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            if (await _scheduleTaskService.GetTaskByTypeAsync(ZohoDefaults.DataSynchronizationTaskType) is ScheduleTask task)
                await _scheduleTaskService.DeleteTaskAsync(task);

            await this.UninstallPluginAsync(new ZohoPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.ZohoCRM.Menu.ZohoCRM"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(ZohoPermissionProvider.ManageConfiguration))
            {
                var configMenu = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.ZohoCRM.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ZohoCRM/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ZohoCRM.Configuration"
                };
                menuItem.ChildNodes.Add(configMenu);

                var syncMenu = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.ZohoCRM.Menu.Sync"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ZohoCRM/Sync",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ZohoCRM.Sync"
                };
                menuItem.ChildNodes.Add(syncMenu);

                var mapShipmentMenu = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.ZohoCRM.Menu.Shipment"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ZohoCRM/MapShipmentFields",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ShipmentCRM.Shipment"
                };
                menuItem.ChildNodes.Add(mapShipmentMenu);

                var mapShipmentItemMenu = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.ZohoCRM.Menu.ShipmentItem"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ZohoCRM/MapShipmentItemFields",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ShipmentCRM.ShipmentItem"
                };
                menuItem.ChildNodes.Add(mapShipmentItemMenu);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/zoho-crm-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=zoho-crm",
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
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Menu.ZohoCRM", "Zoho CRM"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Menu.Sync", "Sync"),

                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration", "Zoho CRM settings"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.UseSandbox", "Use sandbox"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.UseSandbox.Hint", "Check to use in sandbox mode."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.DataCenterId", "Data center"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.DataCenterId.Hint", "Select data center based on your account."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.ClientId", "Client id"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.ClientSecret", "Client secret"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.SyncShipment", "Sync shipment"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.ShipmentModuleName", "Shipment module name"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.SyncShipmentItem", "Sync shipment item"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.ShipmentItemModuleName", "Shipment item module name"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.ClientId.Hint", "Enter ZOHO CRM client id."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.ClientSecret.Hint", "Enter ZOHO CRM client secret."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.SyncShipment.Hint", "Sync shipment."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.ShipmentModuleName.Hint", "Enter shipment module name."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.SyncShipmentItem.Hint", "Sync shipment item."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.DataCenterId", "Data center"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.DataCenterId.Hint", "Select data center based on your account."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Fields.ShipmentItemModuleName.Hint", "Enter shipment item module name."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Instructions", "<ul><li>Go to ZOHO API <a href=\"https://api-console.zoho.com/\" target=\"_blank\">Console</a></li><li>Click '<b><i>+ ADD CLIENT</i></b>'.</li><li>Click '<b><i>Server-based Applications</i></b>'</li><li>Enter '<b><i>Client Name</i></b>'</li><li>Enter '<b><i>Homepage URL</i></b>' (https://{yourstore.com})</li><li>Enter '<b><i>Authorized Redirect URIs</i></b>' (https://{yourstore.com}/Admin/ZohoCRM/Authorize)</li><li>Create <a href=\"https://crm.zoho.com/crm/settings/modules/create\" target=\"_blank\">new module(s)</a> to sync Shipments and Shipment Items.</li></ul>"),

                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields", "Map shipmet fields"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields.None", "- None -"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields.NoSalesOrderLookup", "No sales order lookup fields found! Please go to ZOHO Shipment module edit page and configure it."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields.SalesOrderLookupChanged", "Sales order lookup field has been changed in ZOHO CRM. Please click save button to apply that change in nopCommerce database settings."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.OrderField", "Order field"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.TrackingNumberField", "Tracking number field"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.WeightField", "Weight field"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.ShippedDateUtcField", "Shipped date UTC field"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.DeliveryDateUtcField", "Delivery date UTC field"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.OrderField.Hint", "The sale order field (lookup)."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.TrackingNumberField.Hint", "Select tracking number field."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.WeightField.Hint", "Select weight field."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.ShippedDateUtcField.Hint", "Select shipped date UTC field."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentFields.Fields.DeliveryDateUtcField.Hint", "Select delivery date UTC field."),

                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentItemFields", "Map shipmet item fields"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentItemFields.None", "- None -"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentItemFields.NoShipmentLookup", "No shipment lookup fields found! Please go to ZOHO Shipment module edit page and configure it."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentItemFields.NoProductLookup", "No product lookup fields found! Please go to ZOHO Shipment module edit page and configure it."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentItemFields.ShipmentLookupChanged", "Shipment lookup field has been changed in ZOHO CRM. Please click save button to apply that change in nopCommerce database settings."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentItemFields.ProductLookupChanged", "Product lookup field has been changed in ZOHO CRM. Please click save button to apply that change in nopCommerce database settings."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentItemFields.Fields.ShipmentField", "Shipment field"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentItemFields.Fields.QuantityField", "Quantity field"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentItemFields.Fields.ProductField", "Product field"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentItemFields.Fields.ShipmentField.Hint", "The shipment field (lookup)."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentItemFields.Fields.QuantityField.Hint", "Select quantity field."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.MapShipmentItemFields.Fields.ProductField.Hint", "The product field field (lookup)."),

                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Sync", "Sync to ZOHO"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Sync.SyncStatus", "Sync status"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Sync.Fields.SyncTables", "Sync tables"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Sync.Fields.SyncType", "Sync type"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Sync.Fields.SyncTables.Hint", "Select items you want to sync."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Sync.Fields.SyncType.Hint", "Select sync type."),

                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Sync.Close", "Close"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Sync.SyncButton", "Start sync"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.MapShipmentFields.Button", "Map shipment fields"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.MapShipmentItemFields.Button", "Map shipment item fields"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.SyncButton", "Sync"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.OAuthAuthorizationButton", "Authorize"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoCRM.Configuration.Authorized", "Zoho CRM token authorized successfully.")
            };

            return list;

        }

        #endregion
    }
}
