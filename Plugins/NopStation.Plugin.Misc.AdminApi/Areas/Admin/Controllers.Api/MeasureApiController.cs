using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Directory;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/measure/[action]")]
public partial class MeasureApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly IMeasureModelFactory _measureModelFactory;
    private readonly IMeasureService _measureService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly MeasureSettings _measureSettings;

    #endregion

    #region Ctor

    public MeasureApiController(ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        IMeasureModelFactory measureModelFactory,
        IMeasureService measureService,
        IPermissionService permissionService,
        ISettingService settingService,
        MeasureSettings measureSettings)
    {
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _measureModelFactory = measureModelFactory;
        _measureService = measureService;
        _permissionService = permissionService;
        _settingService = settingService;
        _measureSettings = measureSettings;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _measureModelFactory.PrepareMeasureSearchModelAsync(new MeasureSearchModel());

        return OkWrap(model);
    }

    #region Weights

    [HttpPost]
    public virtual async Task<IActionResult> Weights([FromBody] BaseQueryModel<MeasureWeightSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _measureModelFactory.PrepareMeasureWeightListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> WeightUpdate([FromBody] BaseQueryModel<MeasureWeightModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        var weight = await _measureService.GetMeasureWeightByIdAsync(model.Id);
        weight = model.ToEntity(weight);
        await _measureService.UpdateMeasureWeightAsync(weight);

        //activity log
        await _customerActivityService.InsertActivityAsync("EditMeasureWeight",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditMeasureWeight"), weight.Id), weight);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> WeightAdd([FromBody] BaseQueryModel<MeasureWeightModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        var weight = new MeasureWeight();
        weight = model.ToEntity(weight);
        await _measureService.InsertMeasureWeightAsync(weight);

        //activity log
        await _customerActivityService.InsertActivityAsync("AddNewMeasureWeight",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewMeasureWeight"), weight.Id), weight);

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> WeightDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //try to get a weight with the specified id
        var weight = await _measureService.GetMeasureWeightByIdAsync(id);
        if (weight == null)
            return NotFound("No weight found with the specified id");

        if (weight.Id == _measureSettings.BaseWeightId)
        {
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Measures.Weights.CantDeletePrimary"));
        }

        await _measureService.DeleteMeasureWeightAsync(weight);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteMeasureWeight",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteMeasureWeight"), weight.Id), weight);

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> MarkAsPrimaryWeight(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //try to get a weight with the specified id
        var weight = await _measureService.GetMeasureWeightByIdAsync(id);
        if (weight == null)
            return NotFound("No weight found with the specified id");

        _measureSettings.BaseWeightId = weight.Id;
        await _settingService.SaveSettingAsync(_measureSettings);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Dimensions

    [HttpPost]
    public virtual async Task<IActionResult> Dimensions([FromBody] BaseQueryModel<MeasureDimensionSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _measureModelFactory.PrepareMeasureDimensionListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DimensionUpdate([FromBody] BaseQueryModel<MeasureDimensionModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        var dimension = await _measureService.GetMeasureDimensionByIdAsync(model.Id);
        dimension = model.ToEntity(dimension);
        await _measureService.UpdateMeasureDimensionAsync(dimension);

        //activity log
        await _customerActivityService.InsertActivityAsync("EditMeasureDimension",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditMeasureDimension"), dimension.Id), dimension);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DimensionAdd([FromBody] BaseQueryModel<MeasureDimensionModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        var dimension = new MeasureDimension();
        dimension = model.ToEntity(dimension);
        await _measureService.InsertMeasureDimensionAsync(dimension);

        //activity log
        await _customerActivityService.InsertActivityAsync("AddNewMeasureDimension",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewMeasureDimension"), dimension.Id), dimension);

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> DimensionDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //try to get a dimension with the specified id
        var dimension = await _measureService.GetMeasureDimensionByIdAsync(id);
        if (dimension == null)
            return NotFound("No dimension found with the specified id");

        if (dimension.Id == _measureSettings.BaseDimensionId)
        {
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Measures.Dimensions.CantDeletePrimary"));
        }

        await _measureService.DeleteMeasureDimensionAsync(dimension);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteMeasureDimension",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteMeasureDimension"), dimension.Id), dimension);

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> MarkAsPrimaryDimension(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
            return AdminApiAccessDenied();

        //try to get a dimension with the specified id
        var dimension = await _measureService.GetMeasureDimensionByIdAsync(id);
        if (dimension == null)
            return NotFound("No dimension found with the specified id");

        _measureSettings.BaseDimensionId = dimension.Id;
        await _settingService.SaveSettingAsync(_measureSettings);

        return Ok(defaultMessage: true);
    }

    #endregion

    #endregion
}