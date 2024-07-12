using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Shipping.VendorAndState.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Services.Vendors;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Components;

namespace NopStation.Plugin.Shipping.VendorAndState
{
    public class VendorAndStateComputationMethod : BasePlugin, INopStationPlugin, IAdminMenuPlugin, 
        IShippingRateComputationMethod, IWidgetPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IVendorService _vendorService;
        private readonly IShippingService _shippingService;
        private readonly IVendorShippingService _vendorShippingService;
        private readonly IVendorStateProvinceShippingService _vendorStateProvinceShippingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly VendorAndStateSettings _shippingByVendorSettings;
        private readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public VendorAndStateComputationMethod(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IVendorService vendorService,
            IShippingService shippingService,
            IVendorShippingService vendorShippingService,
            IVendorStateProvinceShippingService vendorStateProvinceShippingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            VendorAndStateSettings shippingByVendorSettings,
            WidgetSettings widgetSettings)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _vendorService = vendorService;
            _shippingService = shippingService;
            _vendorShippingService = vendorShippingService;
            _vendorStateProvinceShippingService = vendorStateProvinceShippingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shippingByVendorSettings = shippingByVendorSettings;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Utilities

        protected async Task<decimal> GetShippingRateAsync(IList<ShoppingCartItem> cart, decimal shippingCharge,
            bool enableFreeShippingOverAmountX, bool withDiscounts, decimal amountX)
        {
            var rate = shippingCharge;
            if (enableFreeShippingOverAmountX)
            {
                (var _, var _, var subTotalWithoutDiscount, var subTotalWithDiscount, var _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, false);
                var subTotal = withDiscounts ? subTotalWithDiscount : subTotalWithoutDiscount;
                if (amountX <= subTotal)
                    rate = 0;
            }

            return rate;
        }

        protected async Task<VendorPackage> GetDefaultVendorShippingDetailsAsync(IList<ShoppingCartItem> cart)
        {
            var package = new VendorPackage()
            {
                AdditionalShippingCharge = await cart.SumAwaitAsync(async shoppingCartItem => await _shippingService.GetAdditionalShippingChargeAsync(shoppingCartItem)),
                Cart = cart,
                Rate = await GetShippingRateAsync(cart, _shippingByVendorSettings.ShippingCharge, _shippingByVendorSettings.EnableFreeShippingOverAmountX,
                    _shippingByVendorSettings.WithDiscounts, _shippingByVendorSettings.AmountX),
                TransitDays = _shippingByVendorSettings.TransitDays,
                Vendor = null
            };

            return package;
        }

        #endregion

        public bool HideInWidgetList => true;

        public Task<decimal?> GetFixedRateAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            return Task.FromResult<decimal?>(null);
        }

        public async Task<GetShippingOptionResponse> GetShippingOptionsAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (!_shippingByVendorSettings.EnablePlugin)
                return new GetShippingOptionResponse();

            var cartItems = getShippingOptionRequest.Items.Select(x => new { ShoppingCartItem = x.ShoppingCartItem, Product = x.Product }).ToList();
            var stateProvinceId = getShippingOptionRequest.ShippingAddress.StateProvinceId ?? 0;
            var vendors = await _vendorService.GetVendorsByProductIdsAsync(getShippingOptionRequest.Items.Select(p => p.Product.Id).ToArray());
            var shippingMethods = await _shippingService.GetAllShippingMethodsAsync(getShippingOptionRequest.ShippingAddress.CountryId ?? 0);

            var response = new GetShippingOptionResponse();

            foreach (var shippingMethod in shippingMethods)
            {
                var vendorRates = new Dictionary<int, VendorPackage>();

                var hideShippingMethod = false;
                foreach (var vendor in vendors)
                {
                    var cart = cartItems.Where(x => x.Product.VendorId == vendor.Id).Select(x => x.ShoppingCartItem).ToList();

                    var stateShipping = await _vendorStateProvinceShippingService.GetVendorStateProvinceShippingByVendorIdAndShippingMethodIdAsync(
                        vendor.Id, shippingMethod.Id, stateProvinceId);

                    if (stateShipping != null)
                    {
                        if (stateShipping.HideShippingMethod)
                        {
                            hideShippingMethod = true;
                            break;
                        }

                        vendorRates.Add(vendor.Id, new VendorPackage()
                        {
                            Rate = await GetShippingRateAsync(cart, stateShipping.ShippingCharge, stateShipping.EnableFreeShippingOverAmountX, 
                                stateShipping.WithDiscounts, stateShipping.AmountX),
                            TransitDays = stateShipping.TransitDays,
                            Cart = cart,
                            Vendor = vendor,
                            AdditionalShippingCharge = await cart.SumAwaitAsync(async shoppingCartItem => await _shippingService.GetAdditionalShippingChargeAsync(shoppingCartItem))
                        });
                    }
                    else
                    {
                        var vendorShipping = await _vendorShippingService.GetVendorShippingByVendorIdAndShippingMethodIdAsync(vendor.Id, shippingMethod.Id);

                        if (vendorShipping != null)
                        {
                            if (vendorShipping.HideShippingMethod)
                            {
                                hideShippingMethod = true;
                                break;
                            }

                            vendorRates.Add(vendor.Id, new VendorPackage()
                            {
                                Rate = await GetShippingRateAsync(cart, vendorShipping.DefaultShippingCharge, vendorShipping.EnableFreeShippingOverAmountX, 
                                    vendorShipping.WithDiscounts, vendorShipping.AmountX),
                                TransitDays = vendorShipping.TransitDays,
                                Cart = cart,
                                Vendor = vendor,
                                AdditionalShippingCharge = await cart.SumAwaitAsync(async shoppingCartItem => await _shippingService.GetAdditionalShippingChargeAsync(shoppingCartItem))
                            });
                        }
                        else
                            vendorRates.Add(vendor.Id, await GetDefaultVendorShippingDetailsAsync(cart));
                    }
                }

                if (!hideShippingMethod)
                {
                    var otherCart = cartItems.Where(x => !vendors.Select(y => y.Id).Contains(x.Product.VendorId)).Select(x => x.ShoppingCartItem).ToList();
                    if (otherCart.Any())
                        vendorRates.Add(0, await GetDefaultVendorShippingDetailsAsync(otherCart));

                    response.ShippingOptions.Add(new ShippingOption
                    {
                        IsPickupInStore = false,
                        Name = shippingMethod.Name,
                        Rate = vendorRates.Sum(x => x.Value.Rate),
                        ShippingRateComputationMethodSystemName = "NopStation.Plugin.Shipping.VendorAndState",
                        TransitDays = vendorRates.Max(x => x.Value.TransitDays)
                    });
                }
            }

            return response;
        }

        public override async Task InstallAsync()
        {
            //settings
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _settingService.SaveSettingAsync(new VendorAndStateSettings
            {
                EnablePlugin = true
            });

            await this.InstallPluginAsync(new VendorAndStatePermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await this.UninstallPluginAsync(new VendorAndStatePermissionProvider());
            await base.UninstallAsync();
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await base.UpdateAsync(currentVersion, targetVersion);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.ShippingByVendor.Menu.ShippingByVendor"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(VendorAndStatePermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.ShippingByVendor.Menu.Configuration"),
                    Url = "~/Admin/ShippingByVendor/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ShippingByVendor.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);

                var shippingItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.ShippingByVendor.Menu.ShippingCharges"),
                    Url = "~/Admin/ShippingByVendor/ShippingCharges",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ShippingByVendor.ShippingCharges"
                };
                menuItem.ChildNodes.Add(shippingItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/shipping-by-vendor-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=shipping-by-vendor",
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
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Menu.ShippingByVendor", "Shipping by vendor"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Menu.ShippingCharges", "Shipping charges"),

                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Configuration.Fields.ShippingCharge", "Shipping charge"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Configuration.Fields.EnableFreeShippingOverAmountX", "Enable free shipping over amount X"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Configuration.Fields.AmountX", "Amount X"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Configuration.Fields.WithDiscounts", "With discounts"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Configuration.Fields.TransitDays", "Transit days"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Configuration.Fields.ShippingCharge.Hint", "Define default shipping charge."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Configuration.Fields.EnableFreeShippingOverAmountX.Hint", "Enable free shipping over amount X."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Configuration.Fields.AmountX.Hint", "Define amount X."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Configuration.Fields.WithDiscounts.Hint", "Determines whether 'X' discounted amount or not."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Configuration.Fields.TransitDays.Hint", "Transit days."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Configuration", "Shipping by vendor settings"),

                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.Vendor", "Vendor"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.ShippingMethod", "Shipping method"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.ShippingMethod.Hint", "Select shipping method."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.HideShippingMethod", "Hide shipping method"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.HideShippingMethod.Hint", "Determines whether the shipping method will be hidden or not."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.SellerSideDelivery", "Seller side delivery"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.SellerSideDelivery.Hint", "Determines whether vendor will deliver from their side or not."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.DefaultShippingCharge", "Shipping charge"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.DefaultShippingCharge.Hint", "Define default shipping charge."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.EnableFreeShippingOverAmountX", "Enable free shipping over amount X"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.EnableFreeShippingOverAmountX.Hint", "Enable free shipping over amount X."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.AmountX", "Amount X"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.AmountX.Hint", "Define amount X."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.WithDiscounts", "With discounts"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.WithDiscounts.Hint", "Determines whether 'X' discounted amount or not."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.TransitDays", "Transit days"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.TransitDays.Hint", "Transit days."),

                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.Vendor", "Vendor"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.StateProvince", "State province"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.ShippingMethod", "Shipping method"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.HideShippingMethod", "Hide shipping method"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.SellerSideDelivery", "Seller side delivery"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.ShippingCharge", "Shipping charge"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.EnableFreeShippingOverAmountX", "Enable free shipping over amount X"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.AmountX", "Amount X"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.WithDiscounts", "With discounts"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.TransitDays", "Transit days"),

                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Vendor.Reset", "Reset"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Vendor.SaveShippingInfo", "Save shipping info"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Vendor.StateProviceShipping", "Shipping charge (state/provice wise)"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Vendor.Tab.Shippings", "Shippings"),

                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.List", "Vendor shippings"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.List.SearchVendor", "Vendor"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.List.SearchVendor.Hint", "Search by vendor."),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.List.SearchShippingMethod", "Shipping method"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.VendorShippings.List.SearchShippingMethod.Hint", "Search by shipping method."),

                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.Vendors.All", "All"),
                new KeyValuePair<string, string>("Admin.NopStation.ShippingByVendor.ShippingMethods.All", "All"),
            };

            return list;
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                AdminWidgetZones.VendorDetailsBlock
            });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(VendorShippingViewComponent);
        }

        public Task<IShipmentTracker> GetShipmentTrackerAsync()
        {
            return Task.FromResult<IShipmentTracker>(null);
        }

        public class VendorPackage
        {
            public decimal Rate { get; set; }

            public decimal AdditionalShippingCharge { get; set; }

            public Vendor Vendor { get; set; }

            public int? TransitDays { get; set; }

            public IList<ShoppingCartItem> Cart { get; set; }
        }
    }
}
