using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.Shipping.Redx.Infrastructure;
using NopStation.Plugin.Shipping.Redx.Services;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Cms;
using Nop.Web.Framework.Infrastructure;
using Nop.Core.Domain.Cms;
using Nop.Services.Configuration;
using System;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Components;

namespace NopStation.Plugin.Shipping.Redx
{
    public class RedxComputationMethod : BasePlugin, IShippingRateComputationMethod, IAdminMenuPlugin, 
        INopStationPlugin, IWidgetPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly RedxSettings _redxSettings;
        private readonly IRedxShipmentService _redxShipmentService;
        private readonly WidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;

        public bool HideInWidgetList => true;

        #region Ctor

        public RedxComputationMethod(IWebHelper webHelper,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            RedxSettings redxSettings,
            IRedxShipmentService redxShipmentService,
            WidgetSettings widgetSettings,
            ISettingService settingService)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _redxSettings = redxSettings;
            _redxShipmentService = redxShipmentService;
            _widgetSettings = widgetSettings;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Redx.Menu.Redx"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(RedxShipmentPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Redx.Menu.Configuration"),
                    Url = "~/Admin/Redx/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Redx.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);

                var areaList = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Redx.Menu.AreaList"),
                    Url = "~/Admin/RedxArea/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Redx.AreaList"
                };
                menuItem.ChildNodes.Add(areaList);

                var shipmentList = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Redx.Menu.ShipmentList"),
                    Url = "~/Admin/RedxShipment/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Redx.ShipmentList"
                };
                menuItem.ChildNodes.Add(shipmentList);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/redx-shipping-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=redx-shipping",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public override async Task InstallAsync()
        {
            var settings = new RedxSettings
            {
                ParcelTrackUrl = "https://redx.com.bd/track-global-parcel/",
                SandboxUrl = "https://sandbox.redx.com.bd/v1.0.0-beta",
                FlatShippingCharge = 60,
                UseSandbox = false,
                BaseUrl = "https://openapi.redx.com.bd/v1.0.0-beta/"
            };
            await _settingService.SaveSettingAsync(settings);

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
            { 
                _widgetSettings.ActiveWidgetSystemNames.Add(PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await this.InstallPluginAsync(new RedxShipmentPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await this.UninstallPluginAsync(new RedxShipmentPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>();

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Menu.Configuration", "Configuration"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Menu.AreaList", "Area list"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Menu.ShipmentList", "Shipment list"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Menu.Redx", "REDX"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration", "REDX settings"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.FlatShippingCharge", "Flat shipping charge"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.FlatShippingCharge.Hint", "Flat shipping charge for REDX shipment."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.ApiAccessToken", "Api access token"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.ApiAccessToken.Hint", "REDX api access token."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.ParcelTrackUrl", "Parcel track url"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.ParcelTrackUrl.Hint", "REDX parcel track api url."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.ShipmentEventsUrl", "Shipment event url"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.ShipmentEventsUrl.Hint", "Enter REDX shipment events api url."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.UseSandbox", "Use sandbox"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.UseSandbox.Hint", "Check to mark as sandbox mode."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.SandboxUrl", "Sandbox url"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.SandboxUrl.Hint", "Enter REDX sandbox api url."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.BaseUrl", "Base url"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.Configuration.Fields.BaseUrl.Hint", "REDX api base url."));;

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.Fields.Name", "Area name"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.Fields.Name.Hint", "Enter REDX shipping area name."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.Fields.PostCode", "Post code"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.Fields.PostCode.Hint", "Enter REDX shipping area post code."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.Fields.DistrictName", "District name"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.Fields.DistrictName.Hint", "Enter REDX shipping district name."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.Fields.StateProvince", "State province"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.Fields.StateProvince.Hint", "Select state province for this area."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.Fields.AreaId", "Area id"));
                                                                             
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.List.SearchDisctrictName", "Disctrict name"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.List.SearchDisctrictName.Hint", "Search by disctrict name."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.List.SearchStateProvince", "State province"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.List.SearchStateProvince.Hint", "Search by state province."));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.Fields.Order", "Order#"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.Fields.Shipment", "Shipment#"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.Fields.CustomOrderNumber", "Custom order number"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.Fields.OrderStatus", "Order status"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.Fields.Shipment", "Shipment#"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.Fields.TrackingId", "Tracking id"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.Fields.TrackingId.Hint", "The REDX tracking id."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.Fields.RedxArea", "Redx area"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.Fields.RedxArea.Hint", "The REDX shipping area."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.Fields.LabelUrl", "Label url"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.Fields.LabelUrl.Hint", "The REDX shipment label url."));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.List.SearchOrderId", "Order id"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.List.SearchOrderId.Hint", "Search REDX shipment by order id."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.List.SearchTrackingId", "Tracking id"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.List.SearchTrackingId.Hint", "Search REDX shipment by tracking id."));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxArea.Updated", "REDX area has been updated successfully."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.CancelOrder", "Order is cancelled."));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.EditArea", "Edit area details"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.BackToList", "back to area list"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.List", "REDX areas"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxAreas.SyncAreas", "Sync areas"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.List", "REDX shipments"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.ParcelSubmit", "Submit REDX parcel"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.ConfirmRedxShipment", "Are you sure want to create REDX shipment?"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipment.CreateFailed", "Failed to create REDX shipment."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Redx.RedxShipments.Info", "REDX shipment info"));

            return list;
        }

        public async Task<GetShippingOptionResponse> GetShippingOptionsAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            var response = new GetShippingOptionResponse();

            response.ShippingOptions.Add(new ShippingOption
            {
                Rate = _redxSettings.FlatShippingCharge,
                Name = await _localizationService.GetResourceAsync("NopStation.Redx.ShippingName"),
                Description = await _localizationService.GetResourceAsync("NopStation.Redx.ShippingDescription")
            });
            return response;
        }

        public Task<decimal?> GetFixedRateAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            return Task.FromResult<decimal?>(_redxSettings.FlatShippingCharge);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/Redx/Configure";
        }

        public Task<IShipmentTracker> GetShipmentTrackerAsync()
        {
            return Task.FromResult<IShipmentTracker>(new RedxShipmentTracker(_redxSettings, _redxShipmentService));
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.OrderShipmentDetailsButtons });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(RedxViewComponent);
        }

        #endregion
    }
}