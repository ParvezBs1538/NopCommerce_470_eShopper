using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Cms;
using Nop.Core.Events;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Cms;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/widget/[action]")]
public partial class WidgetApiController : BaseAdminApiController
{
    #region Fields

    private readonly IEventPublisher _eventPublisher;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IWidgetModelFactory _widgetModelFactory;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly WidgetSettings _widgetSettings;

    #endregion

    #region Ctor

    public WidgetApiController(IEventPublisher eventPublisher,
        IPermissionService permissionService,
        ISettingService settingService,
        IWidgetModelFactory widgetModelFactory,
        IWidgetPluginManager widgetPluginManager,
        WidgetSettings widgetSettings)
    {
        _eventPublisher = eventPublisher;
        _permissionService = permissionService;
        _settingService = settingService;
        _widgetModelFactory = widgetModelFactory;
        _widgetPluginManager = widgetPluginManager;
        _widgetSettings = widgetSettings;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _widgetModelFactory.PrepareWidgetSearchModelAsync(new WidgetSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<WidgetSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _widgetModelFactory.PrepareWidgetListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> WidgetUpdate([FromBody] BaseQueryModel<WidgetModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
            return AdminApiAccessDenied();

        var model = queryModel.Data;

        var widget = await _widgetPluginManager.LoadPluginBySystemNameAsync(model.SystemName);
        if (_widgetPluginManager.IsPluginActive(widget, _widgetSettings.ActiveWidgetSystemNames))
        {
            if (!model.IsActive)
            {
                //mark as disabled
                _widgetSettings.ActiveWidgetSystemNames.Remove(widget.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }
        }
        else
        {
            if (model.IsActive)
            {
                //mark as active
                _widgetSettings.ActiveWidgetSystemNames.Add(widget.PluginDescriptor.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }
        }

        var pluginDescriptor = widget.PluginDescriptor;

        //display order
        pluginDescriptor.DisplayOrder = model.DisplayOrder;

        //update the description file
        pluginDescriptor.Save();

        //raise event
        await _eventPublisher.PublishAsync(new PluginUpdatedEvent(pluginDescriptor));

        return Ok(defaultMessage: true);
    }

    #endregion
}