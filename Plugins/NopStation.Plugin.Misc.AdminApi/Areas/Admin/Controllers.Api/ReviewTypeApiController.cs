using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/reviewtype/[action]")]
public partial class ReviewTypeApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IPermissionService _permissionService;
    private readonly IReviewTypeModelFactory _reviewTypeModelFactory;
    private readonly IReviewTypeService _reviewTypeService;

    #endregion

    #region Ctor

    public ReviewTypeApiController(ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IPermissionService permissionService,
        IReviewTypeModelFactory reviewTypeModelFactory,
        IReviewTypeService reviewTypeService)
    {
        _reviewTypeModelFactory = reviewTypeModelFactory;
        _reviewTypeService = reviewTypeService;
        _customerActivityService = customerActivityService;
        _localizedEntityService = localizedEntityService;
        _localizationService = localizationService;
        _permissionService = permissionService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateReviewTypeLocalesAsync(ReviewType reviewType, ReviewTypeModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(reviewType,
                x => x.Name,
                localized.Name,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(reviewType,
               x => x.Description,
               localized.Description,
               localized.LanguageId);
        }
    }

    #endregion

    #region Review type

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<ReviewTypeSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _reviewTypeModelFactory.PrepareReviewTypeListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _reviewTypeModelFactory.PrepareReviewTypeModelAsync(new ReviewTypeModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<ReviewTypeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var reviewType = model.ToEntity<ReviewType>();
            await _reviewTypeService.InsertReviewTypeAsync(reviewType);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewReviewType",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewReviewType"), reviewType.Id), reviewType);

            //locales                
            await UpdateReviewTypeLocalesAsync(reviewType, model);

            return Created(reviewType.Id, await _localizationService.GetResourceAsync("Admin.Settings.ReviewType.Added"));
        }

        //prepare model
        model = await _reviewTypeModelFactory.PrepareReviewTypeModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get an product review type with the specified id
        var reviewType = await _reviewTypeService.GetReviewTypeByIdAsync(id);
        if (reviewType == null)
            return NotFound("No review type found with the specified id");

        //prepare model
        var model = await _reviewTypeModelFactory.PrepareReviewTypeModelAsync(null, reviewType);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<ReviewTypeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get an review type with the specified id
        var reviewType = await _reviewTypeService.GetReviewTypeByIdAsync(model.Id);
        if (reviewType == null)
            return NotFound("No review type found with the specified id");

        if (ModelState.IsValid)
        {
            reviewType = model.ToEntity(reviewType);
            await _reviewTypeService.UpdateReviewTypeAsync(reviewType);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditReviewType",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditReviewType"), reviewType.Id),
                reviewType);

            //locales
            await UpdateReviewTypeLocalesAsync(reviewType, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Settings.ReviewType.Updated"));
        }

        //prepare model
        model = await _reviewTypeModelFactory.PrepareReviewTypeModelAsync(model, reviewType, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get an review type with the specified id
        var reviewType = await _reviewTypeService.GetReviewTypeByIdAsync(id);
        if (reviewType == null)
            return NotFound("No review type found with the specified id");

        try
        {
            await _reviewTypeService.DeleteReviewTypeAsync(reviewType);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteReviewType",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteReviewType"), reviewType),
                reviewType);

            return Ok(await _localizationService.GetResourceAsync("Admin.Settings.ReviewType.Deleted"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    #endregion
}
