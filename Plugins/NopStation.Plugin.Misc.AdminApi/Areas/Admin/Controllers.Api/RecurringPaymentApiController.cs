using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Orders;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/recurringpayment/[action]")]
public partial class RecurringPaymentApiController : BaseAdminApiController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IOrderService _orderService;
    private readonly IPermissionService _permissionService;
    private readonly IRecurringPaymentModelFactory _recurringPaymentModelFactory;

    #endregion Fields

    #region Ctor

    public RecurringPaymentApiController(ILocalizationService localizationService,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        IPermissionService permissionService,
        IRecurringPaymentModelFactory recurringPaymentModelFactory)
    {
        _localizationService = localizationService;
        _orderProcessingService = orderProcessingService;
        _orderService = orderService;
        _permissionService = permissionService;
        _recurringPaymentModelFactory = recurringPaymentModelFactory;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageRecurringPayments))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _recurringPaymentModelFactory.PrepareRecurringPaymentSearchModelAsync(new RecurringPaymentSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<RecurringPaymentSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageRecurringPayments))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _recurringPaymentModelFactory.PrepareRecurringPaymentListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageRecurringPayments))
            return AdminApiAccessDenied();

        //try to get a recurring payment with the specified id
        var payment = await _orderService.GetRecurringPaymentByIdAsync(id);
        if (payment == null || payment.Deleted)
            return NotFound("No recurring payment found with the specified id");

        //prepare model
        var model = await _recurringPaymentModelFactory.PrepareRecurringPaymentModelAsync(null, payment);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<RecurringPaymentModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageRecurringPayments))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a recurring payment with the specified id
        var payment = await _orderService.GetRecurringPaymentByIdAsync(model.Id);
        if (payment == null || payment.Deleted)
            return NotFound("No recurring payment found with the specified id");

        if (ModelState.IsValid)
        {
            payment = model.ToEntity(payment);
            await _orderService.UpdateRecurringPaymentAsync(payment);

            return Ok(await _localizationService.GetResourceAsync("Admin.RecurringPayments.Updated"));
        }

        //prepare model
        model = await _recurringPaymentModelFactory.PrepareRecurringPaymentModelAsync(model, payment, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageRecurringPayments))
            return AdminApiAccessDenied();

        //try to get a recurring payment with the specified id
        var payment = await _orderService.GetRecurringPaymentByIdAsync(id);
        if (payment == null)
            return NotFound("No recurring payment found with the specified id");

        await _orderService.DeleteRecurringPaymentAsync(payment);

        return Ok(await _localizationService.GetResourceAsync("Admin.RecurringPayments.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> HistoryList([FromBody] BaseQueryModel<RecurringPaymentHistorySearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageRecurringPayments))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a recurring payment with the specified id
        var payment = await _orderService.GetRecurringPaymentByIdAsync(searchModel.RecurringPaymentId);
        if (payment == null)
            return NotFound("No recurring payment found with the specified id");

        //prepare model
        var model = await _recurringPaymentModelFactory.PrepareRecurringPaymentHistoryListModelAsync(searchModel, payment);

        return OkWrap(model);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ProcessNextPayment(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageRecurringPayments))
            return AdminApiAccessDenied();

        //try to get a recurring payment with the specified id
        var payment = await _orderService.GetRecurringPaymentByIdAsync(id);
        if (payment == null)
            return NotFound("No recurring payment found with the specified id");

        try
        {
            var errors = (await _orderProcessingService.ProcessNextRecurringPaymentAsync(payment)).ToList();
            if (errors.Any())
                return BadRequest(errors: errors);

            //prepare model
            var model = await _recurringPaymentModelFactory.PrepareRecurringPaymentModelAsync(null, payment);

            return OkWrap(model, await _localizationService.GetResourceAsync("Admin.RecurringPayments.NextPaymentProcessed"));
        }
        catch (Exception exc)
        {
            //prepare model
            var model = await _recurringPaymentModelFactory.PrepareRecurringPaymentModelAsync(null, payment);
            return BadRequestWrap(model, null, [exc.Message]);
        }
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> CancelRecurringPayment(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageRecurringPayments))
            return AdminApiAccessDenied();

        //try to get a recurring payment with the specified id
        var payment = await _orderService.GetRecurringPaymentByIdAsync(id);
        if (payment == null)
            return NotFound("No recurring payment found with the specified id");

        try
        {
            var errors = await _orderProcessingService.CancelRecurringPaymentAsync(payment);
            if (errors.Any())
                return BadRequest(errors: errors);

            //prepare model
            var model = await _recurringPaymentModelFactory.PrepareRecurringPaymentModelAsync(null, payment);

            return OkWrap(model, await _localizationService.GetResourceAsync("Admin.RecurringPayments.Cancelled"));
        }
        catch (Exception exc)
        {
            //prepare model
            var model = await _recurringPaymentModelFactory.PrepareRecurringPaymentModelAsync(null, payment);
            return BadRequestWrap(model, null, [exc.Message]);
        }
    }

    #endregion
}