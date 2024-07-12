using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Misc.Opc.Factories;

namespace NopStation.Plugin.Misc.Opc.Components;

public class OpcEstimateShippingViewComponent : NopStationViewComponent
{
    private readonly OpcSettings _opcSettings;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IOpcModelFactory _opcModelFactory;

    public OpcEstimateShippingViewComponent(OpcSettings opcSettings,
        IStoreContext storeContext,
        IWorkContext workContext,
        IShoppingCartService shoppingCartService,
        IOpcModelFactory opcModelFactory)
    {
        _opcSettings = opcSettings;
        _storeContext = storeContext;
        _workContext = workContext;
        _shoppingCartService = shoppingCartService;
        _opcModelFactory = opcModelFactory;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!_opcSettings.ShowEstimateShippingInCheckout)
            return Content(string.Empty);
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        var model = await _opcModelFactory.PrepareEstimateShippingModelAsync(cart);
        if (!model.Enabled)
            return Content(string.Empty);

        return View(model);
    }
}