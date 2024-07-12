using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Opc.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.Opc.Areas.Admin.Controllers;

public class OpcController : NopStationAdminController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;

    #endregion

    #region Ctor

    public OpcController(IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IBaseAdminModelFactory baseAdminModelFactory)
    {
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _baseAdminModelFactory = baseAdminModelFactory;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(OpcPermissionProvider.ManageOpc))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var opcSettings = await _settingService.LoadSettingAsync<OpcSettings>(storeId);

        var model = opcSettings.ToSettingsModel<ConfigurationModel>();
        model.ActiveStoreScopeConfiguration = storeId;

        await _baseAdminModelFactory.PrepareCountriesAsync(model.AvailableCountries, defaultItemText: await _localizationService.GetResourceAsync("Admin.NopStation.Opc.Configuration.SelectCountry"));
        model.AvailableAddressFields = await OpcExtension.GetAddressPropertiesAsync();

        if (storeId > 0)
        {
            model.EnableOnePageCheckout_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.EnableOnePageCheckout, storeId);
            model.BypassShoppingCartPage_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.BypassShoppingCartPage, storeId);
            model.ShowShoppingCartInCheckout_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.ShowShoppingCartInCheckout, storeId);
            model.ShowDiscountBoxInCheckout_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.ShowDiscountBoxInCheckout, storeId);
            model.ShowGiftCardBoxInCheckout_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.ShowGiftCardBoxInCheckout, storeId);
            model.ShowCheckoutAttributesInCheckout_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.ShowCheckoutAttributesInCheckout, storeId);
            model.ShowOrderReviewDataInCheckout_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.ShowOrderReviewDataInCheckout, storeId);
            model.ShowEstimateShippingInCheckout_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.ShowEstimateShippingInCheckout, storeId);
            model.IsShoppingCartEditable_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.IsShoppingCartEditable, storeId);
            model.EnableBuyNowButton_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, settings => settings.EnableBuyNowButton, storeId);

            model.PreselectPreviousBillingAddress_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.PreselectPreviousBillingAddress, storeId);
            model.PreselectShipToSameAddress_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.PreselectShipToSameAddress, storeId);
            model.SaveBillingAddressOnChangeFields_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.SaveBillingAddressOnChangeFields, storeId);
            model.DefaultBillingAddressCountryId_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.DefaultBillingAddressCountryId, storeId);

            model.PreselectPreviousShippingAddress_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.PreselectPreviousShippingAddress, storeId);
            model.DefaultShippingAddressCountryId_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.DefaultShippingAddressCountryId, storeId);
            model.SaveShippingAddressOnChangeFields_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.SaveShippingAddressOnChangeFields, storeId);

            model.UpdateShippingMethodsOnChangeShippingAddressFields_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.UpdateShippingMethodsOnChangeShippingAddressFields, storeId);

            model.UpdatePaymentMethodsOnChangeBillingAddressFields_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.UpdatePaymentMethodsOnChangeBillingAddressFields, storeId);
            model.UpdatePaymentMethodsOnChangeShippingAddressFields_OverrideForStore = await _settingService.SettingExistsAsync(opcSettings, x => x.UpdatePaymentMethodsOnChangeShippingAddressFields, storeId);
        }

        return View(model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(OpcPermissionProvider.ManageOpc))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var opcSettings = await _settingService.LoadSettingAsync<OpcSettings>(storeScope);
        opcSettings = model.ToSettings(opcSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.EnableOnePageCheckout, model.EnableOnePageCheckout_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.BypassShoppingCartPage, model.BypassShoppingCartPage_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.ShowShoppingCartInCheckout, model.ShowShoppingCartInCheckout_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.ShowDiscountBoxInCheckout, model.ShowDiscountBoxInCheckout_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.ShowGiftCardBoxInCheckout, model.ShowGiftCardBoxInCheckout_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.ShowCheckoutAttributesInCheckout, model.ShowCheckoutAttributesInCheckout_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.ShowOrderReviewDataInCheckout, model.ShowOrderReviewDataInCheckout_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.ShowEstimateShippingInCheckout, model.ShowEstimateShippingInCheckout_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.IsShoppingCartEditable, model.IsShoppingCartEditable_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.EnableBuyNowButton, model.EnableBuyNowButton_OverrideForStore, storeScope, false);


        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.PreselectPreviousBillingAddress, model.PreselectPreviousBillingAddress_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.PreselectShipToSameAddress, model.PreselectShipToSameAddress_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.SaveBillingAddressOnChangeFields, model.SaveBillingAddressOnChangeFields_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.DefaultBillingAddressCountryId, model.DefaultBillingAddressCountryId_OverrideForStore, storeScope, false);

        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.PreselectPreviousShippingAddress, model.PreselectPreviousShippingAddress_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.DefaultShippingAddressCountryId, model.DefaultShippingAddressCountryId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.SaveShippingAddressOnChangeFields, model.SaveShippingAddressOnChangeFields_OverrideForStore, storeScope, false);

        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.UpdateShippingMethodsOnChangeShippingAddressFields, model.UpdateShippingMethodsOnChangeShippingAddressFields_OverrideForStore, storeScope, false);

        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.UpdatePaymentMethodsOnChangeBillingAddressFields, model.UpdatePaymentMethodsOnChangeBillingAddressFields_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(opcSettings, x => x.UpdatePaymentMethodsOnChangeShippingAddressFields, model.UpdatePaymentMethodsOnChangeShippingAddressFields_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        return RedirectToAction("Configure");
    }

    #endregion
}
