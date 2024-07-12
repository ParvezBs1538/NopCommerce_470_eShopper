using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core.Domain.Payments;
using Nop.Core.Events;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Payments;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/payment/[action]")]
public partial class PaymentApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICountryService _countryService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILocalizationService _localizationService;
    private readonly IPaymentModelFactory _paymentModelFactory;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly PaymentSettings _paymentSettings;

    #endregion

    #region Ctor

    public PaymentApiController(ICountryService countryService,
        IEventPublisher eventPublisher,
        ILocalizationService localizationService,
        IPaymentModelFactory paymentModelFactory,
        IPaymentPluginManager paymentPluginManager,
        IPermissionService permissionService,
        ISettingService settingService,
        PaymentSettings paymentSettings)
    {
        _countryService = countryService;
        _eventPublisher = eventPublisher;
        _localizationService = localizationService;
        _paymentModelFactory = paymentModelFactory;
        _paymentPluginManager = paymentPluginManager;
        _permissionService = permissionService;
        _settingService = settingService;
        _paymentSettings = paymentSettings;
    }

    #endregion

    #region Methods  

    public virtual async Task<IActionResult> Methods()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _paymentModelFactory.PreparePaymentMethodsModelAsync(new PaymentMethodsModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Methods([FromBody] BaseQueryModel<PaymentMethodSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _paymentModelFactory.PreparePaymentMethodListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MethodUpdate([FromBody] BaseQueryModel<PaymentMethodModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var pm = await _paymentPluginManager.LoadPluginBySystemNameAsync(model.SystemName);
        if (_paymentPluginManager.IsPluginActive(pm))
        {
            if (!model.IsActive)
            {
                //mark as disabled
                _paymentSettings.ActivePaymentMethodSystemNames.Remove(pm.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_paymentSettings);
            }
        }
        else
        {
            if (model.IsActive)
            {
                //mark as active
                _paymentSettings.ActivePaymentMethodSystemNames.Add(pm.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_paymentSettings);
            }
        }

        var pluginDescriptor = pm.PluginDescriptor;
        pluginDescriptor.FriendlyName = model.FriendlyName;
        pluginDescriptor.DisplayOrder = model.DisplayOrder;

        //update the description file
        pluginDescriptor.Save();

        //raise event
        await _eventPublisher.PublishAsync(new PluginUpdatedEvent(pluginDescriptor));

        return Ok(defaultMessage: true);
    }

    public virtual async Task<IActionResult> MethodRestrictions()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _paymentModelFactory.PreparePaymentMethodsModelAsync(new PaymentMethodsModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MethodRestrictionsSave([FromBody] BaseQueryModel<PaymentMethodsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var form = queryModel.FormValues.ToNameValueCollection();
        var paymentMethods = await _paymentPluginManager.LoadAllPluginsAsync();
        var countries = await _countryService.GetAllCountriesAsync(showHidden: true);

        foreach (var pm in paymentMethods)
        {
            var formKey = "restrict_" + pm.PluginDescriptor.SystemName;
            var countryIdsToRestrict = (!StringValues.IsNullOrEmpty(form[formKey])
                    ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                    : new List<string>())
                .Select(x => Convert.ToInt32(x)).ToList();

            var newCountryIds = new List<int>();
            foreach (var c in countries)
            {
                if (countryIdsToRestrict.Contains(c.Id))
                {
                    newCountryIds.Add(c.Id);
                }
            }

            await _paymentPluginManager.SaveRestrictedCountriesAsync(pm, newCountryIds);
        }

        return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Payment.MethodRestrictions.Updated"));
    }

    #endregion
}