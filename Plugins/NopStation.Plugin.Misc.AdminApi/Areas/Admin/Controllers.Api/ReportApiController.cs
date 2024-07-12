using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Reports;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/report/[action]")]
public partial class ReportApiController : BaseAdminApiController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IReportModelFactory _reportModelFactory;

    #endregion

    #region Ctor

    public ReportApiController(
        IPermissionService permissionService,
        IReportModelFactory reportModelFactory)
    {
        _permissionService = permissionService;
        _reportModelFactory = reportModelFactory;
    }

    #endregion

    #region Methods

    #region Sales summary

    public virtual async Task<IActionResult> SalesSummary()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _reportModelFactory.PrepareSalesSummarySearchModelAsync(new SalesSummarySearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SalesSummaryList([FromBody] BaseQueryModel<SalesSummarySearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _reportModelFactory.PrepareSalesSummaryListModelAsync(searchModel);

        return OkWrap(model);
    }


    #endregion

    #region Low stock

    public virtual async Task<IActionResult> LowStock()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _reportModelFactory.PrepareLowStockProductSearchModelAsync(new LowStockProductSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> LowStockList([FromBody] BaseQueryModel<LowStockProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _reportModelFactory.PrepareLowStockProductListModelAsync(searchModel);

        return OkWrap(model);
    }

    #endregion

    #region Bestsellers

    public virtual async Task<IActionResult> Bestsellers()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _reportModelFactory.PrepareBestsellerSearchModelAsync(new BestsellerSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> BestsellersList([FromBody] BaseQueryModel<BestsellerSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _reportModelFactory.PrepareBestsellerListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> BestsellersReportAggregates([FromBody] BaseQueryModel<BestsellerSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var totalAmount = await _reportModelFactory.GetBestsellerTotalAmountAsync(searchModel);

        return Ok(new GenericResponseModel<object> { Data = new { AggregatorTotal = totalAmount } });
    }

    #endregion

    #region Never Sold

    public virtual async Task<IActionResult> NeverSold()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _reportModelFactory.PrepareNeverSoldSearchModelAsync(new NeverSoldReportSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> NeverSoldList([FromBody] BaseQueryModel<NeverSoldReportSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _reportModelFactory.PrepareNeverSoldListModelAsync(searchModel);

        return OkWrap(model);
    }

    #endregion

    #region Country sales

    public virtual async Task<IActionResult> CountrySales()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.OrderCountryReport))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _reportModelFactory.PrepareCountrySalesSearchModelAsync(new CountryReportSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CountrySalesList([FromBody] BaseQueryModel<CountryReportSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.OrderCountryReport))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _reportModelFactory.PrepareCountrySalesListModelAsync(searchModel);

        return OkWrap(model);
    }

    #endregion

    #region Customer reports

    public virtual async Task<IActionResult> RegisteredCustomers()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _reportModelFactory.PrepareCustomerReportsSearchModelAsync(new CustomerReportsSearchModel());

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> BestCustomersByOrderTotal()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _reportModelFactory.PrepareCustomerReportsSearchModelAsync(new CustomerReportsSearchModel());

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> BestCustomersByNumberOfOrders()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _reportModelFactory.PrepareCustomerReportsSearchModelAsync(new CustomerReportsSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ReportBestCustomersByOrderTotalList([FromBody] BaseQueryModel<BestCustomersReportSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _reportModelFactory.PrepareBestCustomersReportListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ReportBestCustomersByNumberOfOrdersList([FromBody] BaseQueryModel<BestCustomersReportSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _reportModelFactory.PrepareBestCustomersReportListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ReportRegisteredCustomersList([FromBody] BaseQueryModel<RegisteredCustomersReportSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _reportModelFactory.PrepareRegisteredCustomersReportListModelAsync(searchModel);

        return OkWrap(model);
    }

    #endregion

    #endregion
}
