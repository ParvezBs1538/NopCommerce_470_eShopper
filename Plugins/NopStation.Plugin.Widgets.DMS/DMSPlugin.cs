using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Components;
using NopStation.Plugin.Widgets.DMS.Components;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS
{
    public class DMSPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IDMSService _dmsService;
        private readonly IScheduleTaskService _scheduleTaskService;

        public bool HideInWidgetList => false;

        #endregion

        #region Ctor

        public DMSPlugin(ICustomerService customerService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IWebHelper webHelper,
            ISettingService settingService,
            IRepository<CustomerRole> customerRoleRepository,
            IDMSService dmsService,
            IScheduleTaskService scheduleTaskService)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _webHelper = webHelper;
            _settingService = settingService;
            _customerRoleRepository = customerRoleRepository;
            _dmsService = dmsService;
            _scheduleTaskService = scheduleTaskService;
        }

        #endregion

        #region Utilites

        private static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/DMS/Configure";
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(DMSDefaults.DMSScheduleTaskType);
            if (scheduleTask == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
                {
                    Enabled = true,
                    Name = "Check online status (DMS)",
                    Seconds = 60 * 60,
                    Type = DMSDefaults.DMSScheduleTaskType
                });
            }

            var keyValuePairs = PluginResouces();
            foreach (var keyValuePair in keyValuePairs)
            {
                await _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
            }
        }
        public override async Task InstallAsync()
        {
            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(DMSDefaults.DMSScheduleTaskType);
            if (scheduleTask == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
                {
                    Enabled = true,
                    Name = "Check online status (DMS)",
                    Seconds = 60 * 60,
                    Type = DMSDefaults.DMSScheduleTaskType
                });
            }
            var dmsSettings = new DMSSettings()
            {
                CheckIat = false,
                EnableJwtSecurity = false,
                ShipmentPageSize = 10,
                TokenKey = RandomString(20),
                TokenSecondsValid = 300,
                TokenSecret = RandomString(35),
                PageSizeOptions = "10,20,30,50",
                DefaultPackagingSlipPaperSizeId = (int)PaperSizeEnum.A4,
                PrintPackagingSlipLandscape = false,
                PrintProductsOnPackagingSlip = true,
                PrintWeightInfoOnPackagingSlip = false,
                PrintEachPackagingSlipInNewPage = true,
                ProofOfDeliveryImageMaxSize = 5,
                ProofOfDeliveryTypeId = (int)ProofOfDeliveryTypes.CustomersSignature,
                OtpLength = 4
            };
            await _settingService.SaveSettingAsync(dmsSettings);

            var customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(DMSDefaults.ShipperCustomerRoleName);
            if (customerRole == null)
            {
                customerRole = new CustomerRole()
                {
                    Active = true,
                    SystemName = DMSDefaults.ShipperCustomerRoleName,
                    Name = "Shippers",
                    IsSystemRole = false
                };
                await _customerService.InsertCustomerRoleAsync(customerRole);
            }

            await this.InstallPluginAsync(new DMSPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(DMSDefaults.DMSScheduleTaskType);
            if (scheduleTask != null)
                await _scheduleTaskService.DeleteTaskAsync(scheduleTask);

            var customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(DMSDefaults.ShipperCustomerRoleName);
            if (customerRole != null)
                await _customerRoleRepository.DeleteAsync(customerRole);

            await this.UninstallPluginAsync(new DMSPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.DMS.Menu.DMS"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageConfiguration))
            {
                var configurationItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DMS.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/DMS/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "DMS.Configuration"
                };
                menuItem.ChildNodes.Add(configurationItem);
            }

            if (await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageCourierShipment))
            {
                var courierShipmentItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DMS.Menu.CourierShipments"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/CourierShipment/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "DMS.CourierShipment"
                };
                menuItem.ChildNodes.Add(courierShipmentItem);
            }

            if (await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageShipper))
            {
                var shipperItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DMS.Menu.Shippers"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/Shipper/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "DMS.Shipper"
                };
                menuItem.ChildNodes.Add(shipperItem);
            }

            if (await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageShipmentPickupPoint))
            {
                var shipmentPickupPointItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DMS.Menu.ShipmentPickupPoint"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ShipmentPickupPoint/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "DMS.ShipmentPickupPoint"
                };
                menuItem.ChildNodes.Add(shipmentPickupPointItem);
            }

            if (await _permissionService.AuthorizeAsync(DMSPermissionProvider.ManageDevice))
            {
                var device = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ShipperDevice/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DMS.Menu.Devices"),
                    SystemName = "DMS.ShipperDevice"
                };
                menuItem.ChildNodes.Add(device);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/dms-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=dms",
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
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration", "DMS settings"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Menu.DMS", "DMS"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Menu.Shippers", "Shippers"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Menu.CourierShipments", "Courier shipments"),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.BlockTitle.Common", "Common"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.BlockTitle.Security", "Security"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.NST.CopyButton", "Copy"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.NST", "NST"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.NST.Copied", "NST has been copied."),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.EditShippersDetails", "Edit Shippers Details"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Edit", "Shippers Edit"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.BackToList", "Back To Shippers List"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Edit.Info", "Shipper Info"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Edit.LastLocation", "Shipper Last Location"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Edit.AssignedPackage", "Shipper Assigned Package"),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.EditShippersDetails", "Edit Shippers Details"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Edit", "Shippers Edit"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.BackToList", "Back To Shippers List"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Edit.Info", "Shipper Info"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Edit.LastLocation", "Shipper Last Location"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Edit.AssignedPackage", "Shipper Assigned Package"),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.GoogleMapApiKey", "Google Map Api Key"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.GoogleMapApiKey.Hint", "give Api Key"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.GeoMapId", "Map"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.GeoMapId.Hint", "Which map use to track live location."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.EnableSignatureUpload", "Enable signature upload"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.EnableSignatureUpload.Hint", "Enable customer signature upload for shipments."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.SignatureUploadRequired", "Signature upload required"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.SignatureUploadRequired.Hint", "Check to signature upload required for shipments to be marked as delivered."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.ShipmentPageSize", "Shipment page size"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.ShipmentPageSize.Hint", "Shipment page size in public side shipment list page."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.UseAjaxLoading", "Use ajax loading"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.UseAjaxLoading.Hint", "Use ajax loading in public side shipment list page."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.AllowShippersToSelectPageSize", "Allow shippers to select page size"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.AllowShippersToSelectPageSize.Hint", "Whether shippers are allowed to select the page size from a predefined list of options."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.PageSizeOptions", "Page size options"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.PageSizeOptions.Hint", "Comma separated list of page size options (e.g. 10, 5, 15, 20). First option is the default page size if none are selected."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.NST", "NST"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.NST.Hint", "Current NST, based on your security settings."),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.EnableJwtSecurity", "Enable JWT security"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.EnableJwtSecurity.Hint", "Check to enable JWT security. It will require 'DMS-NST' header for every api request."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.TokenKey", "Token key"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.TokenKey.Hint", "The JSON web token security key (payload: NST-KEY)."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.TokenSecret", "Token secret"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.TokenSecret.Hint", "512 bit JSON web token secret."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.CheckIat", "Check IAT"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.CheckIat.Hint", "Click to check issued at time."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.TokenSecondsValid", "JSON web token. Seconds valid"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.TokenSecondsValid.Hint", "Enter number of seconds for valid JSON web token."),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Fields.Customer", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Fields.Customer.Hint", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Fields.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Fields.Active.Hint", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Fields.CreatedOn.Hint", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Fields.LastLocation", "Last Location"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Fields.LastLocation.Hint", "Last Location"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Fields.UpdatedOn", "Updated On"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Fields.UpdatedOn.Hint", "Last update time."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Fields.Online", "Online"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.Fields.Online.Hint", "Cutomer online status use to track live location."),


                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.List", "Shippers"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.List.SearchEmail", "Email"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.List.SearchEmail.Hint", "Search shipper by customer email."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Shippers.CustomerRoleMissing", "Shipper (system name: 'Shippers') customer role is missing."),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.List.SearchShipperId.All", "All"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.List", "Courier shipments"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.List.SearchShipperId", "Shipper"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.List.SearchOrderId", "Order Id#"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.List.SearchShipperId.Hint", "Search by shipper."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.List.SearchOrderId.Hint", "Search by order id."),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Details", "Courier shipment details"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.Shipper.NotFound", "Shipper not found."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.Shipper", "Shipper"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.Shipper.Required", "Shipper required"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.Signature", "Signature"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.UpdatedOn", "Updated on"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.UpdatedOn.Hint", "Last updated on."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.CreatedOn.Hint", "Courier shipment created on."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.Shipper.Hint", "The shipper for this shipment."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.Signature.Hint", "The customer signature for this shipment."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.Delivered", "Delivered"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Shipment.Delivered", "Shipment already delivered"),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.PODPhotoUrl", "Proof of Delivery"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.PODPhotoUrl.Hint", "The proof of delivery of this shipment."),

                new KeyValuePair<string, string>("NopStation.DMS.Account.Shipments", "Shipments"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.Shipments.NoShipments", "No Shipments"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.Shipments.ShipmentNumber", "Shipment#"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.Shipments.ShippingStatus", "Shipping Status"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.Shipments.UpdatedOn", "Updated On"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.Shipments.CustomOrderNumber", "Order#"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.Shipments.ShipmentDetails", "Details"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.ShipmentDetails", "Shipment Details"),

                new KeyValuePair<string, string>("NopStation.DMS.Shipper.InvalidAccount", "Shipper account is not valid."),
                new KeyValuePair<string, string>("NopStation.DMS.Shipper.InactiveAccount", "Shipper account is not active."),
                new KeyValuePair<string, string>("NopStation.DMS.Shipper.PickupTime.Default", "Order not Shipped."),
                new KeyValuePair<string, string>("NopStation.DMS.Response.InvalidJwtToken", "Invalid Jwt token."),

                new KeyValuePair<string, string>("NopStation.DMS.Shipments.MarkedAsShipped", "Shipmen marked as shipped."),
                new KeyValuePair<string, string>("NopStation.DMS.Shipments.MarkedAsDelivered", "Shipment marked as delivered."),
                new KeyValuePair<string, string>("NopStation.DMS.Shipments.SignatureUploaded", "Signature uploaded successfully."),

                new KeyValuePair<string, string>("NopStation.DMS.Shipments.List.ShippingStatusId.All", "All"),
                new KeyValuePair<string, string>("NopStation.DMS.Shipments.List.ShippingStatusId", "Shipping status"),
                new KeyValuePair<string, string>("NopStation.DMS.Shipments.List.ShipmentPageSize", "Page size"),
                new KeyValuePair<string, string>("NopStation.DMS.Shipments.List.TrackingNumber", "Tracking number"),
                new KeyValuePair<string, string>("NopStation.DMS.Shipments.List.Email", "Email"),
                new KeyValuePair<string, string>("NopStation.DMS.Shipments.Period", "Period"),

                new KeyValuePair<string, string>("NopStation.DMS.Shipments.ShippedDate", "Shipped date"),
                new KeyValuePair<string, string>("NopStation.DMS.Shipments.DeliveryDate", "Delivery date"),
                new KeyValuePair<string, string>("NopStation.DMS.Shipments.CanShip", "Can ship"),
                new KeyValuePair<string, string>("NopStation.DMS.Shipments.CanDeliver", "Can deliver"),

                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.DMS.Models.FilterOption.All", "All"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.DMS.Models.FilterOption.LastWeek", "Last week"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.DMS.Models.FilterOption.LastMonth", "Last month"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.DMS.Models.FilterOption.LastYear", "Last year"),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentStatus", "Courier shipment status"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentStatus.Hint", "The status of the courier shipment"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentStatus.ShipmentStatusTypes.0", "Assigned To Shipper"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentStatus.ShipmentStatusTypes.1", "Received By Shipper"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentStatus.ShipmentStatusTypes.2", "Delivered"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentStatus.ShipmentStatusTypes.3", "Delivery Failed"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.CourierShipment.NotFound", "Courier shipment not found"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.AlreadyReceivedByShipper", "Shipper already received the courier"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.ShippingNotPossible", "Shipping not possible."),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.ShipperNotFound", "Shipper error. Shipper not found"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.WrongShipper", "Wrong shipper. Already assigned to another person"),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.BlockTitle.PackagingSlip", "Packaging slip"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.BlockTitle.PackagingSlip.Hint", "Packaging slip"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.DefaultPackagingSlipPaperSizeId", "Default packaging slip paper size"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.DefaultPackagingSlipPaperSizeId.Hint", "Default packaging slip paper size."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.PrintPackagingSlipLandscape", "Print packaging slip landscape"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.PrintPackagingSlipLandscape.Hint", "Print packaging slip landscape."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.PrintProductsOnPackagingSlip", "Print products on packaging slip"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.PrintProductsOnPackagingSlip.Hint", "Print products on packaging slip."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.PrintOrderTotalOnPackagingSlip", "Print ordertotal on packagingslip"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.PrintOrderTotalOnPackagingSlip.Hint", "Print ordertotal on packagingslip."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.PrintWeightInfoOnPackagingSlip", "Print weight info on packaging slip"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.PrintWeightInfoOnPackagingSlip.Hint", "Print weight info on packaging slip."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.PrintEachPackagingSlipInNewPage", "Print each packaging slip in new page"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.PrintEachPackagingSlipInNewPage.Hint", "Print each packaging slip in new page."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.LocationUpdateIntervalInSeconds", "Location update interval in seconds"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.LocationUpdateIntervalInSeconds.Hint", "Location update interval in seconds."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.BlockTitle.ProofOfDelivery", "Proof of delivery"),
                new KeyValuePair<string, string>("NopStation.DMS.PDFPackagingSlip.TrackingNumber",  "Tracking number: {0}"),

                new KeyValuePair<string, string>("NopStation.DMS.PDFPackagingSlip.WeightInfo", "Weight of the Package"),
                new KeyValuePair<string, string>("NopStation.DMS.PDFPackagingSlip.Address", "Pickup point:"),
                new KeyValuePair<string, string>("NopStation.DMS.PDFPackagingSlip.Order", "Order"),
                new KeyValuePair<string, string>("NopStation.DMS.PDFPackagingSlip.Shipment", "Shipment"),

                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.DeliveryFailed.CustomerNote", "Shipper tried to deliver the product on: {0}. But the delivery"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.Status", "Courier shipment status"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.Shipments.CourierShipmentStatus", "Courier shipment status"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDelivery.NotActive", "Proof of delivery disabled"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpRequest.NotAuthorized", "Shipper is not authorized"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpRequest.ShipmentNotFound", "Otp not found for shipment #"),

                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.Otp.OrderNote", "Otp for shipment #"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpRequest.OtpGenerated", "Otp generated successfully"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpRequest.FailedToGenerateOtp", "Failed to generated otp"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpValidation.ValidationFailed", "Invalid opt"),

                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.AlreadyReceived", "You have already received the shipment"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.Shipment.NotFound", "Shipment not found"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentRecieveByShipper.MarkedAsReceived", "Marked as received"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentDeliveryFailed.CourierShipment.NotFound", "Courier shipment not found"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentDeliveryFailed.Shipment.NotFound", "Shipment not found"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentDeliveryFailed.AlreadyMarked", "Already marked as delivery failed"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentDeliveryFailed.ShippingNotPossible", "Not possible to mark as failed"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentDeliveryFailed.ShipperNotFound", "Shipper not found"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateShipmentDeliveryFailed.WrongShipper", "Wrong shipper. Marked as delivery failed is not possible."),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.DeliveryFailed.CustomerOrderNote", "Shipment tried to deliver. But delivery is failed"),

                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.ProofOfDeliveryDisabled", "Proof of delivery is disabled"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.ProofOfDeliveryType.OtpType", "Wrong validation type. Otp validation needed."),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.InvalidData", "Invalid data."),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.ProofOfDeliveryType.WrongProofOfDeliveryType", "Proof of delivery type is not correct."),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.CourierShipment.NotFound", "CourierShipment not found"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.ShippingNotPossible", "Shipping not possible"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.AlreadyMarkedAsDelivered", "Already marked as delivered"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.ShipmentNotFound", "Shipment not found"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.NotPossibleToMark", "Not possible to mark as deliver."),

                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ShipperNotFound", "Shipper not found"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.UpdateProofOfDeliveryReferenceData.WrongShipper", "Wrong shipper"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDeliveryDisabled", "Proof of delivery is disabled"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpValidation.NotOtpType", "Validation is not OTP type"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpValidation.OtpMissing", "OTP missing"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.NotFound", "Courier shipment not found"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpValidation.ShippingNotPossible", "Shipment is not possible."),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.WrongShipper", "Wrong shipper"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpValidation.OtpNotFound", "Otp not found. Request for OTP again"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDelivery.OtpValidation.AlreadyValidated", "OTP validation failed. Already marked as verified."),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Id", "Id"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Name.Required", "Pickup point name required"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Name.Hint", "Name of the pickup point of the shipment."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.OpeningHours", "Opening hours"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.OpeningHours.Hint", "Opening hours."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.DisplayOrder.Hint", "Pickup point display order."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Description", "Description"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Description.Hint", "Description."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Latitude", "Latitude"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Latitude.Hint", "Latitude."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Longitude", "Longitude"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Longitude.Hint", "Longitude."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Active", "Is active"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Fields.Active.Hint", "Is active."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.List", "Shipment pickup points"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Menu.ShipmentPickupPoint", "Shipment pickup points"),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.EnabledProofOfDelivery", "Enable proof of delivery"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.EnabledProofOfDelivery.Hint", "Enable or disable proof of delivery."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.ProofOfDeliveryType", "Proof of delivery type"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.ProofOfDeliveryType.Hint", "Select proof of delivery type."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.ProofOfDeliveryRequired", "Proof of delivery required"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.ProofOfDeliveryRequired.Hint", "Proof of delivery data store is required or not."),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.OtpLength", "Otp length"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.OtpLength.Hint", "Length of the otp. By default it will be 4."),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Create.Successful", "Pickup point created successfully"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Create.Failed", "Failed to create"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.PickupPointNotFound", "Pickup point not found"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.Edit.Successful", "Pickup point edited successfully"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.ShipmentPickupPoint.BackToList", "Back to pickup point list"),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.Devices.BackToList", "Back to shipper device list"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Devices.Fields.Customer", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Devices.Fields.DeviceType", "Device type"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Devices.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Devices.Fields.DeviceToken", "Device token"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Devices.Fields.SubscriptionId", "Subscription id"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Devices.Fields.IsRegistered", "Is registered"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Devices.Fields.UpdatedOn", "Updated on"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Devices.ViewDetails", "View shipper device details"),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.Menu.Devices", "Shipper device"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Devices.List", "Shipper device list"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Devices.DeleteSelected", "Delete selected"),
                new KeyValuePair<string, string>("NopStation.DMS.Response.InvalidDevice", "The device is not valid. Please try with login again."),
                new KeyValuePair<string, string>("NopStation.DMS.Response.InvalidToken", "The token is not valid"),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.AllowCustomersToDeleteAccount", "Allow customers to delete account"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.AllowCustomersToDeleteAccount.Hint", "This settings allow customers to delete account permanently as per european union's new data privacy law (GDPR)."),

                new KeyValuePair<string, string>("NopStation.DMS.Account.AdminAccountShouldExists.DeleteAdministrator", "Can't delete the last administrator. At least one administrator account should exists"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.Deleted", "Customer deleted successfully"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.deleteaccount", "Delete account"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.deleteaccountdialogtitle","Confirm account deletion"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.deleteaccountdialogbody", "This action can not be revoked.\nAre you sure?"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.deleteaccountpassworddialogtitle", "Password"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.deleteaccountpassworddialogbody", "Enter your password"),
                new KeyValuePair<string, string>("NopStation.DMS.common.cancel", "Cancel"),

                new KeyValuePair<string, string>("NopStation.DMS.ShipperDevice.Header.DeviceId.NotFound", "Device id is not present on the headers"),
                new KeyValuePair<string, string>("NopStation.DMS.ShipperDevice.InvalidDeviceId", "Invalid deviceId. Please try with clearing the cache and app data"),
                new KeyValuePair<string, string>("NopStation.DMS.Account.DeviceNotRegistered", "Shipper device is not registered"),
                new KeyValuePair<string, string>("NopStation.DMS.ShipperDevice.NotValid", "Invalid device. Please try with login again"),
                new KeyValuePair<string, string>("NopStation.DMS.ShipperDevice.NotRegistered", "Shipper don't have registered device. Please try with login again."),
                new KeyValuePair<string, string>("NopStation.DMS.Shipments.Notes", "Note (s)"),
                new KeyValuePair<string, string>("NopStation.DMS.Shipments.Notes.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("NopStation.DMS.Shipments.Notes.Note", "Note"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.SelectPickupPoint", "Select pickup point"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentPickupPoint", "Shipment pickup point"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentPickupPoint.Hint", "Select the shipment pickup point. You need setup DMS pickup point first"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentPickupPoint.Required", "Shipment pickup point required. Please select a Shipment pickup point"),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.SaveCourierShipment.Shipper.NotFound", "Shipper not found"),

                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDelivery.MaximumUploadedFileSize", "Maximum image size is {0} megabyte"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.ProofOfDeliveryImageMaxSize", "POD Image Max Size(MB)"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.Configuration.Fields.ProofOfDeliveryImageMaxSize.Hint", "Proof of delivery image max size in megabyte"),
                new KeyValuePair<string, string>("nopstation.dms.shipments.markedasdeliveryfailed", "Dervivery failed"),
                new KeyValuePair<string, string>("dms.common.nodatafound", "No data found"),
                new KeyValuePair<string, string>("dms.scanner.pagetitle", "Scan Qr code"),
                new KeyValuePair<string, string>("dms.common.name", "Name"),
                new KeyValuePair<string, string>("dms.common.email", "Email"),
                new KeyValuePair<string, string>("NopStation.DMS.CourierShipment.ProofOfDelivery.Otp.CustomerOtp", "Otp for the shipment #{0} : {1}"),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.ShipmentId", "Shipment"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.Fields.TrackingNumber", "Tracking Number"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.List.SearchCustomOrderNumber", "Custom Order Number"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.List.SearchCustomOrderNumber.Hint", "Custom Order Number"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.List.SearchShipmentId", "Shipment Id"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.List.SearchShipmentId.Hint", "Shipment Id"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.List.SearchShipmentId.Hint", "Shipment Id"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.List.SearchShipmentTrackingNumber", "Tracking Number"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.CourierShipments.List.SearchShipmentTrackingNumber.Hint", "Shipment tracking number"),

                new KeyValuePair<string, string>("Admin.NopStation.DMS.PackagingSlips.PDF.EntryName", "Payment_slip"),
                new KeyValuePair<string, string>("Admin.NopStation.DMS.PackagingSlipsSource.Fields.ShipppingMethod", "Shipping method:"),

                new KeyValuePair<string, string>("dms.pickedupat", "Picked up at"),
                new KeyValuePair<string, string>("dms.deliveryaddress", "Delivery address"),
                new KeyValuePair<string, string>("dms.deliveredon", "Delivered on"),
                new KeyValuePair<string, string>("dms.delivered.at", "Delivered at"),
                new KeyValuePair<string, string>("dms.total.orders", "Total orders"),
                new KeyValuePair<string, string>("dms.total.assigned", "Total assigned"),
                new KeyValuePair<string, string>("dms.total.picked", "Total picked"),
                new KeyValuePair<string, string>("dms.alreadydelivered", "Already delivered"),
                new KeyValuePair<string, string>("dms.faileddelivery", "Failed delivery"),
                new KeyValuePair<string, string>("dms.dashboard", "Dashboard"),
                new KeyValuePair<string, string>("dms.password.old", "Old password"),
                new KeyValuePair<string, string>("dms.password.New", "New password"),
                new KeyValuePair<string, string>("dms.password.Confirm", "Confirm password"),
                new KeyValuePair<string, string>("dms.orderno", "Order no"),
                new KeyValuePair<string, string>("dms.customerorderno", "Customer Order no"),
                new KeyValuePair<string, string>("dms.gender.male", "Male"),
                new KeyValuePair<string, string>("dms.gender.female", "Female")
            };

            var appStringsResources = _dmsService.LoadAppStringResourcesAsync().Result;
            list.AddRange(appStringsResources);

            return list;
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                DMSDefaults.ShipmentDetailsBottom,
                PublicWidgetZones.AccountNavigationAfter,
                DMSDefaults.SHIPMENT_PAGE_BODY_MIDDLE
            });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == DMSDefaults.ShipmentDetailsBottom)
                return typeof(CourierShipmentViewComponent);
            else if (widgetZone == DMSDefaults.SHIPMENT_PAGE_BODY_MIDDLE)
                return typeof(CustomerShipmentNoteTableViewComponent);

            return typeof(DMSAccountNavigationViewComponent);
        }

        #endregion
    }
}
