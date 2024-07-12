using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Common;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Affiliates;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/affiliate/[action]")]
public partial class AffiliateApiController : BaseAdminApiController
{
    #region Fields

    private readonly IAddressService _addressService;
    private readonly IAffiliateModelFactory _affiliateModelFactory;
    private readonly IAffiliateService _affiliateService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public AffiliateApiController(IAddressService addressService,
        IAffiliateModelFactory affiliateModelFactory,
        IAffiliateService affiliateService,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        IPermissionService permissionService)
    {
        _addressService = addressService;
        _affiliateModelFactory = affiliateModelFactory;
        _affiliateService = affiliateService;
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAffiliates))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _affiliateModelFactory.PrepareAffiliateSearchModelAsync(new AffiliateSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<AffiliateSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAffiliates))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _affiliateModelFactory.PrepareAffiliateListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAffiliates))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _affiliateModelFactory.PrepareAffiliateModelAsync(new AffiliateModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<AffiliateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAffiliates))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var address = model.Address.ToEntity<Address>();
            address.CreatedOnUtc = DateTime.UtcNow;

            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;

            await _addressService.InsertAddressAsync(address);

            var affiliate = model.ToEntity<Affiliate>();

            //validate friendly URL name
            var friendlyUrlName = await _affiliateService.ValidateFriendlyUrlNameAsync(affiliate, model.FriendlyUrlName);
            affiliate.FriendlyUrlName = friendlyUrlName;
            affiliate.AddressId = address.Id;

            await _affiliateService.InsertAffiliateAsync(affiliate);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewAffiliate",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewAffiliate"), affiliate.Id), affiliate);

            return Created(affiliate.Id, await _localizationService.GetResourceAsync("Admin.Affiliates.Added"));
        }

        //prepare model
        model = await _affiliateModelFactory.PrepareAffiliateModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAffiliates))
            return AdminApiAccessDenied();

        //try to get an affiliate with the specified id
        var affiliate = await _affiliateService.GetAffiliateByIdAsync(id);
        if (affiliate == null || affiliate.Deleted)
            return NotFound("No affiliate found with the specified id");

        //prepare model
        var model = await _affiliateModelFactory.PrepareAffiliateModelAsync(null, affiliate);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<AffiliateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAffiliates))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get an affiliate with the specified id
        var affiliate = await _affiliateService.GetAffiliateByIdAsync(model.Id);
        if (affiliate == null || affiliate.Deleted)
            return NotFound("No affiliate found with the specified id");

        if (ModelState.IsValid)
        {
            var address = await _addressService.GetAddressByIdAsync(affiliate.AddressId);
            address = model.Address.ToEntity(address);

            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;

            await _addressService.UpdateAddressAsync(address);

            affiliate = model.ToEntity(affiliate);

            //validate friendly URL name
            var friendlyUrlName = await _affiliateService.ValidateFriendlyUrlNameAsync(affiliate, model.FriendlyUrlName);
            affiliate.FriendlyUrlName = friendlyUrlName;
            affiliate.AddressId = address.Id;

            await _affiliateService.UpdateAffiliateAsync(affiliate);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditAffiliate",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditAffiliate"), affiliate.Id), affiliate);

            return Ok(await _localizationService.GetResourceAsync("Admin.Affiliates.Updated"));
        }

        //prepare model
        model = await _affiliateModelFactory.PrepareAffiliateModelAsync(model, affiliate, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    //delete
    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAffiliates))
            return AdminApiAccessDenied();

        //try to get an affiliate with the specified id
        var affiliate = await _affiliateService.GetAffiliateByIdAsync(id);
        if (affiliate == null)
            return NotFound("No affiliate found with the specified id");

        await _affiliateService.DeleteAffiliateAsync(affiliate);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteAffiliate",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteAffiliate"), affiliate.Id), affiliate);

        return Ok(await _localizationService.GetResourceAsync("Admin.Affiliates.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> AffiliatedOrderListGrid([FromBody] BaseQueryModel<AffiliatedOrderSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAffiliates))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get an affiliate with the specified id
        var affiliate = await _affiliateService.GetAffiliateByIdAsync(searchModel.AffliateId);
        if (affiliate == null)
            return NotFound("No affiliate found with the specified id");

        //prepare model
        var model = await _affiliateModelFactory.PrepareAffiliatedOrderListModelAsync(searchModel, affiliate);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AffiliatedCustomerList([FromBody] BaseQueryModel<AffiliatedCustomerSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAffiliates))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get an affiliate with the specified id
        var affiliate = await _affiliateService.GetAffiliateByIdAsync(searchModel.AffliateId);
        if (affiliate == null)
            return NotFound("No affiliate found with the specified id");

        //prepare model
        var model = await _affiliateModelFactory.PrepareAffiliatedCustomerListModelAsync(searchModel, affiliate);

        return OkWrap(model);
    }

    #endregion
}