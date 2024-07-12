using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Misc.Opc.Components;

public partial class ProductBoxBuyNowViewComponent : NopStationViewComponent
{
    private readonly IProductService _productService;
    private readonly OpcSettings _opcSettings;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;

    public ProductBoxBuyNowViewComponent(IProductService productService,
        OpcSettings opcSettings,
        IWidgetPluginManager widgetPluginManager,
        IStoreContext storeContext,
        IWorkContext workContext)
    {
        _productService = productService;
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

        if (additionalData.GetType() != typeof(ProductOverviewModel))
            return Content("");

        var productOverviewModel = additionalData as ProductOverviewModel;

        var product = await _productService.GetProductByIdAsync(productOverviewModel.Id);
        if (product == null)
            return Content("");

        if (!_opcSettings.EnableOnePageCheckout)
            return Content("");

        if (!_opcSettings.EnableBuyNowButton)
            return Content("");

        return View(productOverviewModel);
    }
}
