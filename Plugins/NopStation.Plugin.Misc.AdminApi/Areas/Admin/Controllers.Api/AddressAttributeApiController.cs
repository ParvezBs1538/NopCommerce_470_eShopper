using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Common;
using Nop.Services.Attributes;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/addressattribute/[action]")]
public partial class AddressAttributeApiController : BaseAdminApiController
{
    #region Fields

    private readonly IAddressAttributeModelFactory _addressAttributeModelFactory;
    private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _addressAttributeService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public AddressAttributeApiController(IAddressAttributeModelFactory addressAttributeModelFactory,
        IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService,
        ICustomerActivityService customerActivityService,
        ILocalizedEntityService localizedEntityService,
        ILocalizationService localizationService,
        IPermissionService permissionService)
    {
        _addressAttributeModelFactory = addressAttributeModelFactory;
        _addressAttributeService = addressAttributeService;
        _customerActivityService = customerActivityService;
        _localizedEntityService = localizedEntityService;
        _localizationService = localizationService;
        _permissionService = permissionService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateAttributeLocalesAsync(AddressAttribute addressAttribute, AddressAttributeModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(addressAttribute,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    protected virtual async Task UpdateValueLocalesAsync(AddressAttributeValue addressAttributeValue, AddressAttributeValueModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(addressAttributeValue,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    #endregion

    #region Address attributes

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<AddressAttributeSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _addressAttributeModelFactory.PrepareAddressAttributeListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _addressAttributeModelFactory.PrepareAddressAttributeModelAsync(new AddressAttributeModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<AddressAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;

        if (ModelState.IsValid)
        {
            var addressAttribute = model.ToEntity<AddressAttribute>();
            await _addressAttributeService.InsertAttributeAsync(addressAttribute);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewAddressAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewAddressAttribute"), addressAttribute.Id),
                addressAttribute);

            //locales
            await UpdateAttributeLocalesAsync(addressAttribute, model);

            return Created(addressAttribute.Id, await _localizationService.GetResourceAsync("Admin.Address.AddressAttributes.Added"));
        }

        //prepare model
        model = await _addressAttributeModelFactory.PrepareAddressAttributeModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get an address attribute with the specified id
        var addressAttribute = await _addressAttributeService.GetAttributeByIdAsync(id);
        if (addressAttribute == null)
            return NotFound("No address attribute found with the specified id");

        //prepare model
        var model = await _addressAttributeModelFactory.PrepareAddressAttributeModelAsync(null, addressAttribute);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<AddressAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;

        //try to get an address attribute with the specified id
        var addressAttribute = await _addressAttributeService.GetAttributeByIdAsync(model.Id);
        if (addressAttribute == null)
            return NotFound();

        if (ModelState.IsValid)
        {
            addressAttribute = model.ToEntity(addressAttribute);
            await _addressAttributeService.UpdateAttributeAsync(addressAttribute);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditAddressAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditAddressAttribute"), addressAttribute.Id),
                addressAttribute);

            //locales
            await UpdateAttributeLocalesAsync(addressAttribute, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Address.AddressAttributes.Updated"));
        }

        //prepare model
        model = await _addressAttributeModelFactory.PrepareAddressAttributeModelAsync(model, addressAttribute, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get an address attribute with the specified id
        var addressAttribute = await _addressAttributeService.GetAttributeByIdAsync(id);
        if (addressAttribute == null)
            return NotFound("No address attribute found with the specified id");

        await _addressAttributeService.DeleteAttributeAsync(addressAttribute);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteAddressAttribute",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteAddressAttribute"), addressAttribute.Id),
            addressAttribute);

        return Ok(await _localizationService.GetResourceAsync("Admin.Address.AddressAttributes.Deleted"));
    }

    #endregion

    #region Address attribute values

    [HttpPost]
    public virtual async Task<IActionResult> ValueList([FromBody] BaseQueryModel<AddressAttributeValueSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;

        //try to get an address attribute with the specified id
        var addressAttribute = await _addressAttributeService.GetAttributeByIdAsync(searchModel.AddressAttributeId);
        if (addressAttribute == null)
            return NotFound("No address attribute found with the specified id");

        //prepare model
        var model = await _addressAttributeModelFactory.PrepareAddressAttributeValueListModelAsync(searchModel, addressAttribute);

        return OkWrap(model);
    }

    [HttpGet("{addressAttributeId}")]
    public virtual async Task<IActionResult> ValueCreatePopup(int addressAttributeId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get an address attribute with the specified id
        var addressAttribute = await _addressAttributeService.GetAttributeByIdAsync(addressAttributeId);
        if (addressAttribute == null)
            return NotFound("No address attribute found with the specified id");

        //prepare model
        var model = await _addressAttributeModelFactory
            .PrepareAddressAttributeValueModelAsync(new AddressAttributeValueModel(), addressAttribute, null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ValueCreatePopup([FromBody] BaseQueryModel<AddressAttributeValueModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;

        //try to get an address attribute with the specified id
        var addressAttribute = await _addressAttributeService.GetAttributeByIdAsync(model.AttributeId);
        if (addressAttribute == null)
            return NotFound("No address attribute found with the specified id");

        if (ModelState.IsValid)
        {
            var addressAttributeValue = model.ToEntity<AddressAttributeValue>();
            await _addressAttributeService.InsertAttributeValueAsync(addressAttributeValue);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewAddressAttributeValue",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewAddressAttributeValue"), addressAttributeValue.Id),
                addressAttributeValue);

            await UpdateValueLocalesAsync(addressAttributeValue, model);

            return OkWrap(model);
        }

        //prepare model
        model = await _addressAttributeModelFactory.PrepareAddressAttributeValueModelAsync(model, addressAttribute, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> ValueEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get an address attribute value with the specified id
        var addressAttributeValue = await _addressAttributeService.GetAttributeValueByIdAsync(id);
        if (addressAttributeValue == null)
            return NotFound("No address attribute value found with the specified id");

        //try to get an address attribute with the specified id
        var addressAttribute = await _addressAttributeService.GetAttributeByIdAsync(addressAttributeValue.AttributeId);
        if (addressAttribute == null)
            return NotFound("No address attribute found with the specified id");

        //prepare model
        var model = await _addressAttributeModelFactory.PrepareAddressAttributeValueModelAsync(null, addressAttribute, addressAttributeValue);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ValueEditPopup([FromBody] BaseQueryModel<AddressAttributeValueModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;

        //try to get an address attribute value with the specified id
        var addressAttributeValue = await _addressAttributeService.GetAttributeValueByIdAsync(model.Id);
        if (addressAttributeValue == null)
            return NotFound("No address attribute value found with the specified id");

        //try to get an address attribute with the specified id
        var addressAttribute = await _addressAttributeService.GetAttributeByIdAsync(addressAttributeValue.AttributeId);
        if (addressAttribute == null)
            return NotFound("No address attribute found with the specified id");

        if (ModelState.IsValid)
        {
            addressAttributeValue = model.ToEntity(addressAttributeValue);
            await _addressAttributeService.UpdateAttributeValueAsync(addressAttributeValue);

            await UpdateValueLocalesAsync(addressAttributeValue, model);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditAddressAttributeValue",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditAddressAttributeValue"), addressAttributeValue.Id),
                addressAttributeValue);

            return OkWrap(model);
        }

        //prepare model
        model = await _addressAttributeModelFactory.PrepareAddressAttributeValueModelAsync(model, addressAttribute, addressAttributeValue, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ValueDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get an address attribute value with the specified id
        var addressAttributeValue = await _addressAttributeService.GetAttributeValueByIdAsync(id);
        if (addressAttributeValue == null)
            return NotFound("No address attribute value found with the specified id");

        await _addressAttributeService.DeleteAttributeValueAsync(addressAttributeValue);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteAddressAttributeValue",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteAddressAttributeValue"), addressAttributeValue.Id),
            addressAttributeValue);

        return Ok(defaultMessage: true);
    }

    #endregion
}