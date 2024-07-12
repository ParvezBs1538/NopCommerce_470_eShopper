using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.ShoppingCart;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/shoppingcart/[action]")]
public partial class ShoppingCartApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IPermissionService _permissionService;
    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    private readonly IShoppingCartService _shoppingCartService;

    #endregion

    #region Ctor

    public ShoppingCartApiController(ICustomerService customerService,
        IPermissionService permissionService,
        IShoppingCartService shoppingCartService,
        IShoppingCartModelFactory shoppingCartModelFactory)
    {
        _customerService = customerService;
        _permissionService = permissionService;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _shoppingCartService = shoppingCartService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> CurrentCarts()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrentCarts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _shoppingCartModelFactory.PrepareShoppingCartSearchModelAsync(new ShoppingCartSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CurrentCarts([FromBody] BaseQueryModel<ShoppingCartSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrentCarts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _shoppingCartModelFactory.PrepareShoppingCartListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> GetCartDetails([FromBody] BaseQueryModel<ShoppingCartItemSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrentCarts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(searchModel.CustomerId);
        if (customer == null)
            return NotFound("No customer found with the specified id");

        //prepare model
        var model = await _shoppingCartModelFactory.PrepareShoppingCartItemListModelAsync(searchModel, customer);

        return OkWrap(model);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> DeleteItem(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrentCarts))
            return AdminApiAccessDenied();

        await _shoppingCartService.DeleteShoppingCartItemAsync(id);

        return Ok(defaultMessage: true);
    }

    #endregion
}