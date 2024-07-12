using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Orders;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/returnrequest/[action]")]
public partial class ReturnRequestApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IPermissionService _permissionService;
    private readonly IReturnRequestModelFactory _returnRequestModelFactory;
    private readonly IReturnRequestService _returnRequestService;
    private readonly IWorkflowMessageService _workflowMessageService;

    #endregion Fields

    #region Ctor

    public ReturnRequestApiController(ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IOrderService orderService,
        IPermissionService permissionService,
        IReturnRequestModelFactory returnRequestModelFactory,
        IReturnRequestService returnRequestService,
        IWorkflowMessageService workflowMessageService,
        IProductService productService)
    {
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _orderService = orderService;
        _permissionService = permissionService;
        _returnRequestModelFactory = returnRequestModelFactory;
        _returnRequestService = returnRequestService;
        _workflowMessageService = workflowMessageService;
        _productService = productService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(ReturnRequestReason rrr, ReturnRequestReasonModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(rrr,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    protected virtual async Task UpdateLocalesAsync(ReturnRequestAction rra, ReturnRequestActionModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(rra,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _returnRequestModelFactory.PrepareReturnRequestSearchModelAsync(new ReturnRequestSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<ReturnRequestSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _returnRequestModelFactory.PrepareReturnRequestListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AdminApiAccessDenied();

        //try to get a return request with the specified id
        var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(id);
        if (returnRequest == null)
            return NotFound("No return request found with the specified id");

        //prepare model
        var model = await _returnRequestModelFactory.PrepareReturnRequestModelAsync(null, returnRequest);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<ReturnRequestModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a return request with the specified id
        var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(model.Id);
        if (returnRequest == null)
            return NotFound("No return request found with the specified id");

        var errorList = new List<string>();
        if (ModelState.IsValid)
        {
            var quantityToReturn = model.ReturnedQuantity - returnRequest.ReturnedQuantity;
            if (quantityToReturn < 0)
                errorList.Add(string.Format(await _localizationService.GetResourceAsync("Admin.ReturnRequests.Fields.ReturnedQuantity.CannotBeLessThanQuantityAlreadyReturned"), returnRequest.ReturnedQuantity));
            else
            {
                if (quantityToReturn > 0)
                {
                    var orderItem = await _orderService.GetOrderItemByIdAsync(returnRequest.OrderItemId);
                    if (orderItem != null)
                    {
                        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                        if (product != null)
                        {
                            var productStockChangedMessage = string.Format(await _localizationService.GetResourceAsync("Admin.ReturnRequests.QuantityReturnedToStock"), quantityToReturn);

                            await _productService.AdjustInventoryAsync(product, quantityToReturn, orderItem.AttributesXml, productStockChangedMessage);
                        }
                    }
                }

                returnRequest = model.ToEntity(returnRequest);
                returnRequest.UpdatedOnUtc = DateTime.UtcNow;

                await _returnRequestService.UpdateReturnRequestAsync(returnRequest);

                //activity log
                await _customerActivityService.InsertActivityAsync("EditReturnRequest",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditReturnRequest"), returnRequest.Id), returnRequest);

                return Ok(await _localizationService.GetResourceAsync("Admin.ReturnRequests.Updated"));
            }
        }

        //prepare model
        model = await _returnRequestModelFactory.PrepareReturnRequestModelAsync(model, returnRequest, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState, errors: errorList);
    }

    [HttpPost]
    public virtual async Task<IActionResult> NotifyCustomer([FromBody] BaseQueryModel<ReturnRequestModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a return request with the specified id
        var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(model.Id);
        if (returnRequest == null)
            return NotFound("No return request found with the specified id");

        var orderItem = await _orderService.GetOrderItemByIdAsync(returnRequest.OrderItemId);
        if (orderItem is null)
        {
            return BadRequest(await _localizationService.GetResourceAsync("Admin.ReturnRequests.OrderItemDeleted"));
        }

        var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);

        var queuedEmailIds = await _workflowMessageService.SendReturnRequestStatusChangedCustomerNotificationAsync(returnRequest, orderItem, order);

        if (queuedEmailIds.Any())
            return Ok(await _localizationService.GetResourceAsync("Admin.ReturnRequests.Notified"));

        return BadRequest();
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests))
            return AdminApiAccessDenied();

        //try to get a return request with the specified id
        var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(id);
        if (returnRequest == null)
            return NotFound("No return request found with the specified id");

        await _returnRequestService.DeleteReturnRequestAsync(returnRequest);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteReturnRequest",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteReturnRequest"), returnRequest.Id), returnRequest);

        return Ok(await _localizationService.GetResourceAsync("Admin.ReturnRequests.Deleted"));
    }

    #region Return request reasons

    [HttpPost]
    public virtual async Task<IActionResult> ReturnRequestReasonList([FromBody] BaseQueryModel<ReturnRequestReasonSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _returnRequestModelFactory.PrepareReturnRequestReasonListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> ReturnRequestReasonCreate()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _returnRequestModelFactory.PrepareReturnRequestReasonModelAsync(new ReturnRequestReasonModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ReturnRequestReasonCreate([FromBody] BaseQueryModel<ReturnRequestReasonModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var returnRequestReason = model.ToEntity<ReturnRequestReason>();
            await _returnRequestService.InsertReturnRequestReasonAsync(returnRequestReason);

            //locales
            await UpdateLocalesAsync(returnRequestReason, model);

            return Created(returnRequestReason.Id, await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Order.ReturnRequestReasons.Added"));
        }

        //prepare model
        model = await _returnRequestModelFactory.PrepareReturnRequestReasonModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> ReturnRequestReasonEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a return request reason with the specified id
        var returnRequestReason = await _returnRequestService.GetReturnRequestReasonByIdAsync(id);
        if (returnRequestReason == null)
            return NotFound("No return request reason found with the specified id");

        //prepare model
        var model = await _returnRequestModelFactory.PrepareReturnRequestReasonModelAsync(null, returnRequestReason);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ReturnRequestReasonEdit([FromBody] BaseQueryModel<ReturnRequestReasonModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a return request reason with the specified id
        var returnRequestReason = await _returnRequestService.GetReturnRequestReasonByIdAsync(model.Id);
        if (returnRequestReason == null)
            return NotFound("No return request reason found with the specified id");

        if (ModelState.IsValid)
        {
            returnRequestReason = model.ToEntity(returnRequestReason);
            await _returnRequestService.UpdateReturnRequestReasonAsync(returnRequestReason);

            //locales
            await UpdateLocalesAsync(returnRequestReason, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Order.ReturnRequestReasons.Updated"));
        }

        //prepare model
        model = await _returnRequestModelFactory.PrepareReturnRequestReasonModelAsync(model, returnRequestReason, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ReturnRequestReasonDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a return request reason with the specified id
        var returnRequestReason = await _returnRequestService.GetReturnRequestReasonByIdAsync(id);
        if (returnRequestReason == null)
            return NotFound("No return request reason found with the specified id");

        await _returnRequestService.DeleteReturnRequestReasonAsync(returnRequestReason);

        return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Order.ReturnRequestReasons.Deleted"));
    }

    #endregion

    #region Return request actions

    [HttpPost]
    public virtual async Task<IActionResult> ReturnRequestActionList([FromBody] BaseQueryModel<ReturnRequestActionSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _returnRequestModelFactory.PrepareReturnRequestActionListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> ReturnRequestActionCreate()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _returnRequestModelFactory.PrepareReturnRequestActionModelAsync(new ReturnRequestActionModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ReturnRequestActionCreate([FromBody] BaseQueryModel<ReturnRequestActionModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var returnRequestAction = model.ToEntity<ReturnRequestAction>();
            await _returnRequestService.InsertReturnRequestActionAsync(returnRequestAction);

            //locales
            await UpdateLocalesAsync(returnRequestAction, model);

            return Created(returnRequestAction.Id, await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Order.ReturnRequestActions.Added"));
        }

        //prepare model
        model = await _returnRequestModelFactory.PrepareReturnRequestActionModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> ReturnRequestActionEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a return request action with the specified id
        var returnRequestAction = await _returnRequestService.GetReturnRequestActionByIdAsync(id);
        if (returnRequestAction == null)
            return NotFound("No return request action found with the specified id");

        //prepare model
        var model = await _returnRequestModelFactory.PrepareReturnRequestActionModelAsync(null, returnRequestAction);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ReturnRequestActionEdit([FromBody] BaseQueryModel<ReturnRequestActionModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a return request action with the specified id
        var returnRequestAction = await _returnRequestService.GetReturnRequestActionByIdAsync(model.Id);
        if (returnRequestAction == null)
            return NotFound("No return request action found with the specified id");

        if (ModelState.IsValid)
        {
            returnRequestAction = model.ToEntity(returnRequestAction);
            await _returnRequestService.UpdateReturnRequestActionAsync(returnRequestAction);

            //locales
            await UpdateLocalesAsync(returnRequestAction, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Order.ReturnRequestActions.Updated"));
        }

        //prepare model
        model = await _returnRequestModelFactory.PrepareReturnRequestActionModelAsync(model, returnRequestAction, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ReturnRequestActionDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a return request action with the specified id
        var returnRequestAction = await _returnRequestService.GetReturnRequestActionByIdAsync(id);
        if (returnRequestAction == null)
            return NotFound("No return request action found with the specified id");

        await _returnRequestService.DeleteReturnRequestActionAsync(returnRequestAction);

        return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Order.ReturnRequestActions.Deleted"));
    }

    #endregion

    #endregion
}