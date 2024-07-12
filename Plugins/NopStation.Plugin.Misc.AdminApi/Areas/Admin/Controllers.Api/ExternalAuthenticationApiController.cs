using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.ExternalAuthentication;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/externalauthentication/[action]")]
public partial class ExternalAuthenticationApiController : BaseAdminApiController
{
    #region Fields

    private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
    private readonly IAuthenticationPluginManager _authenticationPluginManager;
    private readonly IEventPublisher _eventPublisher;
    private readonly IExternalAuthenticationMethodModelFactory _externalAuthenticationMethodModelFactory;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public ExternalAuthenticationApiController(ExternalAuthenticationSettings externalAuthenticationSettings,
        IAuthenticationPluginManager authenticationPluginManager,
        IEventPublisher eventPublisher,
        IExternalAuthenticationMethodModelFactory externalAuthenticationMethodModelFactory,
        IPermissionService permissionService,
        ISettingService settingService)
    {
        _externalAuthenticationSettings = externalAuthenticationSettings;
        _authenticationPluginManager = authenticationPluginManager;
        _eventPublisher = eventPublisher;
        _externalAuthenticationMethodModelFactory = externalAuthenticationMethodModelFactory;
        _permissionService = permissionService;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> Methods()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageExternalAuthenticationMethods))
            return AdminApiAccessDenied();

        //prepare model
        var model = _externalAuthenticationMethodModelFactory
            .PrepareExternalAuthenticationMethodSearchModel(new ExternalAuthenticationMethodSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Methods([FromBody] BaseQueryModel<ExternalAuthenticationMethodSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageExternalAuthenticationMethods))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _externalAuthenticationMethodModelFactory.PrepareExternalAuthenticationMethodListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MethodUpdate([FromBody] BaseQueryModel<ExternalAuthenticationMethodModel> queryModel)
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
}