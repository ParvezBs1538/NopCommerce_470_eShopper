using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Vendors;
using Nop.Services.Attributes;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Vendors;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/vendorattribute/[action]")]
public partial class VendorAttributeApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IPermissionService _permissionService;
    private readonly IVendorAttributeModelFactory _vendorAttributeModelFactory;
    private readonly IAttributeService<VendorAttribute, VendorAttributeValue> _vendorAttributeService;

    #endregion

    #region Ctor

    public VendorAttributeApiController(ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IPermissionService permissionService,
        IVendorAttributeModelFactory vendorAttributeModelFactory,
        IAttributeService<VendorAttribute, VendorAttributeValue> vendorAttributeService)
    {
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _permissionService = permissionService;
        _vendorAttributeModelFactory = vendorAttributeModelFactory;
        _vendorAttributeService = vendorAttributeService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateAttributeLocalesAsync(VendorAttribute vendorAttribute, VendorAttributeModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(vendorAttribute,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    protected virtual async Task UpdateValueLocalesAsync(VendorAttributeValue vendorAttributeValue, VendorAttributeValueModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(vendorAttributeValue,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    #endregion

    #region Vendor attributes

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<VendorAttributeSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _vendorAttributeModelFactory.PrepareVendorAttributeListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _vendorAttributeModelFactory.PrepareVendorAttributeModelAsync(new VendorAttributeModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<VendorAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var errorList = new List<string>();
        if (ModelState.IsValid)
        {
            var vendorAttribute = model.ToEntity<VendorAttribute>();
            await _vendorAttributeService.InsertAttributeAsync(vendorAttribute);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewVendorAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewVendorAttribute"), vendorAttribute.Id), vendorAttribute);

            //locales
            await UpdateAttributeLocalesAsync(vendorAttribute, model);

            return Created(vendorAttribute.Id, await _localizationService.GetResourceAsync("Admin.Vendors.VendorAttributes.Added"), errorList);
        }

        //prepare model
        model = await _vendorAttributeModelFactory.PrepareVendorAttributeModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState, errorList);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a vendor attribute with the specified id
        var vendorAttribute = await _vendorAttributeService.GetAttributeByIdAsync(id);
        if (vendorAttribute == null)
            return NotFound("No vendor attribute found with the specified id");

        //prepare model
        var model = await _vendorAttributeModelFactory.PrepareVendorAttributeModelAsync(null, vendorAttribute);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<VendorAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;

        //try to get a vendor attribute with the specified id
        var vendorAttribute = await _vendorAttributeService.GetAttributeByIdAsync(model.Id);
        if (vendorAttribute == null)
            return NotFound("No vendor attribute found with the specified id");

        if (ModelState.IsValid)
        {
            vendorAttribute = model.ToEntity(vendorAttribute);
            await _vendorAttributeService.UpdateAttributeAsync(vendorAttribute);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditVendorAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditVendorAttribute"), vendorAttribute.Id), vendorAttribute);

            //locales
            await UpdateAttributeLocalesAsync(vendorAttribute, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Vendors.VendorAttributes.Updated"));
        }

        //prepare model
        model = await _vendorAttributeModelFactory.PrepareVendorAttributeModelAsync(model, vendorAttribute, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a vendor attribute with the specified id
        var vendorAttribute = await _vendorAttributeService.GetAttributeByIdAsync(id);
        if (vendorAttribute == null)
            return NotFound("No vendor attribute found with the specified id");

        await _vendorAttributeService.DeleteAttributeAsync(vendorAttribute);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteVendorAttribute",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteVendorAttribute"), vendorAttribute.Id), vendorAttribute);

        return Ok(await _localizationService.GetResourceAsync("Admin.Vendors.VendorAttributes.Deleted"));
    }

    #endregion

    #region Vendor attribute values

    [HttpPost]
    public virtual async Task<IActionResult> ValueList([FromBody] BaseQueryModel<VendorAttributeValueSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a vendor attribute with the specified id
        var vendorAttribute = await _vendorAttributeService.GetAttributeByIdAsync(searchModel.VendorAttributeId);
        if (vendorAttribute == null)
            return NotFound("No vendor attribute found with the specified id");

        //prepare model
        var model = await _vendorAttributeModelFactory.PrepareVendorAttributeValueListModelAsync(searchModel, vendorAttribute);

        return OkWrap(model);
    }

    [HttpGet("{vendorAttributeId}")]
    public virtual async Task<IActionResult> ValueCreatePopup(int vendorAttributeId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a vendor attribute with the specified id
        var vendorAttribute = await _vendorAttributeService.GetAttributeByIdAsync(vendorAttributeId);
        if (vendorAttribute == null)
            return NotFound("No vendor attribute found with the specified id");

        //prepare model
        var model = await _vendorAttributeModelFactory.PrepareVendorAttributeValueModelAsync(new VendorAttributeValueModel(), vendorAttribute, null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ValueCreatePopup([FromBody] BaseQueryModel<VendorAttributeValueModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a vendor attribute with the specified id
        var vendorAttribute = await _vendorAttributeService.GetAttributeByIdAsync(model.AttributeId);
        if (vendorAttribute == null)
            return NotFound("No vendor attribute found with the specified id");

        var errorList = new List<string>();
        if (ModelState.IsValid)
        {
            var value = model.ToEntity<VendorAttributeValue>();

            await _vendorAttributeService.InsertAttributeValueAsync(value);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewVendorAttributeValue",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewVendorAttributeValue"), value.Id), value);

            await UpdateValueLocalesAsync(value, model);

            return Created(value.Id, await _localizationService.GetResourceAsync("Admin.Vendors.VendorAttributes.Added"), errorList);
        }

        //prepare model
        model = await _vendorAttributeModelFactory.PrepareVendorAttributeValueModelAsync(model, vendorAttribute, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState, errorList);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> ValueEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a vendor attribute value with the specified id
        var vendorAttributeValue = await _vendorAttributeService.GetAttributeValueByIdAsync(id);
        if (vendorAttributeValue == null)
            return NotFound("No vendor attribute value found with the specified id");

        //try to get a vendor attribute with the specified id
        var vendorAttribute = await _vendorAttributeService.GetAttributeByIdAsync(vendorAttributeValue.AttributeId);
        if (vendorAttribute == null)
            return NotFound("No vendor attribute found with the specified id");

        //prepare model
        var model = await _vendorAttributeModelFactory.PrepareVendorAttributeValueModelAsync(null, vendorAttribute, vendorAttributeValue);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ValueEditPopup([FromBody] BaseQueryModel<VendorAttributeValueModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a vendor attribute value with the specified id
        var vendorAttributeValue = await _vendorAttributeService.GetAttributeValueByIdAsync(model.Id);
        if (vendorAttributeValue == null)
            return NotFound("No vendor attribute found with the specified id");

        //try to get a vendor attribute with the specified id
        var vendorAttribute = await _vendorAttributeService.GetAttributeByIdAsync(vendorAttributeValue.AttributeId);
        if (vendorAttribute == null)
            return NotFound("No vendor attribute value found with the specified id");

        var errorList = new List<string>();
        if (ModelState.IsValid)
        {
            vendorAttributeValue = model.ToEntity(vendorAttributeValue);
            await _vendorAttributeService.UpdateAttributeValueAsync(vendorAttributeValue);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditVendorAttributeValue",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditVendorAttributeValue"), vendorAttributeValue.Id),
                vendorAttributeValue);

            await UpdateValueLocalesAsync(vendorAttributeValue, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Vendors.VendorAttributes.Updated"), errorList);
        }

        //prepare model
        model = await _vendorAttributeModelFactory.PrepareVendorAttributeValueModelAsync(model, vendorAttribute, vendorAttributeValue, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState, errorList);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ValueDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a vendor attribute value with the specified id
        var value = await _vendorAttributeService.GetAttributeValueByIdAsync(id);
        if (value == null)
            return NotFound("No vendor attribute value found with the specified id");

        await _vendorAttributeService.DeleteAttributeValueAsync(value);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteVendorAttributeValue",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteVendorAttributeValue"), value.Id), value);

        return Ok(defaultMessage: true);
    }

    #endregion
}