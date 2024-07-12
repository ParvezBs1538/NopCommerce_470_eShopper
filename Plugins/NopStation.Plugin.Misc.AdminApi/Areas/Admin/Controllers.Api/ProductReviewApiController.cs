using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/productreview/[action]")]
public partial class ProductReviewApiController : BaseAdminApiController
{
    #region Fields

    private readonly CatalogSettings _catalogSettings;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ICustomerService _customerService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IProductReviewModelFactory _productReviewModelFactory;
    private readonly IProductService _productService;
    private readonly IWorkContext _workContext;
    private readonly IWorkflowMessageService _workflowMessageService;

    #endregion Fields

    #region Ctor

    public ProductReviewApiController(CatalogSettings catalogSettings,
        ICustomerActivityService customerActivityService,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IProductReviewModelFactory productReviewModelFactory,
        IProductService productService,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        ICustomerService customerService)
    {
        _catalogSettings = catalogSettings;
        _customerActivityService = customerActivityService;
        _eventPublisher = eventPublisher;
        _genericAttributeService = genericAttributeService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _productReviewModelFactory = productReviewModelFactory;
        _productService = productService;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _customerService = customerService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductReviews))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productReviewModelFactory.PrepareProductReviewSearchModelAsync(new ProductReviewSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<ProductReviewSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductReviews))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _productReviewModelFactory.PrepareProductReviewListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductReviews))
            return AdminApiAccessDenied();

        //try to get a product review with the specified id
        var productReview = await _productService.GetProductReviewByIdAsync(id);
        if (productReview == null)
            return NotFound("No product review found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && (await _productService.GetProductByIdAsync(productReview.ProductId)).VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productReviewModelFactory.PrepareProductReviewModelAsync(null, productReview);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<ProductReviewModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductReviews))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product review with the specified id
        var productReview = await _productService.GetProductReviewByIdAsync(model.Id);
        if (productReview == null)
            return NotFound("No product review found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && (await _productService.GetProductByIdAsync(productReview.ProductId)).VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        if (ModelState.IsValid)
        {
            var previousIsApproved = productReview.IsApproved;

            //vendor can edit "Reply text" only
            var isLoggedInAsVendor = currentVendor != null;
            if (!isLoggedInAsVendor)
            {
                productReview.Title = model.Title;
                productReview.ReviewText = model.ReviewText;
                productReview.IsApproved = model.IsApproved;
            }

            productReview.ReplyText = model.ReplyText;

            //notify customer about reply
            if (productReview.IsApproved && !string.IsNullOrEmpty(productReview.ReplyText)
                && _catalogSettings.NotifyCustomerAboutProductReviewReply && !productReview.CustomerNotifiedOfReply)
            {
                var customer = await _customerService.GetCustomerByIdAsync(productReview.CustomerId);
                var customerLanguageId = customer?.LanguageId ?? 0;

                var queuedEmailIds = await _workflowMessageService.SendProductReviewReplyCustomerNotificationMessageAsync(productReview, customerLanguageId);
                if (queuedEmailIds.Any())
                    productReview.CustomerNotifiedOfReply = true;
            }

            await _productService.UpdateProductReviewAsync(productReview);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditProductReview",
               string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditProductReview"), productReview.Id), productReview);

            //vendor can edit "Reply text" only
            if (!isLoggedInAsVendor)
            {
                var product = await _productService.GetProductByIdAsync(productReview.ProductId);
                //update product totals
                await _productService.UpdateProductReviewTotalsAsync(product);

                //raise event (only if it wasn't approved before and is approved now)
                if (!previousIsApproved && productReview.IsApproved)
                    await _eventPublisher.PublishAsync(new ProductReviewApprovedEvent(productReview));
            }

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.ProductReviews.Updated"));
        }

        //prepare model
        model = await _productReviewModelFactory.PrepareProductReviewModelAsync(model, productReview, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductReviews))
            return AdminApiAccessDenied();

        //try to get a product review with the specified id
        var productReview = await _productService.GetProductReviewByIdAsync(id);
        if (productReview == null)
            return NotFound("No product review found with the specified id");

        //a vendor does not have access to this functionality
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
            return AdminApiAccessDenied();

        await _productService.DeleteProductReviewAsync(productReview);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteProductReview",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteProductReview"), productReview.Id), productReview);

        var product = await _productService.GetProductByIdAsync(productReview.ProductId);

        //update product totals
        await _productService.UpdateProductReviewTotalsAsync(product);

        return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.ProductReviews.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> ApproveSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductReviews))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        //a vendor does not have access to this functionality
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
            return AdminApiAccessDenied();

        if (selectedIds == null)
            return Ok(defaultMessage: true);

        //filter not approved reviews
        var productReviews = (await _productService.GetProductReviewsByIdsAsync(selectedIds.ToArray())).Where(review => !review.IsApproved);

        foreach (var productReview in productReviews)
        {
            productReview.IsApproved = true;
            await _productService.UpdateProductReviewAsync(productReview);

            var product = await _productService.GetProductByIdAsync(productReview.ProductId);

            //update product totals
            await _productService.UpdateProductReviewTotalsAsync(product);

            //raise event 
            await _eventPublisher.PublishAsync(new ProductReviewApprovedEvent(productReview));
        }

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DisapproveSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductReviews))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        //a vendor does not have access to this functionality
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
            return AdminApiAccessDenied();

        if (selectedIds == null)
            return Ok(defaultMessage: true);

        //filter approved reviews
        var productReviews = (await _productService.GetProductReviewsByIdsAsync(selectedIds.ToArray())).Where(review => review.IsApproved);

        foreach (var productReview in productReviews)
        {
            productReview.IsApproved = false;
            await _productService.UpdateProductReviewAsync(productReview);

            var product = await _productService.GetProductByIdAsync(productReview.ProductId);

            //update product totals
            await _productService.UpdateProductReviewTotalsAsync(product);
        }

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductReviews))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        //a vendor does not have access to this functionality
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
            return AdminApiAccessDenied();

        if (selectedIds == null)
            return Ok(defaultMessage: true);

        var productReviews = await _productService.GetProductReviewsByIdsAsync(selectedIds.ToArray());
        var products = await _productService.GetProductsByIdsAsync(productReviews.Select(p => p.ProductId).Distinct().ToArray());

        await _productService.DeleteProductReviewsAsync(productReviews);

        //update product totals
        foreach (var product in products)
        {
            await _productService.UpdateProductReviewTotalsAsync(product);
        }

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductReviewReviewTypeMappingList([FromBody] BaseQueryModel<ProductReviewReviewTypeMappingSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductReviews))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        var productReview = await _productService.GetProductReviewByIdAsync(searchModel.ProductReviewId);
        if (productReview == null)
            return NotFound("No product review found with the specified id");

        //prepare model
        var model = await _productReviewModelFactory.PrepareProductReviewReviewTypeMappingListModelAsync(searchModel, productReview);

        return OkWrap(model);
    }

    #endregion
}