using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Services.Attributes;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/customerattribute/[action]")]
public partial class CustomerAttributeApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly ICustomerAttributeModelFactory _customerAttributeModelFactory;
    private readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public CustomerAttributeApiController(ICustomerActivityService customerActivityService,
        ICustomerAttributeModelFactory customerAttributeModelFactory,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IPermissionService permissionService)
    {
        _customerActivityService = customerActivityService;
        _customerAttributeModelFactory = customerAttributeModelFactory;
        _customerAttributeService = customerAttributeService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _permissionService = permissionService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateAttributeLocalesAsync(CustomerAttribute customerAttribute, CustomerAttributeModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(customerAttribute,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    protected virtual async Task UpdateValueLocalesAsync(CustomerAttributeValue customerAttributeValue, CustomerAttributeValueModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(customerAttributeValue,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    #endregion

    #region Customer attributes

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<CustomerAttributeSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _customerAttributeModelFactory.PrepareCustomerAttributeListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _customerAttributeModelFactory.PrepareCustomerAttributeModelAsync(new CustomerAttributeModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<CustomerAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var customerAttribute = model.ToEntity<CustomerAttribute>();
            await _customerAttributeService.InsertAttributeAsync(customerAttribute);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewCustomerAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCustomerAttribute"), customerAttribute.Id),
                customerAttribute);

            //locales
            await UpdateAttributeLocalesAsync(customerAttribute, model);

            return Created(customerAttribute.Id, await _localizationService.GetResourceAsync("Admin.Customers.CustomerAttributes.Added"));
        }

        //prepare model
        model = await _customerAttributeModelFactory.PrepareCustomerAttributeModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a customer attribute with the specified id
        var customerAttribute = await _customerAttributeService.GetAttributeByIdAsync(id);
        if (customerAttribute == null)
            return NotFound("No customer attribute found with the specified id");

        //prepare model
        var model = await _customerAttributeModelFactory.PrepareCustomerAttributeModelAsync(null, customerAttribute);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<CustomerAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var customerAttribute = await _customerAttributeService.GetAttributeByIdAsync(model.Id);
        if (customerAttribute == null)
            //no customer attribute found with the specified id
            return NotFound("No customer attribute found with the specified id");

        if (!ModelState.IsValid)
            //if we got this far, something failed, redisplay form
            return BadRequestWrap(model, ModelState);

        customerAttribute = model.ToEntity(customerAttribute);
        await _customerAttributeService.UpdateAttributeAsync(customerAttribute);

        //activity log
        await _customerActivityService.InsertActivityAsync("EditCustomerAttribute",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCustomerAttribute"), customerAttribute.Id),
            customerAttribute);

        //locales
        await UpdateAttributeLocalesAsync(customerAttribute, model);

        return Created(customerAttribute.Id, await _localizationService.GetResourceAsync("Admin.Customers.CustomerAttributes.Updated"));
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var customerAttribute = await _customerAttributeService.GetAttributeByIdAsync(id);
        await _customerAttributeService.DeleteAttributeAsync(customerAttribute);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteCustomerAttribute",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCustomerAttribute"), customerAttribute.Id),
            customerAttribute);

        return Ok(await _localizationService.GetResourceAsync("Admin.Customers.CustomerAttributes.Deleted"));
    }

    #endregion

    #region Customer attribute values

    [HttpPost]
    public virtual async Task<IActionResult> ValueList([FromBody] BaseQueryModel<CustomerAttributeValueSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a customer attribute with the specified id
        var customerAttribute = await _customerAttributeService.GetAttributeByIdAsync(searchModel.CustomerAttributeId);

        //prepare model
        var model = await _customerAttributeModelFactory.PrepareCustomerAttributeValueListModelAsync(searchModel, customerAttribute);

        return OkWrap(model);
    }

    [HttpGet("{customerAttributeId}")]
    public virtual async Task<IActionResult> ValueCreatePopup(int customerAttributeId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a customer attribute with the specified id
        var customerAttribute = await _customerAttributeService.GetAttributeByIdAsync(customerAttributeId);
        if (customerAttribute == null)
            return NotFound("No customer attribute found with the specified id");

        //prepare model
        var model = await _customerAttributeModelFactory
            .PrepareCustomerAttributeValueModelAsync(new CustomerAttributeValueModel(), customerAttribute, null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ValueCreatePopup([FromBody] BaseQueryModel<CustomerAttributeValueModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a customer attribute with the specified id
        var customerAttribute = await _customerAttributeService.GetAttributeByIdAsync(model.AttributeId);
        if (customerAttribute == null)
            return NotFound("No customer attribute found with the specified id");

        if (ModelState.IsValid)
        {
            var cav = model.ToEntity<CustomerAttributeValue>();
            await _customerAttributeService.InsertAttributeValueAsync(cav);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewCustomerAttributeValue",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCustomerAttributeValue"), cav.Id), cav);

            await UpdateValueLocalesAsync(cav, model);

            return OkWrap(model);
        }

        //prepare model
        model = await _customerAttributeModelFactory.PrepareCustomerAttributeValueModelAsync(model, customerAttribute, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> ValueEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a customer attribute value with the specified id
        var customerAttributeValue = await _customerAttributeService.GetAttributeValueByIdAsync(id);
        if (customerAttributeValue == null)
            return NotFound("No customer attribute value found with the specified id");

        //try to get a customer attribute with the specified id
        var customerAttribute = await _customerAttributeService.GetAttributeByIdAsync(customerAttributeValue.AttributeId);
        if (customerAttribute == null)
            return NotFound("No customer attribute found with the specified id");

        //prepare model
        var model = await _customerAttributeModelFactory.PrepareCustomerAttributeValueModelAsync(null, customerAttribute, customerAttributeValue);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ValueEditPopup([FromBody] BaseQueryModel<CustomerAttributeValueModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a customer attribute value with the specified id
        var customerAttributeValue = await _customerAttributeService.GetAttributeValueByIdAsync(model.Id);
        if (customerAttributeValue == null)
            return NotFound("No customer attribute value found with the specified id");

        //try to get a customer attribute with the specified id
        var customerAttribute = await _customerAttributeService.GetAttributeByIdAsync(customerAttributeValue.AttributeId);
        if (customerAttribute == null)
            return NotFound("No customer attribute found with the specified id");

        if (ModelState.IsValid)
        {
            customerAttributeValue = model.ToEntity(customerAttributeValue);
            await _customerAttributeService.UpdateAttributeValueAsync(customerAttributeValue);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditCustomerAttributeValue",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCustomerAttributeValue"), customerAttributeValue.Id),
                customerAttributeValue);

            await UpdateValueLocalesAsync(customerAttributeValue, model);

            return OkWrap(model);
        }

        //prepare model
        model = await _customerAttributeModelFactory.PrepareCustomerAttributeValueModelAsync(model, customerAttribute, customerAttributeValue, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ValueDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a customer attribute value with the specified id
        var customerAttributeValue = await _customerAttributeService.GetAttributeValueByIdAsync(id);
        if (customerAttributeValue == null)
            return NotFound("No customer attribute value found with the specified id");

        await _customerAttributeService.DeleteAttributeValueAsync(customerAttributeValue);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteCustomerAttributeValue",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCustomerAttributeValue"), customerAttributeValue.Id),
            customerAttributeValue);

        return Ok(defaultMessage: true);
    }

    #endregion
}