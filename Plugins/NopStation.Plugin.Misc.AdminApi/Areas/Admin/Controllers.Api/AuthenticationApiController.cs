using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.ExternalAuthentication;
using Nop.Web.Areas.Admin.Models.MultiFactorAuthentication;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/authentication/[action]")]
public partial class AuthenticationApiController : BaseAdminApiController
{
    #region Fields

    private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
    private readonly IAuthenticationPluginManager _authenticationPluginManager;
    private readonly IEventPublisher _eventPublisher;
    private readonly IExternalAuthenticationMethodModelFactory _externalAuthenticationMethodModelFactory;
    private readonly IMultiFactorAuthenticationMethodModelFactory _multiFactorAuthenticationMethodModelFactory;
    private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly MultiFactorAuthenticationSettings _multiFactorAuthenticationSettings;

    #endregion

    #region Ctor

    public AuthenticationApiController(ExternalAuthenticationSettings externalAuthenticationSettings,
        IAuthenticationPluginManager authenticationPluginManager,
        IEventPublisher eventPublisher,
        IExternalAuthenticationMethodModelFactory externalAuthenticationMethodModelFactory,
        IMultiFactorAuthenticationMethodModelFactory multiFactorAuthenticationMethodModelFactory,
        IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
        IPermissionService permissionService,
        ISettingService settingService,
        MultiFactorAuthenticationSettings multiFactorAuthenticationSettings)
    {
        _externalAuthenticationSettings = externalAuthenticationSettings;
        _authenticationPluginManager = authenticationPluginManager;
        _eventPublisher = eventPublisher;
        _externalAuthenticationMethodModelFactory = externalAuthenticationMethodModelFactory;
        _multiFactorAuthenticationMethodModelFactory = multiFactorAuthenticationMethodModelFactory;
        _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
        _permissionService = permissionService;
        _settingService = settingService;
        _multiFactorAuthenticationSettings = multiFactorAuthenticationSettings;
    }

    #endregion

    #region External Authentication

    public virtual async Task<IActionResult> ExternalMethods()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageExternalAuthenticationMethods))
            return AdminApiAccessDenied();

        //prepare model
        var model = _externalAuthenticationMethodModelFactory
            .PrepareExternalAuthenticationMethodSearchModel(new ExternalAuthenticationMethodSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExternalMethods([FromBody] BaseQueryModel<ExternalAuthenticationMethodSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageExternalAuthenticationMethods))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _externalAuthenticationMethodModelFactory.PrepareExternalAuthenticationMethodListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExternalMethodUpdate([FromBody] BaseQueryModel<ExternalAuthenticationMethodModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageExternalAuthenticationMethods))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var method = await _authenticationPluginManager.LoadPluginBySystemNameAsync(model.SystemName);
        if (_authenticationPluginManager.IsPluginActive(method))
        {
            if (!model.IsActive)
            {
                //mark as disabled
                _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Remove(method.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_externalAuthenticationSettings);
            }
        }
        else
        {
            if (model.IsActive)
            {
                //mark as active
                _externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(method.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_externalAuthenticationSettings);
            }
        }

        var pluginDescriptor = method.PluginDescriptor;
        pluginDescriptor.DisplayOrder = model.DisplayOrder;

        //update the description file
        pluginDescriptor.Save();

        //raise event
        await _eventPublisher.PublishAsync(new PluginUpdatedEvent(pluginDescriptor));

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Multi-factor Authentication

    public virtual async Task<IActionResult> MultiFactorMethods()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMultifactorAuthenticationMethods))
            return AdminApiAccessDenied();

        //prepare model
        var model = _multiFactorAuthenticationMethodModelFactory
            .PrepareMultiFactorAuthenticationMethodSearchModel(new MultiFactorAuthenticationMethodSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MultiFactorMethods([FromBody] BaseQueryModel<MultiFactorAuthenticationMethodSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMultifactorAuthenticationMethods))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _multiFactorAuthenticationMethodModelFactory.PrepareMultiFactorAuthenticationMethodListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MultiFactorMethodUpdate([FromBody] BaseQueryModel<MultiFactorAuthenticationMethodModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMultifactorAuthenticationMethods))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var method = await _multiFactorAuthenticationPluginManager.LoadPluginBySystemNameAsync(model.SystemName);
        if (_multiFactorAuthenticationPluginManager.IsPluginActive(method))
        {
            if (!model.IsActive)
            {
                //mark as disabled
                _multiFactorAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Remove(method.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_multiFactorAuthenticationSettings);
            }
        }
        else
        {
            if (model.IsActive)
            {
                //mark as active
                _multiFactorAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(method.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_multiFactorAuthenticationSettings);
            }
        }

        var pluginDescriptor = method.PluginDescriptor;
        pluginDescriptor.DisplayOrder = model.DisplayOrder;

        //update the description file
        pluginDescriptor.Save();

        //raise event
        await _eventPublisher.PublishAsync(new PluginUpdatedEvent(pluginDescriptor));

        return Ok(defaultMessage: true);
    }

    #endregion
}