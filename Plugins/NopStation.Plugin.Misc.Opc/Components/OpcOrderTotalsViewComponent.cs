using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Web.Factories;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Misc.Opc.Components;

public class OpcOrderTotalsViewComponent : NopStationViewComponent
{
    #region Fields

    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly OpcSettings _opcSettings;

    #endregion Fields

    #region Ctor

    public OpcOrderTotalsViewComponent(IShoppingCartModelFactory shoppingCartModelFactory,
        IShoppingCartService shoppingCartService,
        IStoreContext storeContext,
        IWorkContext workContext,
        OpcSettings opcSettings)
    {
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _shoppingCartService = shoppingCartService;
        _storeContext = storeContext;
        _workContext = workContext;
        _opcSettings = opcSettings;
    }

    #endregion Ctor

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(bool isEditable)
    {
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        var model = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, _opcSettings.IsShoppingCartEditable);
        return View(model);
    }

    #endregion Methods
}