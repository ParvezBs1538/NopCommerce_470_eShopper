using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/customerrole/[action]")]
public partial class CustomerRoleApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly ICustomerRoleModelFactory _customerRoleModelFactory;
    private readonly ICustomerService _customerService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly IProductService _productService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public CustomerRoleApiController(ICustomerActivityService customerActivityService,
        ICustomerRoleModelFactory customerRoleModelFactory,
        ICustomerService customerService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IProductService productService,
        IWorkContext workContext)
    {
        _customerActivityService = customerActivityService;
        _customerRoleModelFactory = customerRoleModelFactory;
        _customerService = customerService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _productService = productService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _customerRoleModelFactory.PrepareCustomerRoleSearchModelAsync(new CustomerRoleSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<CustomerRoleSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _customerRoleModelFactory.PrepareCustomerRoleListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _customerRoleModelFactory.PrepareCustomerRoleModelAsync(new CustomerRoleModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<CustomerRoleModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var customerRole = model.ToEntity<CustomerRole>();
            await _customerService.InsertCustomerRoleAsync(customerRole);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewCustomerRole",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCustomerRole"), customerRole.Name), customerRole);

            return Created(customerRole.Id, await _localizationService.GetResourceAsync("Admin.Customers.CustomerRoles.Added"));
        }

        //prepare model
        model = await _customerRoleModelFactory.PrepareCustomerRoleModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
            return AdminApiAccessDenied();

        //try to get a customer role with the specified id
        var customerRole = await _customerService.GetCustomerRoleByIdAsync(id);
        if (customerRole == null)
            return NotFound("No customer role found with the specified id");

        //prepare model
        var model = await _customerRoleModelFactory.PrepareCustomerRoleModelAsync(null, customerRole);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<CustomerRoleModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a customer role with the specified id
        var customerRole = await _customerService.GetCustomerRoleByIdAsync(model.Id);
        if (customerRole == null)
            return NotFound("No customer role found with the specified id");

        try
        {
            if (ModelState.IsValid)
            {
                if (customerRole.IsSystemRole && !model.Active)
                    throw new NopException(await _localizationService.GetResourceAsync("Admin.Customers.CustomerRoles.Fields.Active.CantEditSystem"));

                if (customerRole.IsSystemRole && !customerRole.SystemName.Equals(model.SystemName, StringComparison.InvariantCultureIgnoreCase))
                    throw new NopException(await _localizationService.GetResourceAsync("Admin.Customers.CustomerRoles.Fields.SystemName.CantEditSystem"));

                if (NopCustomerDefaults.RegisteredRoleName.Equals(customerRole.SystemName, StringComparison.InvariantCultureIgnoreCase) &&
                    model.PurchasedWithProductId > 0)
                    throw new NopException(await _localizationService.GetResourceAsync("Admin.Customers.CustomerRoles.Fields.PurchasedWithProduct.Registered"));

                customerRole = model.ToEntity(customerRole);
                await _customerService.UpdateCustomerRoleAsync(customerRole);

                //activity log
                await _customerActivityService.InsertActivityAsync("EditCustomerRole",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCustomerRole"), customerRole.Name), customerRole);

                return Ok(await _localizationService.GetResourceAsync("Admin.Customers.CustomerRoles.Updated"));
            }

            //prepare model
            model = await _customerRoleModelFactory.PrepareCustomerRoleModelAsync(model, customerRole, true);

            //if we got this far, something failed, redisplay form
            return BadRequestWrap(model, ModelState);
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
            return AdminApiAccessDenied();

        //try to get a customer role with the specified id
        var customerRole = await _customerService.GetCustomerRoleByIdAsync(id);
        if (customerRole == null)
            return NotFound("No customer role found with the specified id");

        try
        {
            await _customerService.DeleteCustomerRoleAsync(customerRole);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteCustomerRole",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCustomerRole"), customerRole.Name), customerRole);

            return Ok(await _localizationService.GetResourceAsync("Admin.Customers.CustomerRoles.Deleted"));
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }

    public virtual async Task<IActionResult> AssociateProductToCustomerRolePopup()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _customerRoleModelFactory.PrepareCustomerRoleProductSearchModelAsync(new CustomerRoleProductSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AssociateProductToCustomerRolePopupList([FromBody] BaseQueryModel<CustomerRoleProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _customerRoleModelFactory.PrepareCustomerRoleProductListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AssociateProductToCustomerRolePopup([FromBody] BaseQueryModel<AddProductToCustomerRoleModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product with the specified id
        var associatedProduct = await _productService.GetProductByIdAsync(model.AssociatedToProductId);
        if (associatedProduct == null)
            return NotFound("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && associatedProduct.VendorId != currentVendor.Id)
            return BadRequest("This is not your product");

        return Ok(defaultMessage: true);
    }

    #endregion
}