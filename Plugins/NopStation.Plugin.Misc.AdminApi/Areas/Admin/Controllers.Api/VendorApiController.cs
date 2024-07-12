using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Vendors;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Vendors;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/vendor/[action]")]
public partial class VendorApiController : BaseAdminApiController
{
    #region Fields

    private readonly IAddressService _addressService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ICustomerService _customerService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IPermissionService _permissionService;
    private readonly IPictureService _pictureService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
    private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _addressAttributeService;
    private readonly IAttributeParser<VendorAttribute, VendorAttributeValue> _vendorAttributeParser;
    private readonly IAttributeService<VendorAttribute, VendorAttributeValue> _vendorAttributeService;
    private readonly IVendorModelFactory _vendorModelFactory;
    private readonly IVendorService _vendorService;

    #endregion

    #region Ctor

    public VendorApiController(IAddressService addressService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IUrlRecordService urlRecordService,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService,
        IAttributeParser<VendorAttribute, VendorAttributeValue> vendorAttributeParser,
        IAttributeService<VendorAttribute, VendorAttributeValue> vendorAttributeService,
        IVendorModelFactory vendorModelFactory,
        IVendorService vendorService)
    {
        _addressService = addressService;
        _customerActivityService = customerActivityService;
        _customerService = customerService;
        _genericAttributeService = genericAttributeService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _permissionService = permissionService;
        _pictureService = pictureService;
        _urlRecordService = urlRecordService;
        _addressAttributeParser = addressAttributeParser;
        _addressAttributeService = addressAttributeService;
        _vendorAttributeParser = vendorAttributeParser;
        _vendorAttributeService = vendorAttributeService;
        _vendorModelFactory = vendorModelFactory;
        _vendorService = vendorService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdatePictureSeoNamesAsync(Vendor vendor)
    {
        var picture = await _pictureService.GetPictureByIdAsync(vendor.PictureId);
        if (picture != null)
            await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(vendor.Name));
    }

    protected virtual async Task UpdateLocalesAsync(Vendor vendor, VendorModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(vendor,
                x => x.Name,
                localized.Name,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(vendor,
                x => x.Description,
                localized.Description,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(vendor,
                x => x.MetaKeywords,
                localized.MetaKeywords,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(vendor,
                x => x.MetaDescription,
                localized.MetaDescription,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(vendor,
                x => x.MetaTitle,
                localized.MetaTitle,
                localized.LanguageId);

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(vendor, localized.SeName, localized.Name, false);
            await _urlRecordService.SaveSlugAsync(vendor, seName, localized.LanguageId);
        }
    }

    protected virtual async Task<string> ParseVendorAttributesAsync(NameValueCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);

        var attributesXml = string.Empty;
        var vendorAttributes = await _vendorAttributeService.GetAllAttributesAsync();
        foreach (var attribute in vendorAttributes)
        {
            var controlId = $"{NopVendorDefaults.VendorAttributePrefix}{attribute.Id}";
            StringValues ctrlAttributes;
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                    ctrlAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                    {
                        var selectedAttributeId = int.Parse(ctrlAttributes);
                        if (selectedAttributeId > 0)
                            attributesXml = _vendorAttributeParser.AddAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                    }

                    break;
                case AttributeControlType.Checkboxes:
                    var cblAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(cblAttributes))
                    {
                        foreach (var item in cblAttributes.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries))
                        {
                            var selectedAttributeId = int.Parse(item);
                            if (selectedAttributeId > 0)
                                attributesXml = _vendorAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }

                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                    //load read-only (already server-side selected) values
                    var attributeValues = await _vendorAttributeService.GetAttributeValuesAsync(attribute.Id);
                    foreach (var selectedAttributeId in attributeValues
                        .Where(v => v.IsPreSelected)
                        .Select(v => v.Id)
                        .ToList())
                    {
                        attributesXml = _vendorAttributeParser.AddAttribute(attributesXml,
                            attribute, selectedAttributeId.ToString());
                    }

                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    ctrlAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                    {
                        var enteredText = ctrlAttributes.ToString().Trim();
                        attributesXml = _vendorAttributeParser.AddAttribute(attributesXml,
                            attribute, enteredText);
                    }

                    break;
                case AttributeControlType.Datepicker:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                case AttributeControlType.FileUpload:
                //not supported vendor attributes
                default:
                    break;
            }
        }

        return attributesXml;
    }

    #endregion

    #region Vendors

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _vendorModelFactory.PrepareVendorSearchModelAsync(new VendorSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<VendorSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _vendorModelFactory.PrepareVendorListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _vendorModelFactory.PrepareVendorModelAsync(new VendorModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<VendorModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var form = queryModel.FormValues.ToNameValueCollection();

        //parse vendor attributes
        var vendorAttributesXml = await ParseVendorAttributesAsync(form);
        var warnings = (await _vendorAttributeParser.GetAttributeWarningsAsync(vendorAttributesXml)).ToList();
        foreach (var warning in warnings)
        {
            ModelState.AddModelError(string.Empty, warning);
        }

        if (ModelState.IsValid)
        {
            var vendor = model.ToEntity<Vendor>();
            await _vendorService.InsertVendorAsync(vendor);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewVendor",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewVendor"), vendor.Id), vendor);

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(vendor, model.SeName, vendor.Name, true);
            await _urlRecordService.SaveSlugAsync(vendor, model.SeName, 0);

            //address
            var address = model.Address.ToEntity<Address>();
            address.CreatedOnUtc = DateTime.UtcNow;

            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;
            await _addressService.InsertAddressAsync(address);
            vendor.AddressId = address.Id;
            await _vendorService.UpdateVendorAsync(vendor);

            //vendor attributes
            await _genericAttributeService.SaveAttributeAsync(vendor, NopVendorDefaults.VendorAttributes, vendorAttributesXml);

            //locales
            await UpdateLocalesAsync(vendor, model);

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(vendor);

            return Created(vendor.Id, await _localizationService.GetResourceAsync("Admin.Vendors.Added"));
        }

        //prepare model
        model = await _vendorModelFactory.PrepareVendorModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
            return AdminApiAccessDenied();

        //try to get a vendor with the specified id
        var vendor = await _vendorService.GetVendorByIdAsync(id);
        if (vendor == null || vendor.Deleted)
            return NotFound("No vendor found with the specified id");

        //prepare model
        var model = await _vendorModelFactory.PrepareVendorModelAsync(null, vendor);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<VendorModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var form = queryModel.FormValues.ToNameValueCollection();

        //try to get a vendor with the specified id
        var vendor = await _vendorService.GetVendorByIdAsync(model.Id);
        if (vendor == null || vendor.Deleted)
            return NotFound("No vendor found with the specified id");

        //parse vendor attributes
        var vendorAttributesXml = await ParseVendorAttributesAsync(form);
        var warnings = (await _vendorAttributeParser.GetAttributeWarningsAsync(vendorAttributesXml)).ToList();
        foreach (var warning in warnings)
        {
            ModelState.AddModelError(string.Empty, warning);
        }

        //custom address attributes
        var customAttributes = await form.ParseCustomAddressAttributesAsync(_addressAttributeParser, _addressAttributeService);
        var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
        foreach (var error in customAttributeWarnings)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        if (ModelState.IsValid)
        {
            var prevPictureId = vendor.PictureId;
            vendor = model.ToEntity(vendor);
            await _vendorService.UpdateVendorAsync(vendor);

            //vendor attributes
            await _genericAttributeService.SaveAttributeAsync(vendor, NopVendorDefaults.VendorAttributes, vendorAttributesXml);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditVendor",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditVendor"), vendor.Id), vendor);

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(vendor, model.SeName, vendor.Name, true);
            await _urlRecordService.SaveSlugAsync(vendor, model.SeName, 0);

            //address
            var address = await _addressService.GetAddressByIdAsync(vendor.AddressId);
            if (address == null)
            {
                address = model.Address.ToEntity<Address>();
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;

                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                await _addressService.InsertAddressAsync(address);
                vendor.AddressId = address.Id;
                await _vendorService.UpdateVendorAsync(vendor);
            }
            else
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;

                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                await _addressService.UpdateAddressAsync(address);
            }

            //locales
            await UpdateLocalesAsync(vendor, model);

            //delete an old picture (if deleted or updated)
            if (prevPictureId > 0 && prevPictureId != vendor.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureByIdAsync(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePictureAsync(prevPicture);
            }
            //update picture seo file name
            await UpdatePictureSeoNamesAsync(vendor);

            return Ok(await _localizationService.GetResourceAsync("Admin.Vendors.Updated"));
        }

        //prepare model
        model = await _vendorModelFactory.PrepareVendorModelAsync(model, vendor, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
            return AdminApiAccessDenied();

        //try to get a vendor with the specified id
        var vendor = await _vendorService.GetVendorByIdAsync(id);
        if (vendor == null)
            return NotFound("No vendor found with the specified id");

        //clear associated customer references
        var associatedCustomers = await _customerService.GetAllCustomersAsync(vendorId: vendor.Id);
        foreach (var customer in associatedCustomers)
        {
            customer.VendorId = 0;
            await _customerService.UpdateCustomerAsync(customer);
        }

        //delete a vendor
        await _vendorService.DeleteVendorAsync(vendor);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteVendor",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteVendor"), vendor.Id), vendor);

        return Ok(await _localizationService.GetResourceAsync("Admin.Vendors.Deleted"));
    }

    #endregion

    #region Vendor notes

    [HttpPost]
    public virtual async Task<IActionResult> VendorNotesSelect([FromBody] BaseQueryModel<VendorNoteSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a vendor with the specified id
        var vendor = await _vendorService.GetVendorByIdAsync(searchModel.VendorId);
        if (vendor == null)
            return NotFound("No vendor found with the specified id");

        //prepare model
        var model = await _vendorModelFactory.PrepareVendorNoteListModelAsync(searchModel, vendor);

        return OkWrap(model);
    }

    [HttpGet("{vendorId}")]
    public virtual async Task<IActionResult> VendorNoteAdd(int vendorId, [FromBody] BaseQueryModel<string> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
            return AdminApiAccessDenied();

        var message = queryModel.Data;
        if (string.IsNullOrEmpty(message))
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Vendors.VendorNotes.Fields.Note.Validation"));

        //try to get a vendor with the specified id
        var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
        if (vendor == null)
            return NotFound("No vendor found with the specified id");

        await _vendorService.InsertVendorNoteAsync(new VendorNote
        {
            Note = message,
            CreatedOnUtc = DateTime.UtcNow,
            VendorId = vendor.Id
        });

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> VendorNoteDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
            return AdminApiAccessDenied();

        //try to get a vendor note with the specified id
        var vendorNote = await _vendorService.GetVendorNoteByIdAsync(id);
        if (vendorNote == null)
            return NotFound();

        await _vendorService.DeleteVendorNoteAsync(vendorNote);

        return Ok(defaultMessage: true);
    }

    #endregion
}