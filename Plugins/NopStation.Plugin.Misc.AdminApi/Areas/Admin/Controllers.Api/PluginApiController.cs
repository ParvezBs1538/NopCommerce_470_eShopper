using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Tax;
using Nop.Services.Themes;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Plugins;
using Nop.Web.Areas.Admin.Models.Plugins.Marketplace;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/plugin/[action]")]
public partial class PluginApiController : BaseAdminApiController
{
    #region Fields

    private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
    private readonly IAuthenticationPluginManager _authenticationPluginManager;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILocalizationService _localizationService;
    private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
    private readonly IPermissionService _permissionService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IPickupPluginManager _pickupPluginManager;
    private readonly IPluginModelFactory _pluginModelFactory;
    private readonly IPluginService _pluginService;
    private readonly ISettingService _settingService;
    private readonly IShippingPluginManager _shippingPluginManager;
    private readonly IUploadService _uploadService;
    private readonly IWebHelper _webHelper;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly IWorkContext _workContext;
    private readonly MultiFactorAuthenticationSettings _multiFactorAuthenticationSettings;
    private readonly PaymentSettings _paymentSettings;
    private readonly ShippingSettings _shippingSettings;
    private readonly TaxSettings _taxSettings;
    private readonly WidgetSettings _widgetSettings;

    #endregion

    #region Ctor

    public PluginApiController(ExternalAuthenticationSettings externalAuthenticationSettings,
        IAuthenticationPluginManager authenticationPluginManager,
        ICustomerActivityService customerActivityService,
        IEventPublisher eventPublisher,
        ILocalizationService localizationService,
        IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
        IPermissionService permissionService,
        IPaymentPluginManager paymentPluginManager,
        IPickupPluginManager pickupPluginManager,
        IPluginModelFactory pluginModelFactory,
        IPluginService pluginService,
        ISettingService settingService,
        IShippingPluginManager shippingPluginManager,
        IUploadService uploadService,
        IWebHelper webHelper,
        IWidgetPluginManager widgetPluginManager,
        IWorkContext workContext,
        MultiFactorAuthenticationSettings multiFactorAuthenticationSettings,
        PaymentSettings paymentSettings,
        ShippingSettings shippingSettings,
        TaxSettings taxSettings,
        WidgetSettings widgetSettings)
    {
        _externalAuthenticationSettings = externalAuthenticationSettings;
        _authenticationPluginManager = authenticationPluginManager;
        _customerActivityService = customerActivityService;
        _eventPublisher = eventPublisher;
        _localizationService = localizationService;
        _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
        _permissionService = permissionService;
        _paymentPluginManager = paymentPluginManager;
        _pickupPluginManager = pickupPluginManager;
        _pluginModelFactory = pluginModelFactory;
        _pluginService = pluginService;
        _settingService = settingService;
        _shippingPluginManager = shippingPluginManager;
        _uploadService = uploadService;
        _webHelper = webHelper;
        _widgetPluginManager = widgetPluginManager;
        _workContext = workContext;
        _multiFactorAuthenticationSettings = multiFactorAuthenticationSettings;
        _paymentSettings = paymentSettings;
        _shippingSettings = shippingSettings;
        _taxSettings = taxSettings;
        _widgetSettings = widgetSettings;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AdminApiAccessDenied();

        var model = await _pluginModelFactory.PreparePluginSearchModelAsync(new PluginSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ListSelect([FromBody] BaseQueryModel<PluginSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _pluginModelFactory.PreparePluginListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> AdminNavigationPlugins()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return Json(new List<string>());

        //prepare models
        var models = await (await _pluginModelFactory.PrepareAdminNavigationPluginModelsAsync()).SelectAwait(async model => new
        {
            Title = model.FriendlyName,
            Link = model.ConfigurationUrl,
            Parent = await _localizationService.GetResourceAsync("Admin.Configuration.Plugins.Local"),
            GrandParent = string.Empty,
            Rate = -50 //negative rate is set to move plugins to the end of list
        }).ToListAsync();

        return Ok(new GenericResponseModel<object>()
        {
            Data = models
        });
    }

    [HttpPost]
    public virtual async Task<IActionResult> UploadPluginsAndThemes(IFormFile archivefile)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AdminApiAccessDenied();

        try
        {
            if (archivefile == null || archivefile.Length == 0)
                throw new NopException(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));

            var descriptors = await _uploadService.UploadPluginsAndThemesAsync(archivefile);
            var pluginDescriptors = descriptors.OfType<PluginDescriptor>().ToList();
            var themeDescriptors = descriptors.OfType<ThemeDescriptor>().ToList();

            //activity log
            foreach (var descriptor in pluginDescriptors)
            {
                await _customerActivityService.InsertActivityAsync("UploadNewPlugin",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.UploadNewPlugin"), descriptor.FriendlyName));
            }

            foreach (var descriptor in themeDescriptors)
            {
                await _customerActivityService.InsertActivityAsync("UploadNewTheme",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.UploadNewTheme"), descriptor.FriendlyName));
            }

            //events
            if (pluginDescriptors.Count != 0)
                await _eventPublisher.PublishAsync(new PluginsUploadedEvent(pluginDescriptors));

            if (themeDescriptors.Count != 0)
                await _eventPublisher.PublishAsync(new ThemesUploadedEvent(themeDescriptors));

            var message = string.Format(await _localizationService.GetResourceAsync("Admin.Configuration.Plugins.Uploaded"), pluginDescriptors.Count, themeDescriptors.Count);
            return Ok(message);
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> Install([FromBody] BaseQueryModel<string> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AdminApiAccessDenied();

        var form = queryModel.FormValues.ToNameValueCollection();
        try
        {
            //get plugin system name
            string systemName = null;
            foreach (string formValue in form.Keys)
                if (formValue.StartsWith("install-plugin-link-", StringComparison.InvariantCultureIgnoreCase))
                    systemName = formValue["install-plugin-link-".Length..];

            var pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(systemName, LoadPluginsMode.All);
            if (pluginDescriptor == null)
                //No plugin found with the specified id
                return NotFound("No plugin found with the specific system name.");

            //check whether plugin is not installed
            if (pluginDescriptor.Installed)
                return BadRequest();

            await _pluginService.PreparePluginToInstallAsync(pluginDescriptor.SystemName, await _workContext.GetCurrentCustomerAsync());
            pluginDescriptor.ShowInPluginsList = false;
            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Plugins.ChangesApplyAfterReboot"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> Uninstall([FromBody] BaseQueryModel<string> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AdminApiAccessDenied();

        var form = queryModel.FormValues.ToNameValueCollection();
        try
        {
            //get plugin system name
            string systemName = null;
            foreach (string formValue in form.Keys)
                if (formValue.StartsWith("uninstall-plugin-link-", StringComparison.InvariantCultureIgnoreCase))
                    systemName = formValue["uninstall-plugin-link-".Length..];

            var pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(systemName, LoadPluginsMode.All);
            if (pluginDescriptor == null)
                //No plugin found with the specified id
                return NotFound("No plugin found with the specific system name.");

            //check whether plugin is installed
            if (!pluginDescriptor.Installed)
                return BadRequest();

            await _pluginService.PreparePluginToUninstallAsync(pluginDescriptor.SystemName);
            pluginDescriptor.ShowInPluginsList = false;
            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Plugins.ChangesApplyAfterReboot"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete([FromBody] BaseQueryModel<string> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AdminApiAccessDenied();

        var form = queryModel.FormValues.ToNameValueCollection();
        try
        {
            //get plugin system name
            string systemName = null;
            foreach (string formValue in form.Keys)
                if (formValue.StartsWith("delete-plugin-link-", StringComparison.InvariantCultureIgnoreCase))
                    systemName = formValue["delete-plugin-link-".Length..];

            var pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(systemName, LoadPluginsMode.All);

            //check whether plugin is not installed
            if (pluginDescriptor.Installed)
                return BadRequest();

            await _pluginService.PreparePluginToDeleteAsync(pluginDescriptor.SystemName);
            pluginDescriptor.ShowInPluginsList = false;
            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Plugins.ChangesApplyAfterReboot"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ReloadList()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AdminApiAccessDenied();

        await _pluginService.UninstallPluginsAsync();
        await _pluginService.DeletePluginsAsync();

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> UninstallAndDeleteUnusedPlugins([FromBody] BaseQueryModel<string[]> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AdminApiAccessDenied();

        var names = queryModel.Data;
        foreach (var name in names)
            await _pluginService.PreparePluginToUninstallAsync(name);

        await _pluginService.UninstallPluginsAsync();

        foreach (var name in names)
            await _pluginService.PreparePluginToDeleteAsync(name);

        await _pluginService.DeletePluginsAsync();

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ApplyChanges()
    {
        return await ReloadList();
    }

    [HttpPost]
    public virtual async Task<IActionResult> DiscardChanges()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AdminApiAccessDenied();

        _pluginService.ResetChanges();

        return Ok(defaultMessage: true);
    }

    [HttpGet("{systemName}")]
    public virtual async Task<IActionResult> EditPopup(string systemName)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AdminApiAccessDenied();

        //try to get a plugin with the specified system name
        var pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(systemName, LoadPluginsMode.All);
        if (pluginDescriptor == null)
            return NotFound("No plugin found with the specific system name.");

        //prepare model
        var model = await _pluginModelFactory.PreparePluginModelAsync(null, pluginDescriptor);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditPopup([FromBody] BaseQueryModel<PluginModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a plugin with the specified system name
        var pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(model.SystemName, LoadPluginsMode.All);
        if (pluginDescriptor == null)
            return NotFound("No plugin found with the specific system name.");

        if (ModelState.IsValid)
        {
            ViewBag.RefreshPage = true;

            //we allow editing of 'friendly name', 'display order', store mappings
            pluginDescriptor.FriendlyName = model.FriendlyName;
            pluginDescriptor.DisplayOrder = model.DisplayOrder;
            pluginDescriptor.LimitedToStores.Clear();
            if (model.SelectedStoreIds.Any())
                pluginDescriptor.LimitedToStores = model.SelectedStoreIds;
            pluginDescriptor.LimitedToCustomerRoles.Clear();
            if (model.SelectedCustomerRoleIds.Any())
                pluginDescriptor.LimitedToCustomerRoles = model.SelectedCustomerRoleIds;

            //update the description file
            pluginDescriptor.Save();

            //raise event
            await _eventPublisher.PublishAsync(new PluginUpdatedEvent(pluginDescriptor));

            //locales
            var pluginInstance = pluginDescriptor.Instance<IPlugin>();
            foreach (var localized in model.Locales)
            {
                await _localizationService.SaveLocalizedFriendlyNameAsync(pluginInstance, localized.LanguageId, localized.FriendlyName);
            }

            //enabled/disabled
            if (!pluginDescriptor.Installed)
                return View(model);

            bool pluginIsActive;
            switch (pluginInstance)
            {
                case IPaymentMethod paymentMethod:
                    pluginIsActive = _paymentPluginManager.IsPluginActive(paymentMethod);
                    if (pluginIsActive && !model.IsEnabled)
                    {
                        //mark as disabled
                        _paymentSettings.ActivePaymentMethodSystemNames.Remove(pluginDescriptor.SystemName);
                        await _settingService.SaveSettingAsync(_paymentSettings);
                        break;
                    }

                    if (!pluginIsActive && model.IsEnabled)
                    {
                        //mark as enabled
                        _paymentSettings.ActivePaymentMethodSystemNames.Add(pluginDescriptor.SystemName);
                        await _settingService.SaveSettingAsync(_paymentSettings);
                    }

                    break;
                case IShippingRateComputationMethod shippingRateComputationMethod:
                    pluginIsActive = _shippingPluginManager.IsPluginActive(shippingRateComputationMethod);
                    if (pluginIsActive && !model.IsEnabled)
                    {
                        //mark as disabled
                        _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Remove(pluginDescriptor.SystemName);
                        await _settingService.SaveSettingAsync(_shippingSettings);
                        break;
                    }

                    if (!pluginIsActive && model.IsEnabled)
                    {
                        //mark as enabled
                        _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(pluginDescriptor.SystemName);
                        await _settingService.SaveSettingAsync(_shippingSettings);
                    }

                    break;
                case IPickupPointProvider pickupPointProvider:
                    pluginIsActive = _pickupPluginManager.IsPluginActive(pickupPointProvider);
                    if (pluginIsActive && !model.IsEnabled)
                    {
                        //mark as disabled
                        _shippingSettings.ActivePickupPointProviderSystemNames.Remove(pluginDescriptor.SystemName);
                        await _settingService.SaveSettingAsync(_shippingSettings);
                        break;
                    }

                    if (!pluginIsActive && model.IsEnabled)
                    {
                        //mark as enabled
                        _shippingSettings.ActivePickupPointProviderSystemNames.Add(pluginDescriptor.SystemName);
                        await _settingService.SaveSettingAsync(_shippingSettings);
                    }

                    break;
                case ITaxProvider taxProvider:
                    if (!model.IsEnabled)
                    {
                        //mark as disabled
                        _taxSettings.ActiveTaxProviderSystemName = string.Empty;
                        await _settingService.SaveSettingAsync(_taxSettings);
                        break;
                    }

                    //mark as enabled
                    _taxSettings.ActiveTaxProviderSystemName = model.SystemName;
                    await _settingService.SaveSettingAsync(_taxSettings);
                    break;
                case IExternalAuthenticationMethod externalAuthenticationMethod:
                    pluginIsActive = _authenticationPluginManager.IsPluginActive(externalAuthenticationMethod);
                    if (pluginIsActive && !model.IsEnabled)
                    {
                        //mark as disabled
                        _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Remove(pluginDescriptor.SystemName);
                        await _settingService.SaveSettingAsync(_externalAuthenticationSettings);
                        break;
                    }

                    if (!pluginIsActive && model.IsEnabled)
                    {
                        //mark as enabled
                        _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(pluginDescriptor.SystemName);
                        await _settingService.SaveSettingAsync(_externalAuthenticationSettings);
                    }

                    break;
                case IMultiFactorAuthenticationMethod multiFactorAuthenticationMethod:
                    pluginIsActive = _multiFactorAuthenticationPluginManager.IsPluginActive(multiFactorAuthenticationMethod);
                    if (pluginIsActive && !model.IsEnabled)
                    {
                        //mark as disabled
                        _multiFactorAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Remove(pluginDescriptor.SystemName);
                        await _settingService.SaveSettingAsync(_multiFactorAuthenticationSettings);
                        break;
                    }

                    if (!pluginIsActive && model.IsEnabled)
                    {
                        //mark as enabled
                        _multiFactorAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(pluginDescriptor.SystemName);
                        await _settingService.SaveSettingAsync(_multiFactorAuthenticationSettings);
                    }

                    break;
                case IWidgetPlugin widgetPlugin:
                    pluginIsActive = _widgetPluginManager.IsPluginActive(widgetPlugin);
                    if (pluginIsActive && !model.IsEnabled)
                    {
                        //mark as disabled
                        _widgetSettings.ActiveWidgetSystemNames.Remove(pluginDescriptor.SystemName);
                        await _settingService.SaveSettingAsync(_widgetSettings);
                        break;
                    }

                    if (!pluginIsActive && model.IsEnabled)
                    {
                        //mark as enabled
                        _widgetSettings.ActiveWidgetSystemNames.Add(pluginDescriptor.SystemName);
                        await _settingService.SaveSettingAsync(_widgetSettings);
                    }

                    break;
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("EditPlugin",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditPlugin"), pluginDescriptor.FriendlyName));

            return OkWrap(model);
        }

        //prepare model
        model = await _pluginModelFactory.PreparePluginModelAsync(model, pluginDescriptor, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    public virtual async Task<IActionResult> OfficialFeed()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _pluginModelFactory.PrepareOfficialFeedPluginSearchModelAsync(new OfficialFeedPluginSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> OfficialFeedSelect([FromBody] BaseQueryModel<OfficialFeedPluginSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _pluginModelFactory.PrepareOfficialFeedPluginListModelAsync(searchModel);

        return OkWrap(model);
    }

    #endregion
}