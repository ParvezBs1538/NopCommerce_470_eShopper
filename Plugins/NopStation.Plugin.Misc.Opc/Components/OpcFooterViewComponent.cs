using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Cms;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Misc.Opc.Components;

public class OpcFooterViewComponent : NopStationViewComponent
{
    private readonly OpcSettings _opcSettings;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;

    public OpcFooterViewComponent(OpcSettings opcSettings,
        IWidgetPluginManager widgetPluginManager,
        IStoreContext storeContext,
        IWorkContext workContext)
    {
        _opcSettings = opcSettings;
        _widgetPluginManager = widgetPluginManager;
        _storeContext = storeContext;
        _workContext = workContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        if (await _widgetPluginManager.IsPluginActiveAsync("NopStation.Plugin.Widgets.AdvanceCart", customer, store.Id))
            return Content("");

        if (!_opcSettings.EnableOnePageCheckout)
            return Content("");

        return View();
    }
}
