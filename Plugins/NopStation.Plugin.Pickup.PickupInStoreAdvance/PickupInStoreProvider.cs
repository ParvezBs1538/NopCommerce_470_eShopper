using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Shipping.Tracking;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Components;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Components;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Services;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance
{
    public class PickupInStoreProvider : BasePlugin, IPickupPointProvider, IAdminMenuPlugin, IWidgetPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStorePickupPointService _storePickupPointService;
        private readonly IWebHelper _webHelper;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public PickupInStoreProvider(IAddressService addressService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStorePickupPointService storePickupPointService,
            IWebHelper webHelper,
            IMessageTemplateService messageTemplateService,
            IEmailAccountService emailAccountService,
            ISettingService settingService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService)
        {
            _addressService = addressService;
            _countryService = countryService;
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storePickupPointService = storePickupPointService;
            _webHelper = webHelper;
            _messageTemplateService = messageTemplateService;
            _emailAccountService = emailAccountService;
            _settingService = settingService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        #region Properties

        public bool HideInWidgetList => false;

        #endregion

        #region Utilities

        protected async Task AddEmailTemplateAsync()
        {
            var template = (await _messageTemplateService.GetMessageTemplatesByNameAsync(PickupInStoreDefaults.PICKUP_READY_EMAIL_TEMPLATE_SYSTEM_NAME, (await _storeContext.GetCurrentStoreAsync()).Id)).FirstOrDefault();
            if (template != null)
                return;
            var emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync())?.FirstOrDefault();
            template = new MessageTemplate
            {
                Name = PickupInStoreDefaults.PICKUP_READY_EMAIL_TEMPLATE_SYSTEM_NAME,
                Subject = "%Store.Name%. Order #%Order.OrderNumber% is ready to pickup",
                Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Thanks for buying from <a href=\"%Store.URL%\">%Store.Name%</a>. Order #%Order.OrderNumber% has been ready to deliver. Below is the summary of the order.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress2%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress2%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% %Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = emailAccount != null ? emailAccount.Id : 0
            };
            await _messageTemplateService.InsertMessageTemplateAsync(template);
        }

        #endregion

        #region Methods

        public async Task<GetPickupPointsResponse> GetPickupPointsAsync(IList<ShoppingCartItem> cart, Address address)
        {
            var result = new GetPickupPointsResponse();
            var allPoints = await _storePickupPointService.GetAllStorePickupPointsAsync((await _storeContext.GetCurrentStoreAsync()).Id);
            foreach (var point in allPoints)
            {
                if (!point.Active)
                    continue;
                var pointAddress = await _addressService.GetAddressByIdAsync(point.AddressId);
                if (pointAddress == null)
                    continue;

                result.PickupPoints.Add(new PickupPoint
                {
                    Id = point.Id.ToString(),
                    Name = point.Name,
                    Description = point.Description,
                    Address = pointAddress.Address1,
                    City = pointAddress.City,
                    County = pointAddress.County,
                    StateAbbreviation = (await _stateProvinceService.GetStateProvinceByAddressAsync(pointAddress))?.Abbreviation ?? string.Empty,
                    CountryCode = (await _countryService.GetCountryByAddressAsync(pointAddress))?.TwoLetterIsoCode ?? string.Empty,
                    ZipPostalCode = pointAddress.ZipPostalCode,
                    OpeningHours = point.OpeningHours,
                    PickupFee = point.PickupFee,
                    DisplayOrder = point.DisplayOrder,
                    ProviderSystemName = PluginDescriptor.SystemName,
                    Latitude = point.Latitude,
                    Longitude = point.Longitude,
                    TransitDays = point.TransitDays
                });
            }

            if (!result.PickupPoints.Any())
                result.AddError(await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.NoPickupPoints"));

            return result;
        }

        public Task<IShipmentTracker> GetShipmentTrackerAsync()
        {
            return Task.FromResult<IShipmentTracker>(null);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PickupInStoreAdvance/Configure";
        }

        public override async Task InstallAsync()
        {
            //sample pickup point
            var country = await _countryService.GetCountryByThreeLetterIsoCodeAsync("USA");
            var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync("NY", country?.Id);

            var address = new Address
            {
                Address1 = "21 West 52nd Street",
                City = "New York",
                CountryId = country?.Id,
                StateProvinceId = state?.Id,
                ZipPostalCode = "10021",
                CreatedOnUtc = DateTime.UtcNow
            };
            await _addressService.InsertAddressAsync(address);

            var pickupPoint = new StorePickupPoint
            {
                Name = "New York store",
                AddressId = address.Id,
                Active = true,
                OpeningHours = "10.00 - 19.00",
                PickupFee = 1.99m
            };
            await _storePickupPointService.InsertStorePickupPointAsync(pickupPoint);

            //Add email template
            await AddEmailTemplateAsync();

            var settings = new PickupInStoreSettings
            {
                AddOrderNote = true,
                NotifyCustomerIfReady = true,
                OrderNotesShowToCustomer = false,
                ShowOrderStatusInOrderDetails = true
            };
            await _settingService.SaveSettingAsync(settings);
            await this.InstallPluginAsync(new PickupInStorePermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<PickupInStoreSettings>();
            await this.UninstallPluginAsync(new PickupInStorePermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var pickupInStoreOrders = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Orders"),
                Url = "/Admin/PickupInStoreOrder/orders",
                Visible = true,
                IconClass = "far fa-dot-circle",
                SystemName = "PickupInStoreAdvance.Orders"
            };
            var salesNode = rootNode.ChildNodes.Where(x => x.Title.Equals("Sales")).FirstOrDefault();
            if (salesNode != null)
            {
                salesNode.ChildNodes.Add(pickupInStoreOrders);
            }

            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Menu.PickupInStore"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Menu.Configuration"),
                    Url = "/Admin/PickupInStoreAdvance/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "PickupInStoreAdvance.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
                var pickupInStoreOrders2 = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Orders"),
                    Url = "/Admin/PickupInStoreOrder/orders",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Plugin.PickupInStoreAdvance.Orders"
                };
                menuItem.ChildNodes.Add(pickupInStoreOrders2);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.Menu.Documentation"),
                Url = "https://www.nop-station.com/pickup-in-store-advance-documentation",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.OrderDetailsButtons, PublicWidgetZones.OrderDetailsPageOverview });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == AdminWidgetZones.OrderDetailsButtons)
                return typeof(MarkReadyOrPickedViewComponent);
            
            return typeof(PickupStatusViewComponent);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new("Admin.NopStation.PickupInStoreAdvance.AddNew", "Add a new pickup point"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Description", "Description"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Description.Hint", "Specify a description of the pickup point."),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.DisplayOrder", "Display order"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.DisplayOrder.Hint", "Specify the pickup point display order."),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Latitude", "Latitude"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Latitude.Hint", "Specify a latitude (DD.dddddddd°)."),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Latitude.InvalidPrecision", "Precision should be less then 8"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Latitude.InvalidRange", "Latitude should be in range -90 to 90"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Latitude.IsNullWhenLongitudeHasValue", "Latitude and Longitude should be specify together"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Longitude", "Longitude"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Longitude.Hint", "Specify a longitude (DD.dddddddd°)."),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Longitude.InvalidPrecision", "Precision should be less then 8"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Longitude.InvalidRange", "Longitude should be in range -180 to 180"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Longitude.IsNullWhenLatitudeHasValue", "Latitude and Longitude should be specify together"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Name", "Name"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Name.Hint", "Specify a name of the pickup point."),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.OpeningHours", "Opening hours"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.OpeningHours.Hint", "Specify opening hours of the pickup point (Monday - Friday: 09:00 - 19:00 for example)."),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.PickupFee", "Pickup fee"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.PickupFee.Hint", "Specify a fee for the shipping to the pickup point."),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Store", "Store"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Store.Hint", "A store name for which this pickup point will be available."),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.TransitDays", "Transit days"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.TransitDays.Hint", "The number of days of delivery of the goods to pickup point."),
                new("Admin.NopStation.PickupInStoreAdvance.NoPickupPoints", "No pickup points are available"),
                new("Admin.NopStation.PickupInStoreAdvance.Orders", "Pickup In Store Orders"),
                new("Admin.NopStation.PickupInStoreAdvance.Orders.List", "Pickup In Store Order List"),
                new("Admin.NopStation.PickupInStoreAdvance.Field.OrderId", "Order Id"),
                new("Admin.NopStation.PickupInStoreAdvance.Field.OrderStatus", "Order Pickup Status"),
                new("Admin.NopStation.PickupInStoreAdvance.Field.OrderInitied", "Order Initied"),
                new("Admin.NopStation.PickupInStoreAdvance.Field.ReadyForPick", "Ready For Pick"),
                new("Admin.NopStation.PickupInStoreAdvance.Field.PickedUpByCustomer", "Picked Up By Customer"),
                new("Admin.NopStation.PickupInStoreAdvance.Field.ReadyForPickupMarkedAtUtc", "Ready From"),
                new("Admin.NopStation.PickupInStoreAdvance.Field.PickupUpAtUtc", "Picked up at"),
                new("Admin.NopStation.PickupInStoreAdvance.Status.1", "Order Initiated"),
                new("Admin.NopStation.PickupInStoreAdvance.Status.2", "Ready For Pickup"),
                new("Admin.NopStation.PickupInStoreAdvance.Status.3", "Picked Up By Customer"),
                new("Admin.NopStation.PickupInStoreAdvance.Status.4", "Order Cancelled"),
                new("Admin.NopStation.PickupInStoreAdvance.Field.OrderDate", "Order Date"),
                new("Admin.NopStation.PickupInStoreAdvance.Field.NopOrderStatus", "Nop Order Status"),
                new("Admin.NopStation.PickupInStoreAdvance.Field.MarkAsReady", "Mark As Ready"),
                new("Admin.NopStation.PickupInStoreAdvance.Field.MarkAsPickedUp", "Mark As Picked Up"),
                new("Admin.NopStation.PickupInStoreAdvance.Shipment.AdminComment", "Order is ready to pickup"),
                new("Admin.NopStation.PickupInStoreAdvance.SearchOrder.OrderId", "Order Id"),
                new("Admin.NopStation.PickupInStoreAdvance.SearchOrder.OrderId.Hint", "A Order Id"),
                new("Admin.NopStation.PickupInStoreAdvance.SearchOrder.SearchStatusId", "Pickup Status"),
                new("Admin.NopStation.PickupInStoreAdvance.SearchOrder.SearchStatusId.Hint", "Select a pickup status"),
                new("Admin.NopStation.PickupInStoreAdvance.Orders.Title", "Pickup In Store Orders"),
                new("Admin.NopStation.PickupInStoreAdvance.Orders.PickupStatus.Success", "Pickup status change successful"),
                new("Admin.NopStation.PickupInStoreAdvance.Orders.PickupStatus.UnSuccess", "Pickup status change unsuccessful"),
                new("Admin.NopStation.PickupInStoreAdvance.SearchModel.All", "All"),
                new("Admin.NopStation.PickupInStoreAdvance.Settings.ShowOrderStatusInOrderDetails", "Pickup status in order details"),
                new("Admin.NopStation.PickupInStoreAdvance.Settings.ShowOrderStatusInOrderDetails.Hint", "Show pickup status in order details page"),
                new("Admin.NopStation.PickupInStoreAdvance.Settings.AddOrderNote", "Add order notes"),
                new("Admin.NopStation.PickupInStoreAdvance.Settings.AddOrderNote.Hint", "Add order note when order is mark as ready to pickup"),
                new("Admin.NopStation.PickupInStoreAdvance.Settings.OrderNotesShowToCustomer", "Show order notes to customer"),
                new("Admin.NopStation.PickupInStoreAdvance.Settings.OrderNotesShowToCustomer.Hint", "Order notes visible to customer or not"),
                new("Admin.NopStation.PickupInStoreAdvance.Settings.NotifyCustomerIfReady", "Notify customer if ready"),
                new("Admin.NopStation.PickupInStoreAdvance.Settings.NotifyCustomerIfReady.Hint", "Notify customer that the order is ready to pickup"),
                new("Admin.NopStation.PickupInStoreAdvance.Configure.Settings.Title", "Pickup in store order settings"),
                new("Admin.NopStation.PickupInStoreAdvance.Configure.PickupPoints.Title", "Pickup points"),
                new("Admin.NopStation.PickupInStoreAdvance.Configure.PageTitle", "Configure"),
                new("Admin.NopStation.PickupInStoreAdvance.Status.Processing", "Processing"),
                new("Admin.NopStation.PickupInStoreAdvance.Status.ReadyForPick", "Ready for pickup"),
                new("Admin.NopStation.PickupInStoreAdvance.Status.PickedUpByCustomer", "Delivered"),
                new("Admin.NopStation.PickupInStoreAdvance.Template.Edit", "Edit Template"),
                new("Admin.NopStation.PickupInStoreAdvance.Template.Edit.Hint", "Edit notify user email template"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Active", "Active"),
                new("Admin.NopStation.PickupInStoreAdvance.Fields.Active.Hint", "Active or inactive"),
                new("Admin.NopStation.PickupInStoreAdvance.ImportPickupPoints", "Import Pickup Points"),
                new("Admin.NopStation.PickupInStoreAdvance.ExportPickupPoints", "Export Pickup Points"),
                new("Admin.NopStation.PickupInStoreAdvance.ImportPickupPoints.Success", "Pickup points imported successfully"),
                new("Admin.NopStation.PickupInStoreAdvance.Menu.PickupInStore", "Pickup In Store Advance"),
                new("Admin.NopStation.PickupInStoreAdvance.Menu.Configuration", "Configuration"),
                new("Admin.NopStation.PickupInStoreAdvance.Menu.Documentation", "Documentation"),
                new("Admin.Contentmanagement.Messagetemplates.Description.PickupinstoreAdvance.Pickupready", "This message template is used to notify customer when order is ready."),
                new("NopStation.PickupInStoreAdvance.Order.ReadyTime", "Ready from"),
                new("NopStation.PickupInStoreAdvance.Order.DeliveryTime", "Deliverd on"),
                new("NopStation.PickupInStoreAdvance.Status.PickupStatus", "Pickup Status")
            };

            return list;
        }

        #endregion
    }
}