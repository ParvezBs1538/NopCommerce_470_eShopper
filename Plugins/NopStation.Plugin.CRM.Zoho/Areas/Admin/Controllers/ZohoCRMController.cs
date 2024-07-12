using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using NopStation.Plugin.CRM.Zoho.Areas.Admin.Models;
using NopStation.Plugin.CRM.Zoho.Domain;
using NopStation.Plugin.CRM.Zoho.Services;
using NopStation.Plugin.CRM.Zoho.Services.Zoho.Response;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.CRM.Zoho.Areas.Admin.Controllers
{
    public class ZohoCRMController : NopStationAdminController
    {
        #region Fields

        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IZohoService _zohoService;
        private readonly ZohoCRMSettings _zohoCRMSettings;
        private readonly ZohoSyncHub _zohoSyncHub;
        private readonly ZohoCRMShipmentSettings _zohoCRMShipmentSettings;

        #endregion

        #region Ctor

        public ZohoCRMController(INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IScheduleTaskService scheduleTaskService,
            IZohoService zohoService,
            ZohoCRMSettings zohoCRMSettings,
            ZohoSyncHub zohoSyncHub,
            ZohoCRMShipmentSettings zohoCRMShipmentSettings)
        {
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _scheduleTaskService = scheduleTaskService;
            _zohoService = zohoService;
            _zohoCRMSettings = zohoCRMSettings;
            _zohoSyncHub = zohoSyncHub;
            _zohoCRMShipmentSettings = zohoCRMShipmentSettings;
        }

        #endregion

        #region Utilities

        protected bool CanSync()
        {
            return !string.IsNullOrWhiteSpace(_zohoCRMSettings.RefreshToken) &&
                !string.IsNullOrWhiteSpace(_zohoCRMSettings.AccessToken);
        }

        protected bool CustomModule(ModuleResponse moduleResponse)
        {
            if (!moduleResponse.ApiSupported || !moduleResponse.Creatable)
                return false;

            var defaults = new List<string> { "Leads", "Contacts", "Accounts", "Deals", "Home",
                "Activities", "Products", "Quotes", "Sales_Orders", "Purchase_Orders", "Reports",
                "Invoices", "Campaigns", "Vendors", "Price_Books", "Cases", "Solutions", "Dashboards",
                "Tasks", "Events", "Notes", "Attachments", "Calls", "Untitled", "SalesInbox", "Feeds" };

            return !defaults.Contains(moduleResponse.ApiName);
        }

        #endregion

        #region Methods

        #region Configure / authorize

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(ZohoPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                UseSandbox = _zohoCRMSettings.UseSandbox,
                DataCenterId = _zohoCRMSettings.DataCenterId,
                ClientId = _zohoCRMSettings.ClientId,
                ClientSecret = _zohoCRMSettings.ClientSecret,
                ShipmentModuleName = _zohoCRMSettings.ShipmentModuleName,
                SyncShipment = _zohoCRMSettings.SyncShipment,
                SyncShipmentItem = _zohoCRMSettings.SyncShipmentItem,
                ShipmentItemModuleName = _zohoCRMSettings.ShipmentItemModuleName,
                OAuthUrl = _zohoService.GetAuthorizationUrl(_zohoCRMSettings.ClientId),
                CanSync = CanSync()
            };

            if (model.CanSync)
            {
                var modulesBase = await _zohoService.GetModulesAsync();
                if (!string.IsNullOrWhiteSpace(modulesBase.Error))
                    _notificationService.WarningNotification(modulesBase.Error);

                model.AvailableModules = modulesBase.Modules.Where(x => CustomModule(x))
                    .Select(x => new SelectListItem
                    {
                        Value = x.ApiName,
                        Text = x.PluralLabel
                    }).ToList();
            }
            else
            {
                model.AvailableModules.Add(new SelectListItem
                {
                    Text = "-",
                    Value = ""
                });
            }

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ZohoPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            _zohoCRMSettings.UseSandbox = model.UseSandbox;
            _zohoCRMSettings.DataCenterId = model.DataCenterId;
            _zohoCRMSettings.ClientId = model.ClientId;
            _zohoCRMSettings.ClientSecret = model.ClientSecret;
            _zohoCRMSettings.ShipmentModuleName = model.ShipmentModuleName;
            _zohoCRMSettings.SyncShipment = model.SyncShipment;
            _zohoCRMSettings.SyncShipmentItem = model.SyncShipmentItem;
            _zohoCRMSettings.ShipmentItemModuleName = model.ShipmentItemModuleName;

            await _settingService.SaveSettingAsync(_zohoCRMSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        public async Task<IActionResult> Authorize(string code)
        {
            if (!await _permissionService.AuthorizeAsync(ZohoPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var authResponse = await _zohoService.AuthorizeAccessTokenAsync(code);

            if (!string.IsNullOrEmpty(authResponse.Error))
            {
                var errorMessage = authResponse.Error switch
                {
                    "invalid_grant" => "Authorize Error: Grant is not valid",
                    "invalid_code" => "Authorize Error: Code is not valid",
                    "invalid_redirect_uri" => "Authorize Error: Redirect URL is not valid",
                    _ => "Authorize Error: Undefined Error",
                };

                ViewBag.Message = errorMessage;

                return View();
            }

            _zohoCRMSettings.RefreshToken = authResponse.RefreshToken;
            _zohoCRMSettings.AccessToken = authResponse.AccessToken;
            _zohoCRMSettings.ExpireOnUtc = DateTime.UtcNow.AddSeconds(authResponse.ExpiresIn);

            ViewBag.Message = await _localizationService.GetResourceAsync("Admin.NopStation.ZohoCRM.Configuration.Authorized");
            await _settingService.SaveSettingAsync(_zohoCRMSettings);

            return View();
        }

        #endregion

        #region Sync

        public async Task<IActionResult> Sync()
        {
            if (!await _permissionService.AuthorizeAsync(ZohoPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = new SyncModel
            {
                ClientId = _zohoCRMSettings.ClientId,
                ClientSecret = _zohoCRMSettings.ClientSecret,
                OAuthUrl = _zohoService.GetAuthorizationUrl(_zohoCRMSettings.ClientId),
                CanSync = CanSync(),
                AvailableTables = ZohoDefaults.SyncTables.ToListItems()
            };

            if (!_zohoCRMSettings.SyncShipment && model.AvailableTables.FirstOrDefault(x => x.Value == "Shipments") is SelectListItem sli1)
                model.AvailableTables.Remove(sli1);

            if ((!_zohoCRMSettings.SyncShipment || !_zohoCRMSettings.SyncShipmentItem) &&
                model.AvailableTables.FirstOrDefault(x => x.Value == "ShipmentItems") is SelectListItem sli2)
                model.AvailableTables.Remove(sli2);

            return View(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> Sync(SyncModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ZohoPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var completed = new List<string>();
            if (_zohoCRMSettings.ExpireOnUtc.AddMinutes(-5) <= DateTime.UtcNow)
                await _zohoService.RefreshAccessTokenAsync();

            await _zohoSyncHub.SyncZohoItemsAsync("OpenModal", model.SyncTables, completed, "");

            foreach (var item in ZohoDefaults.SyncTables)
            {
                if (!model.SyncTables.Contains(item.Key))
                    continue;

                try
                {
                    await _zohoSyncHub.SyncZohoItemsAsync(item.Key, model.SyncTables, completed, "");
                    switch (item.Key)
                    {
                        case "Stores":
                            await _zohoService.SyncStoresAsync((SyncType)model.SyncType);
                            break;
                        case "Vendors":
                            await _zohoService.SyncVendorsAsync((SyncType)model.SyncType);
                            break;
                        case "Customers":
                            await _zohoService.SyncCustomersAsync((SyncType)model.SyncType);
                            break;
                        case "Products":
                            await _zohoService.SyncProductsAsync((SyncType)model.SyncType);
                            break;
                        case "Orders":
                            await _zohoService.SyncOrdersAsync((SyncType)model.SyncType);
                            break;
                        case "Shipments":
                            if (_zohoCRMSettings.SyncShipment)
                                await _zohoService.SyncShipmentsAsync((SyncType)model.SyncType);
                            break;
                        case "ShipmentItems":
                            if (_zohoCRMSettings.SyncShipment && _zohoCRMSettings.SyncShipmentItem)
                                await _zohoService.SyncShipmentItemsAsync((SyncType)model.SyncType);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    await _zohoSyncHub.SyncZohoItemsAsync(item.Key, model.SyncTables, completed, ex.Message);
                }

                completed.Add(item.Key);
            }

            await _zohoSyncHub.SyncZohoItemsAsync("ShowCloseButton", model.SyncTables, completed, "");

            return Json(new { });
        }

        #endregion

        #region Map

        public async Task<IActionResult> MapShipmentFields()
        {
            if (!await _permissionService.AuthorizeAsync(ZohoPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (_zohoCRMSettings.ExpireOnUtc.AddMinutes(-5) <= DateTime.UtcNow)
                await _zohoService.RefreshAccessTokenAsync();

            var fieldsResponse = await _zohoService.GetModuleFieldsAsync(_zohoCRMSettings.ShipmentModuleName);

            if (!string.IsNullOrWhiteSpace(fieldsResponse.Error))
                _notificationService.WarningNotification(fieldsResponse.Error);

            var model = new MapShipmentFieldsModel()
            {
                DeliveryDateUtcField = _zohoCRMShipmentSettings.DeliveryDateUtcField,
                OrderField = _zohoCRMShipmentSettings.OrderField,
                ShippedDateUtcField = _zohoCRMShipmentSettings.ShippedDateUtcField,
                TrackingNumberField = _zohoCRMShipmentSettings.TrackingNumberField,
                WeightField = _zohoCRMShipmentSettings.WeightField
            };

            //var customFields = fieldsResponse.Fields.Where(x => x.CustomField).ToList();
            //var salesOrderField = customFields.FirstOrDefault(x => x.ApiName == "Sales_Orders");
            //var salesOrderFieldModule = customFields.FirstOrDefault(x => x.Lookup?.Module == "Sales_Orders");

            //if (fieldsResponse.Fields.FirstOrDefault(x => x.CustomField &&
            //    x.ApiName == "Sales_Orders") is ModuleFieldResponse moduleField)

            if (fieldsResponse.Fields.FirstOrDefault(x => x.CustomField &&
                x.Lookup?.Module == "Sales_Orders") is ModuleFieldResponse moduleField)
            {
                model.OrderField = moduleField.ApiName;
                if (!string.IsNullOrWhiteSpace(_zohoCRMShipmentSettings.OrderField) &&
                    _zohoCRMShipmentSettings.OrderField != moduleField.ApiName)
                    _notificationService.WarningNotification(await _localizationService
                        .GetResourceAsync("Admin.NopStation.ZohoCRM.MapShipmentFields.SalesOrderLookupChanged"));
            }
            else
                _notificationService.WarningNotification(await _localizationService
                    .GetResourceAsync("Admin.NopStation.ZohoCRM.MapShipmentFields.NoSalesOrderLookup"));

            foreach (var x in fieldsResponse.Fields.Where(x => x.CustomField))
            {
                model.AvailableFields.Add(new SelectListItem
                {
                    Value = x.ApiName,
                    Text = $"{x.FieldLabel} ({x.DataType})"
                });
            }
            model.AvailableFields.Insert(0, new SelectListItem()
            {
                Text = await _localizationService
                .GetResourceAsync("Admin.NopStation.ZohoCRM.MapShipmentFields.None"),
                Value = ""
            });

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> MapShipmentFields(MapShipmentFieldsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ZohoPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            _zohoCRMShipmentSettings.DeliveryDateUtcField = model.DeliveryDateUtcField;
            _zohoCRMShipmentSettings.OrderField = model.OrderField;
            _zohoCRMShipmentSettings.ShippedDateUtcField = model.ShippedDateUtcField;
            _zohoCRMShipmentSettings.TrackingNumberField = model.TrackingNumberField;
            _zohoCRMShipmentSettings.WeightField = model.WeightField;

            await _settingService.SaveSettingAsync(_zohoCRMShipmentSettings);
            model.CloseWindow = true;

            return View(model);
        }

        public async Task<IActionResult> MapShipmentItemFields()
        {
            if (!await _permissionService.AuthorizeAsync(ZohoPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (_zohoCRMSettings.ExpireOnUtc.AddMinutes(-5) <= DateTime.UtcNow)
                await _zohoService.RefreshAccessTokenAsync();

            var fieldsResponse = await _zohoService.GetModuleFieldsAsync(_zohoCRMSettings.ShipmentItemModuleName);
            if (!string.IsNullOrWhiteSpace(fieldsResponse.Error))
                _notificationService.WarningNotification(fieldsResponse.Error);

            var model = new MapShipmentItemFieldsModel()
            {
                QuantityField = _zohoCRMShipmentSettings.ItemQuantityField,
                ShipmentField = _zohoCRMShipmentSettings.ItemShipmentField,
                ProductField = _zohoCRMShipmentSettings.ItemProductField
            };

            var customFields = fieldsResponse.Fields.Where(x => x.CustomField).ToList();

            if (fieldsResponse.Fields.FirstOrDefault(x => x.CustomField &&
                x.Lookup?.Module == _zohoCRMSettings.ShipmentModuleName) is ModuleFieldResponse moduleField)
            {
                model.ShipmentField = moduleField.ApiName;
                if (!string.IsNullOrWhiteSpace(_zohoCRMShipmentSettings.ItemShipmentField) &&
                    _zohoCRMShipmentSettings.ItemShipmentField != moduleField.ApiName)
                    _notificationService.WarningNotification(await _localizationService
                        .GetResourceAsync("Admin.NopStation.ZohoCRM.MapShipmentItemFields.ShipmentLookupChanged"));
            }
            else
                _notificationService.WarningNotification(await _localizationService
                    .GetResourceAsync("Admin.NopStation.ZohoCRM.MapShipmentItemFields.NoShipmentLookup"));

            if (fieldsResponse.Fields.FirstOrDefault(x => x.CustomField &&
                x.Lookup?.Module == "Products") is ModuleFieldResponse moduleField1)
            {
                model.ProductField = moduleField1.ApiName;
                if (!string.IsNullOrWhiteSpace(_zohoCRMShipmentSettings.ItemProductField) &&
                    _zohoCRMShipmentSettings.ItemProductField != moduleField1.ApiName)
                    _notificationService.WarningNotification(await _localizationService
                        .GetResourceAsync("Admin.NopStation.ZohoCRM.MapShipmentItemFields.ProductLookupChanged"));
            }
            else
                _notificationService.WarningNotification(await _localizationService
                    .GetResourceAsync("Admin.NopStation.ZohoCRM.MapShipmentItemFields.NoProductLookup"));

            foreach (var x in fieldsResponse.Fields.Where(x => x.CustomField))
            {
                model.AvailableFields.Add(new SelectListItem
                {
                    Value = x.ApiName,
                    Text = $"{x.FieldLabel} ({x.DataType})"
                });
            }
            model.AvailableFields.Insert(0, new SelectListItem()
            {
                Text = await _localizationService
                .GetResourceAsync("Admin.NopStation.ZohoCRM.MapShipmentItemFields.None"),
                Value = ""
            });

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> MapShipmentItemFields(MapShipmentItemFieldsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ZohoPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            _zohoCRMShipmentSettings.ItemShipmentField = model.ShipmentField;
            _zohoCRMShipmentSettings.ItemQuantityField = model.QuantityField;
            _zohoCRMShipmentSettings.ItemProductField = model.ProductField;

            await _settingService.SaveSettingAsync(_zohoCRMShipmentSettings);
            model.CloseWindow = true;

            return View(model);
        }

        #endregion

        #endregion
    }
}
