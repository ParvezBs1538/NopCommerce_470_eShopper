using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Tax;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Tax;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/tax/[action]")]
public partial class TaxApiController : BaseAdminApiController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly ITaxCategoryService _taxCategoryService;
    private readonly ITaxModelFactory _taxModelFactory;
    private readonly ITaxPluginManager _taxPluginManager;
    private readonly TaxSettings _taxSettings;

    #endregion

    #region Ctor

    public TaxApiController(IPermissionService permissionService,
        ISettingService settingService,
        ITaxCategoryService taxCategoryService,
        IGenericAttributeService genericAttributeService,
        IWorkContext workContext,
        ITaxModelFactory taxModelFactory,
        ITaxPluginManager taxPluginManager,
        TaxSettings taxSettings)
    {
        _permissionService = permissionService;
        _settingService = settingService;
        _taxCategoryService = taxCategoryService;
        _taxModelFactory = taxModelFactory;
        _taxPluginManager = taxPluginManager;
        _taxSettings = taxSettings;
    }

    #endregion

    #region Methods

    #region Tax Providers

    public virtual async Task<IActionResult> Providers()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaxSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _taxModelFactory.PrepareTaxProviderSearchModelAsync(new TaxProviderSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Providers([FromBody] BaseQueryModel<TaxProviderSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaxSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _taxModelFactory.PrepareTaxProviderListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost("{systemName}")]
    public virtual async Task<IActionResult> MarkAsPrimaryProvider(string systemName)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaxSettings))
            return AdminApiAccessDenied();

        if (string.IsNullOrEmpty(systemName))
            return BadRequest();

        var taxProvider = await _taxPluginManager.LoadPluginBySystemNameAsync(systemName);
        if (taxProvider == null)
            return NotFound("No tax provider found with the specified id");

        _taxSettings.ActiveTaxProviderSystemName = systemName;
        await _settingService.SaveSettingAsync(_taxSettings);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Tax Categories

    public virtual async Task<IActionResult> Categories()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaxSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _taxModelFactory.PrepareTaxCategorySearchModelAsync(new TaxCategorySearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Categories([FromBody] BaseQueryModel<TaxCategorySearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaxSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _taxModelFactory.PrepareTaxCategoryListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CategoryUpdate([FromBody] BaseQueryModel<TaxCategoryModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaxSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        var taxCategory = await _taxCategoryService.GetTaxCategoryByIdAsync(model.Id);
        taxCategory = model.ToEntity(taxCategory);
        await _taxCategoryService.UpdateTaxCategoryAsync(taxCategory);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CategoryAdd([FromBody] BaseQueryModel<TaxCategoryModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaxSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        var taxCategory = new TaxCategory();
        taxCategory = model.ToEntity(taxCategory);
        await _taxCategoryService.InsertTaxCategoryAsync(taxCategory);

        return Created(taxCategory.Id);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> CategoryDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaxSettings))
            return AdminApiAccessDenied();

        //try to get a tax category with the specified id
        var taxCategory = await _taxCategoryService.GetTaxCategoryByIdAsync(id);
        if (taxCategory == null)
            return NotFound("No tax category found with the specified id");

        await _taxCategoryService.DeleteTaxCategoryAsync(taxCategory);

        return Ok(defaultMessage: true);
    }

    #endregion

    #endregion
}