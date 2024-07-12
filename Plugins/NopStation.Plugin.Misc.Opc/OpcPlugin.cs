using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.Opc.Components;

namespace NopStation.Plugin.Misc.Opc;

public class OpcPlugin : BasePlugin, INopStationPlugin, IAdminMenuPlugin, IMiscPlugin, IWidgetPlugin
{
    #region Fields

    private readonly IWebHelper _webHelper;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly ISettingService _settingService;

    public bool HideInWidgetList => false;

    #endregion

    #region Ctor

    public OpcPlugin(IWebHelper webHelper,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        INopStationCoreService nopStationCoreService,
        ISettingService settingService)
    {
        _webHelper = webHelper;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _nopStationCoreService = nopStationCoreService;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/Opc/Configure";
    }

    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new OpcSettings
        {
            ShowCheckoutAttributesInCheckout = true,
            ShowDiscountBoxInCheckout = true,
            ShowEstimateShippingInCheckout = true,
            ShowGiftCardBoxInCheckout = true,
            ShowOrderReviewDataInCheckout = true,
            ShowShoppingCartInCheckout = true,
            BypassShoppingCartPage = true,
            EnableOnePageCheckout = true,
            UpdateShippingMethodsOnChangeShippingAddressFields = (await OpcExtension.GetAddressPropertiesAsync(true)).GetValues().ToList(),
            UpdatePaymentMethodsOnChangeBillingAddressFields = (await OpcExtension.GetAddressPropertiesAsync(true)).GetValues().ToList(),
            UpdatePaymentMethodsOnChangeShippingAddressFields = (await OpcExtension.GetAddressPropertiesAsync(true)).GetValues().ToList(),
            SaveBillingAddressOnChangeFields = (await OpcExtension.GetAddressPropertiesAsync(true)).GetValues().ToList(),
            SaveShippingAddressOnChangeFields = (await OpcExtension.GetAddressPropertiesAsync(true)).GetValues().ToList(),
            PreselectPreviousShippingAddress = true,
            PreselectPreviousBillingAddress = true,
            PreselectShipToSameAddress = true
        });

        await this.InstallPluginAsync(new OpcPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<OpcSettings>();

        await this.UninstallPluginAsync(new OpcPermissionProvider());
        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.Opc.Menu.Opc"),
            Visible = true,
            IconClass = "far fa-dot-circle",
        };

        if (await _permissionService.AuthorizeAsync(OpcPermissionProvider.ManageOpc))
        {
            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Opc.Menu.Configuration"),
                Url = "~/Admin/Opc/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "Opc.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);
        }

        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
        {
            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/opc-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=opc",
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
        var list = new Dictionary<string, string>()
        {
            ["Admin.NopStation.Opc.Menu.Opc"] = "Opc",
            ["Admin.NopStation.Opc.Menu.Configuration"] = "Configuration",

            ["Admin.NopStation.Opc.Configuration.Fields.EnableOnePageCheckout"] = "Enable one page checkout",
            ["Admin.NopStation.Opc.Configuration.Fields.EnableOnePageCheckout.Hint"] = "Check to .",
            ["Admin.NopStation.Opc.Configuration.Fields.EnableBuyNowButton"] = "Enable buy now button",
            ["Admin.NopStation.Opc.Configuration.Fields.EnableBuyNowButton.Hint"] = "By enabling it customer can directly go to checkout page after click",
            ["Admin.NopStation.Opc.Configuration.Fields.BypassShoppingCartPage"] = "Bypass shopping cart page",
            ["Admin.NopStation.Opc.Configuration.Fields.BypassShoppingCartPage.Hint"] = "Check to bypass shopping cart page.",
            ["Admin.NopStation.Opc.Configuration.Fields.ShowShoppingCartInCheckout"] = "Show shopping cart in checkout",
            ["Admin.NopStation.Opc.Configuration.Fields.ShowShoppingCartInCheckout.Hint"] = "Check to show shopping cart in checkout.",
            ["Admin.NopStation.Opc.Configuration.Fields.ShowDiscountBoxInCheckout"] = "Show discount box in checkout",
            ["Admin.NopStation.Opc.Configuration.Fields.ShowDiscountBoxInCheckout.Hint"] = "Check to show discount box in checkout.",
            ["Admin.NopStation.Opc.Configuration.Fields.ShowGiftCardBoxInCheckout"] = "Show gift card box in checkout",
            ["Admin.NopStation.Opc.Configuration.Fields.ShowGiftCardBoxInCheckout.Hint"] = "Check to show gift card box in checkout.",
            ["Admin.NopStation.Opc.Configuration.Fields.ShowCheckoutAttributesInCheckout"] = "Show checkout attributes in checkout",
            ["Admin.NopStation.Opc.Configuration.Fields.ShowCheckoutAttributesInCheckout.Hint"] = "Check to show checkout attributes in checkout.",
            ["Admin.NopStation.Opc.Configuration.Fields.ShowOrderReviewDataInCheckout"] = "Show order review data in checkout",
            ["Admin.NopStation.Opc.Configuration.Fields.ShowOrderReviewDataInCheckout.Hint"] = "Check to show order review data in checkout.",
            ["Admin.NopStation.Opc.Configuration.Fields.ShowEstimateShippingInCheckout"] = "Show estimate shipping in checkout",
            ["Admin.NopStation.Opc.Configuration.Fields.ShowEstimateShippingInCheckout.Hint"] = "Check to show estimate shipping in checkout.",
            ["Admin.NopStation.Opc.Configuration.Fields.IsShoppingCartEditable"] = "Allow edit for shoppingcart items",
            ["Admin.NopStation.Opc.Configuration.Fields.IsShoppingCartEditable.Hint"] = "Check to allow edit for shoppingcart items.",

            ["Admin.NopStation.Opc.Configuration.Fields.PreselectPreviousBillingAddress"] = "Pre-select previous billing address",
            ["Admin.NopStation.Opc.Configuration.Fields.PreselectPreviousBillingAddress.Hint"] = "Check to pre-select previous billing address.",
            ["Admin.NopStation.Opc.Configuration.Fields.PreselectShipToSameAddress"] = "Pre-select ship to same address",
            ["Admin.NopStation.Opc.Configuration.Fields.PreselectShipToSameAddress.Hint"] = "Check to pre-select ship to same address.",
            ["Admin.NopStation.Opc.Configuration.Fields.DefaultBillingAddressCountryId"] = "Default billing address country",
            ["Admin.NopStation.Opc.Configuration.Fields.DefaultBillingAddressCountryId.Hint"] = "Select default billing address country.",
            ["Admin.NopStation.Opc.Configuration.Fields.SaveBillingAddressOnChangeFields"] = "Save billing address on change fields",
            ["Admin.NopStation.Opc.Configuration.Fields.SaveBillingAddressOnChangeFields.Hint"] = "Select the fields in which the billing address will be saved depending on the change.",

            ["Admin.NopStation.Opc.Configuration.Fields.PreselectPreviousShippingAddress"] = "Pre-select previous shipping address",
            ["Admin.NopStation.Opc.Configuration.Fields.PreselectPreviousShippingAddress.Hint"] = "Check to pre-select previous shipping address.",
            ["Admin.NopStation.Opc.Configuration.Fields.DefaultShippingAddressCountryId"] = "Default shipping address country",
            ["Admin.NopStation.Opc.Configuration.Fields.DefaultShippingAddressCountryId.Hint"] = "Select default shipping address country.",
            ["Admin.NopStation.Opc.Configuration.Fields.SaveShippingAddressOnChangeFields"] = "Save shipping address on change fields",
            ["Admin.NopStation.Opc.Configuration.Fields.SaveShippingAddressOnChangeFields.Hint"] = "Select the fields in which the shipping address will be saved depending on the change.",

            ["Admin.NopStation.Opc.Configuration.Fields.UpdateShippingMethodsOnChangeShippingAddressFields"] = "Update shipping methods on change shipping address fields",
            ["Admin.NopStation.Opc.Configuration.Fields.UpdateShippingMethodsOnChangeShippingAddressFields.Hint"] = "Select the shipping address fields in which the shipping methods will be updated depending on the change.",

            ["Admin.NopStation.Opc.Configuration.Fields.UpdatePaymentMethodsOnChangeBillingAddressFields"] = "Update payment methods on change billing address fields",
            ["Admin.NopStation.Opc.Configuration.Fields.UpdatePaymentMethodsOnChangeBillingAddressFields.Hint"] = "Select the billing address fields in which the payment methods will be updated depending on the change.",
            ["Admin.NopStation.Opc.Configuration.Fields.UpdatePaymentMethodsOnChangeShippingAddressFields"] = "Update payment methods on change shipping address fields",
            ["Admin.NopStation.Opc.Configuration.Fields.UpdatePaymentMethodsOnChangeShippingAddressFields.Hint"] = "Select the shipping address fields in which the payment methods will be updated depending on the change.",

            ["Admin.NopStation.Opc.Configuration"] = "OPC settings",
            ["Admin.NopStation.Opc.Configuration.SelectCountry"] = "Select country",
            ["Admin.NopStation.Opc.Configuration.TabTitle.BillingAddress"] = "Billing address",
            ["Admin.NopStation.Opc.Configuration.TabTitle.ShippingAddress"] = "Shipping address",
            ["Admin.NopStation.Opc.Configuration.TabTitle.General"] = "General",
            ["Admin.NopStation.Opc.Configuration.TabTitle.ShippingMethod"] = "Shipping method",
            ["Admin.NopStation.Opc.Configuration.TabTitle.PaymentMethod"] = "Payment method",

            ["NopStation.Opc.Common.Error"] = "An error occured.",
            ["NopStation.Opc.Billing.NotSet"] = "Billing address is not set.",
            ["NopStation.Opc.Shipping.NotSet"] = "Shipping address is not set.",
            ["NopStation.Opc.ShoppingCart.Item.Error"] = "Items should be positive.",
            ["NopStation.Opc.ShoppingCart.Empty"] = "Your shopping cart is empty.",
            ["NopStation.Opc.ShoppingCart.EstimateShipping"] = "Estimate shipping",
            ["NopStation.Opc.ShoppingCart.EstimateShipping.Country"] = "Country",
            ["NopStation.Opc.ShoppingCart.EstimateShipping.StateProvince"] = "State / province",
            ["NopStation.Opc.ShoppingCart.EstimateShipping.ZipPostalCode"] = "Zip / postal code",
            ["NopStation.Opc.ShoppingCart.EstimateShipping.City"] = "City",
            ["NopStation.Opc.Checkout.Attribute"] = "Checkout attributes",
            ["NopStation.Opc.Buttons.BuyNow"] = "Buy Now",
        };

        return list.ToList();
    }
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.ProductBoxAddinfoAfter,
            PublicWidgetZones.ProductDetailsAddInfo,
            PublicWidgetZones.Footer
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == PublicWidgetZones.ProductBoxAddinfoAfter)
        {
            return typeof(ProductBoxBuyNowViewComponent);
        }
        if (widgetZone == PublicWidgetZones.ProductDetailsAddInfo)
        {
            return typeof(ProductDetailsBuyNowViewComponent);
        }
        return typeof(OpcFooterViewComponent);
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        var keyValuePairs = PluginResouces();
        foreach (var keyValuePair in keyValuePairs)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
        }
        await base.UpdateAsync(currentVersion, targetVersion);
    }
    #endregion
}