using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Customers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/onlinecustomer/[action]")]
public partial class OnlineCustomerApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerModelFactory _customerModelFactory;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public OnlineCustomerApiController(ICustomerModelFactory customerModelFactory,
        IPermissionService permissionService)
    {
        _customerModelFactory = customerModelFactory;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _customerModelFactory.PrepareOnlineCustomerSearchModelAsync(new OnlineCustomerSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<OnlineCustomerSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _customerModelFactory.PrepareOnlineCustomerListModelAsync(searchModel);

        return OkWrap(model);
    }

    #endregion
}