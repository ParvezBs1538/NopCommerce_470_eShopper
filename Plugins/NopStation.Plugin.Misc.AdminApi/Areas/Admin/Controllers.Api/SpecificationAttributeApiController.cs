using System;
using System.Collections.Generic;
using System.Linq;
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

[Route("api/a/specificationattribute/[action]")]
public partial class SpecificationAttributeApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IPermissionService _permissionService;
    private readonly ISpecificationAttributeModelFactory _specificationAttributeModelFactory;
    private readonly ISpecificationAttributeService _specificationAttributeService;

    #endregion Fields

    #region Ctor

    public SpecificationAttributeApiController(ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IPermissionService permissionService,
        ISpecificationAttributeModelFactory specificationAttributeModelFactory,
        ISpecificationAttributeService specificationAttributeService)
    {
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _permissionService = permissionService;
        _specificationAttributeModelFactory = specificationAttributeModelFactory;
        _specificationAttributeService = specificationAttributeService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateAttributeLocalesAsync(SpecificationAttribute specificationAttribute, SpecificationAttributeModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(specificationAttribute,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    protected virtual async Task UpdateAttributeGroupLocalesAsync(SpecificationAttributeGroup specificationAttributeGroup, SpecificationAttributeGroupModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(specificationAttributeGroup,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    protected virtual async Task UpdateOptionLocalesAsync(SpecificationAttributeOption specificationAttributeOption, SpecificationAttributeOptionModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(specificationAttributeOption,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    #endregion

    #region Specification attributes

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeGroupSearchModelAsync(new SpecificationAttributeGroupSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SpecificationAttributeGroupList([FromBody] BaseQueryModel<SpecificationAttributeGroupSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        var model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeGroupListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SpecificationAttributeList([FromBody] BaseQueryModel<SpecificationAttributeSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        SpecificationAttributeGroup group = null;
        var searchModel = queryModel.Data;

        if (searchModel.SpecificationAttributeGroupId > 0)
        {
            group = await _specificationAttributeService.GetSpecificationAttributeGroupByIdAsync(searchModel.SpecificationAttributeGroupId);
            if (group == null)
                return NotFound("No specification attribute group found with the specified id");
        }

        var model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeListModelAsync(searchModel, group);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> CreateSpecificationAttributeGroup()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeGroupModelAsync(new SpecificationAttributeGroupModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CreateSpecificationAttributeGroup([FromBody] BaseQueryModel<SpecificationAttributeGroupModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var specificationAttributeGroup = model.ToEntity<SpecificationAttributeGroup>();
            await _specificationAttributeService.InsertSpecificationAttributeGroupAsync(specificationAttributeGroup);
            await UpdateAttributeGroupLocalesAsync(specificationAttributeGroup, model);

            await _customerActivityService.InsertActivityAsync("AddNewSpecAttributeGroup",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewSpecAttributeGroup"), specificationAttributeGroup.Name), specificationAttributeGroup);

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.SpecificationAttributes.SpecificationAttributeGroup.Added"));
        }

        model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeGroupModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    public virtual async Task<IActionResult> CreateSpecificationAttribute()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeModelAsync(new SpecificationAttributeModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CreateSpecificationAttribute([FromBody] BaseQueryModel<SpecificationAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var specificationAttribute = model.ToEntity<SpecificationAttribute>();
            await _specificationAttributeService.InsertSpecificationAttributeAsync(specificationAttribute);
            await UpdateAttributeLocalesAsync(specificationAttribute, model);

            await _customerActivityService.InsertActivityAsync("AddNewSpecAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewSpecAttribute"), specificationAttribute.Name), specificationAttribute);

            return Created(specificationAttribute.Id, await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.SpecificationAttributes.SpecificationAttribute.Added"));
        }

        model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> EditSpecificationAttributeGroup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var specificationAttributeGroup = await _specificationAttributeService.GetSpecificationAttributeGroupByIdAsync(id);
        if (specificationAttributeGroup == null)
            return NotFound("No specification attribute group found with the specified id");

        var model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeGroupModelAsync(null, specificationAttributeGroup);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditSpecificationAttributeGroup([FromBody] BaseQueryModel<SpecificationAttributeGroupModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;

        var specificationAttributeGroup = await _specificationAttributeService.GetSpecificationAttributeGroupByIdAsync(model.Id);
        if (specificationAttributeGroup == null)
            return NotFound("No specification attribute group found with the specified id");

        if (ModelState.IsValid)
        {
            specificationAttributeGroup = model.ToEntity(specificationAttributeGroup);
            await _specificationAttributeService.UpdateSpecificationAttributeGroupAsync(specificationAttributeGroup);
            await UpdateAttributeGroupLocalesAsync(specificationAttributeGroup, model);

            await _customerActivityService.InsertActivityAsync("EditSpecAttributeGroup",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditSpecAttributeGroup"), specificationAttributeGroup.Name), specificationAttributeGroup);

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.SpecificationAttributes.SpecificationAttributeGroup.Updated"));
        }

        model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeGroupModelAsync(model, specificationAttributeGroup, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> EditSpecificationAttribute(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a specification attribute with the specified id
        var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(id);
        if (specificationAttribute == null)
            return NotFound("No specification attribute found with the specified id");

        //prepare model
        var model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeModelAsync(null, specificationAttribute);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditSpecificationAttribute([FromBody] BaseQueryModel<SpecificationAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a specification attribute with the specified id
        var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(model.Id);
        if (specificationAttribute == null)
            return NotFound("No specification attribute found with the specified id");

        if (ModelState.IsValid)
        {
            specificationAttribute = model.ToEntity(specificationAttribute);
            await _specificationAttributeService.UpdateSpecificationAttributeAsync(specificationAttribute);

            await UpdateAttributeLocalesAsync(specificationAttribute, model);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSpecAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditSpecAttribute"), specificationAttribute.Name), specificationAttribute);

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.SpecificationAttributes.SpecificationAttribute.Updated"));
        }

        //prepare model
        model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeModelAsync(model, specificationAttribute, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> DeleteSpecificationAttributeGroup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var specificationAttributeGroup = await _specificationAttributeService.GetSpecificationAttributeGroupByIdAsync(id);
        if (specificationAttributeGroup == null)
            return NotFound("No specification attribute group found with the specified id");

        await _specificationAttributeService.DeleteSpecificationAttributeGroupAsync(specificationAttributeGroup);

        await _customerActivityService.InsertActivityAsync("DeleteSpecAttributeGroup",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteSpecAttributeGroup"), specificationAttributeGroup.Name), specificationAttributeGroup);

        return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.SpecificationAttributes.SpecificationAttributeGroup.Deleted"));
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> DeleteSpecificationAttribute(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(id);

        if (specificationAttribute == null)
            return NotFound("No specification attribute found with the specified id");

        await _specificationAttributeService.DeleteSpecificationAttributeAsync(specificationAttribute);

        await _customerActivityService.InsertActivityAsync("DeleteSpecAttribute",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteSpecAttribute"), specificationAttribute.Name), specificationAttribute);

        return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.SpecificationAttributes.SpecificationAttribute.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelectedSpecificationAttributes([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds != null || selectedIds.Count == 0)
            return NotFound("Not found with specified Id");

        var specificationAttributes = await _specificationAttributeService.GetSpecificationAttributeByIdsAsync(selectedIds.ToArray());
        await _specificationAttributeService.DeleteSpecificationAttributesAsync(specificationAttributes);

        foreach (var specificationAttribute in specificationAttributes)
        {
            await _customerActivityService.InsertActivityAsync("DeleteSpecAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteSpecAttribute"), specificationAttribute.Name), specificationAttribute);
        }

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Specification attribute options

    [HttpPost]
    public virtual async Task<IActionResult> OptionList([FromBody] BaseQueryModel<SpecificationAttributeOptionSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a specification attribute with the specified id
        var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(searchModel.SpecificationAttributeId);
        if (specificationAttribute == null)
            return NotFound("No specification attribute found with the specified id");

        //prepare model
        var model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeOptionListModelAsync(searchModel, specificationAttribute);

        return OkWrap(model);
    }

    [HttpGet("{specificationAttributeId}")]
    public virtual async Task<IActionResult> OptionCreatePopup(int specificationAttributeId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a specification attribute with the specified id
        var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(specificationAttributeId);
        if (specificationAttribute == null)
            return NotFound("No specification attribute found with the specified id");

        //prepare model
        var model = await _specificationAttributeModelFactory
            .PrepareSpecificationAttributeOptionModelAsync(new SpecificationAttributeOptionModel(), specificationAttribute, null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> OptionCreatePopup([FromBody] BaseQueryModel<SpecificationAttributeOptionModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;

        //try to get a specification attribute with the specified id
        var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(model.SpecificationAttributeId);
        if (specificationAttribute == null)
            return NotFound("No specification attribute found with the specified id");

        if (ModelState.IsValid)
        {
            var sao = model.ToEntity<SpecificationAttributeOption>();

            //clear "Color" values if it's disabled
            if (!model.EnableColorSquaresRgb)
                sao.ColorSquaresRgb = null;

            await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(sao);

            await UpdateOptionLocalesAsync(sao, model);

            return OkWrap(model);
        }

        //prepare model
        model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeOptionModelAsync(model, specificationAttribute, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> OptionEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a specification attribute option with the specified id
        var specificationAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(id);
        if (specificationAttributeOption == null)
            return NotFound("No specification attribute option found with the specified id");

        //try to get a specification attribute with the specified id
        var specificationAttribute = await _specificationAttributeService
            .GetSpecificationAttributeByIdAsync(specificationAttributeOption.SpecificationAttributeId);
        if (specificationAttribute == null)
            return NotFound("No specification attribute found with the specified id");

        //prepare model
        var model = await _specificationAttributeModelFactory
            .PrepareSpecificationAttributeOptionModelAsync(null, specificationAttribute, specificationAttributeOption);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> OptionEditPopup([FromBody] BaseQueryModel<SpecificationAttributeOptionModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a specification attribute option with the specified id
        var specificationAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(model.Id);
        if (specificationAttributeOption == null)
            return NotFound("No specification attribute option found with the specified id");

        //try to get a specification attribute with the specified id
        var specificationAttribute = await _specificationAttributeService
            .GetSpecificationAttributeByIdAsync(specificationAttributeOption.SpecificationAttributeId);
        if (specificationAttribute == null)
            return NotFound("No specification attribute found with the specified id");

        if (ModelState.IsValid)
        {
            specificationAttributeOption = model.ToEntity(specificationAttributeOption);

            //clear "Color" values if it's disabled
            if (!model.EnableColorSquaresRgb)
                specificationAttributeOption.ColorSquaresRgb = null;

            await _specificationAttributeService.UpdateSpecificationAttributeOptionAsync(specificationAttributeOption);

            await UpdateOptionLocalesAsync(specificationAttributeOption, model);

            return OkWrap(model);
        }

        //prepare model
        model = await _specificationAttributeModelFactory
            .PrepareSpecificationAttributeOptionModelAsync(model, specificationAttribute, specificationAttributeOption, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}/{specificationAttributeId}")]
    public virtual async Task<IActionResult> OptionDelete(int id, int specificationAttributeId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a specification attribute option with the specified id
        var specificationAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(id);
        if (specificationAttributeOption == null)
            return NotFound("No specification attribute option found with the specified id");

        await _specificationAttributeService.DeleteSpecificationAttributeOptionAsync(specificationAttributeOption);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{attributeId}")]
    public virtual async Task<IActionResult> GetOptionsByAttributeId(string attributeId)
    {
        //do not make any permission validation here 
        //because this method could be used on some other pages (such as product editing)
        //if (!await _permissionService.Authorize(StandardPermissionProvider.ManageAttributes))
        //    return AdminApiAccessDenied();

        //this action method gets called via an ajax request
        if (string.IsNullOrEmpty(attributeId))
            throw new ArgumentNullException(nameof(attributeId));

        var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(Convert.ToInt32(attributeId));
        var result = (from o in options
                      select new { id = o.Id, name = o.Name }).ToList();

        var response = new GenericResponseModel<object>();
        response.Data = result;

        return Ok(response);
    }

    #endregion

    #region Mapped products

    [HttpPost]
    public virtual async Task<IActionResult> UsedByProducts([FromBody] BaseQueryModel<SpecificationAttributeProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a specification attribute with the specified id
        var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(searchModel.SpecificationAttributeId);
        if (specificationAttribute == null)
            return NotFound("No specification attribute found with the specified id");

        //prepare model
        var model = await _specificationAttributeModelFactory.PrepareSpecificationAttributeProductListModelAsync(searchModel, specificationAttribute);

        return OkWrap(model);
    }

    #endregion
}