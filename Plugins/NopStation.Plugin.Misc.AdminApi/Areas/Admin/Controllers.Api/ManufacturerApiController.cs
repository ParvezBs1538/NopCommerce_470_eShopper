using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Http.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/manufacturer/[action]")]
public partial class ManufacturerApiController : BaseAdminApiController
{
    #region Fields

    private readonly IAclService _aclService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ICustomerService _customerService;
    private readonly IDiscountService _discountService;
    private readonly IExportManager _exportManager;
    private readonly IImportManager _importManager;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IManufacturerModelFactory _manufacturerModelFactory;
    private readonly IManufacturerService _manufacturerService;
    private readonly IPermissionService _permissionService;
    private readonly IPictureService _pictureService;
    private readonly IProductService _productService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public ManufacturerApiController(IAclService aclService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        IDiscountService discountService,
        IExportManager exportManager,
        IImportManager importManager,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IManufacturerModelFactory manufacturerModelFactory,
        IManufacturerService manufacturerService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IProductService productService,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        IUrlRecordService urlRecordService,
        IWorkContext workContext)
    {
        _aclService = aclService;
        _customerActivityService = customerActivityService;
        _customerService = customerService;
        _discountService = discountService;
        _exportManager = exportManager;
        _importManager = importManager;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _manufacturerModelFactory = manufacturerModelFactory;
        _manufacturerService = manufacturerService;
        _permissionService = permissionService;
        _pictureService = pictureService;
        _productService = productService;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
        _urlRecordService = urlRecordService;
        _workContext = workContext;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(Manufacturer manufacturer, ManufacturerModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(manufacturer,
                x => x.Name,
                localized.Name,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(manufacturer,
                x => x.Description,
                localized.Description,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(manufacturer,
                x => x.MetaKeywords,
                localized.MetaKeywords,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(manufacturer,
                x => x.MetaDescription,
                localized.MetaDescription,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(manufacturer,
                x => x.MetaTitle,
                localized.MetaTitle,
                localized.LanguageId);

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(manufacturer, localized.SeName, localized.Name, false);
            await _urlRecordService.SaveSlugAsync(manufacturer, seName, localized.LanguageId);
        }
    }

    protected virtual async Task UpdatePictureSeoNamesAsync(Manufacturer manufacturer)
    {
        var picture = await _pictureService.GetPictureByIdAsync(manufacturer.PictureId);
        if (picture != null)
            await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(manufacturer.Name));
    }

    protected virtual async Task SaveManufacturerAclAsync(Manufacturer manufacturer, ManufacturerModel model)
    {
        manufacturer.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
        await _manufacturerService.UpdateManufacturerAsync(manufacturer);

        var existingAclRecords = await _aclService.GetAclRecordsAsync(manufacturer);
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        foreach (var customerRole in allCustomerRoles)
        {
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
            {
                //new role
                if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                    await _aclService.InsertAclRecordAsync(manufacturer, customerRole.Id);
            }
            else
            {
                //remove role
                var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                if (aclRecordToDelete != null)
                    await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
            }
        }
    }

    protected virtual async Task SaveStoreMappingsAsync(Manufacturer manufacturer, ManufacturerModel model)
    {
        manufacturer.LimitedToStores = model.SelectedStoreIds.Any();
        await _manufacturerService.UpdateManufacturerAsync(manufacturer);

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(manufacturer);
        var allStores = await _storeService.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                    await _storeMappingService.InsertStoreMappingAsync(manufacturer, store.Id);
            }
            else
            {
                //remove store
                var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                if (storeMappingToDelete != null)
                    await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
            }
        }
    }

    #endregion

    #region List

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _manufacturerModelFactory.PrepareManufacturerSearchModelAsync(new ManufacturerSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<ManufacturerSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _manufacturerModelFactory.PrepareManufacturerListModelAsync(searchModel);

        return OkWrap(model);
    }

    #endregion

    #region Create / Edit / Delete

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _manufacturerModelFactory.PrepareManufacturerModelAsync(new ManufacturerModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<ManufacturerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var manufacturer = model.ToEntity<Manufacturer>();
            manufacturer.CreatedOnUtc = DateTime.UtcNow;
            manufacturer.UpdatedOnUtc = DateTime.UtcNow;
            await _manufacturerService.InsertManufacturerAsync(manufacturer);

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(manufacturer, model.SeName, manufacturer.Name, true);
            await _urlRecordService.SaveSlugAsync(manufacturer, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(manufacturer, model);

            //discounts
            var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToManufacturers, showHidden: true, isActive: null);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    //manufacturer.AppliedDiscounts.Add(discount);
                    await _manufacturerService.InsertDiscountManufacturerMappingAsync(new DiscountManufacturerMapping { EntityId = manufacturer.Id, DiscountId = discount.Id });

            }

            await _manufacturerService.UpdateManufacturerAsync(manufacturer);

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(manufacturer);

            //ACL (customer roles)
            await SaveManufacturerAclAsync(manufacturer, model);

            //stores
            await SaveStoreMappingsAsync(manufacturer, model);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewManufacturer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewManufacturer"), manufacturer.Name), manufacturer);

            return Created(manufacturer.Id, await _localizationService.GetResourceAsync("Admin.Catalog.Manufacturers.Added"));
        }

        //prepare model
        model = await _manufacturerModelFactory.PrepareManufacturerModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        //try to get a manufacturer with the specified id
        var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(id);
        if (manufacturer == null || manufacturer.Deleted)
            return NotFound("No manufacturer found with the specified id");

        //prepare model
        var model = await _manufacturerModelFactory.PrepareManufacturerModelAsync(null, manufacturer);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<ManufacturerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a manufacturer with the specified id
        var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(model.Id);
        if (manufacturer == null || manufacturer.Deleted)
            return NotFound("No manufacturer found with the specified id");

        if (ModelState.IsValid)
        {
            var prevPictureId = manufacturer.PictureId;
            manufacturer = model.ToEntity(manufacturer);
            manufacturer.UpdatedOnUtc = DateTime.UtcNow;
            await _manufacturerService.UpdateManufacturerAsync(manufacturer);

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(manufacturer, model.SeName, manufacturer.Name, true);
            await _urlRecordService.SaveSlugAsync(manufacturer, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(manufacturer, model);

            //discounts
            var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToManufacturers, showHidden: true, isActive: null);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (await _manufacturerService.GetDiscountAppliedToManufacturerAsync(manufacturer.Id, discount.Id) is null)
                        await _manufacturerService.InsertDiscountManufacturerMappingAsync(new DiscountManufacturerMapping { EntityId = manufacturer.Id, DiscountId = discount.Id });
                }
                else
                {
                    //remove discount
                    if (await _manufacturerService.GetDiscountAppliedToManufacturerAsync(manufacturer.Id, discount.Id) is DiscountManufacturerMapping discountManufacturerMapping)
                        await _manufacturerService.DeleteDiscountManufacturerMappingAsync(discountManufacturerMapping);
                }
            }

            await _manufacturerService.UpdateManufacturerAsync(manufacturer);

            //delete an old picture (if deleted or updated)
            if (prevPictureId > 0 && prevPictureId != manufacturer.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureByIdAsync(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePictureAsync(prevPicture);
            }

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(manufacturer);

            //ACL
            await SaveManufacturerAclAsync(manufacturer, model);

            //stores
            await SaveStoreMappingsAsync(manufacturer, model);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditManufacturer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditManufacturer"), manufacturer.Name), manufacturer);

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Manufacturers.Updated"));
        }

        //prepare model
        model = await _manufacturerModelFactory.PrepareManufacturerModelAsync(model, manufacturer, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        //try to get a manufacturer with the specified id
        var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(id);
        if (manufacturer == null)
            return NotFound("No manufacturer found with the specified id");

        await _manufacturerService.DeleteManufacturerAsync(manufacturer);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteManufacturer",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteManufacturer"), manufacturer.Name), manufacturer);

        return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Manufacturers.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return Ok(defaultMessage: true);

        var manufacturers = await _manufacturerService.GetManufacturersByIdsAsync(selectedIds.ToArray());
        await _manufacturerService.DeleteManufacturersAsync(manufacturers);

        var locale = await _localizationService.GetResourceAsync("ActivityLog.DeleteManufacturer");
        foreach (var manufacturer in manufacturers)
        {
            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteManufacturer", string.Format(locale, manufacturer.Name), manufacturer);
        }

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Export / Import

    public virtual async Task<IActionResult> ExportXml()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        try
        {
            var manufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: true);
            var xml = await _exportManager.ExportManufacturersToXmlAsync(manufacturers);
            return File(Encoding.UTF8.GetBytes(xml), MimeTypes.ApplicationXml, "manufacturers.xml");
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    public virtual async Task<IActionResult> ExportXlsx()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        try
        {
            var manufacturers = (await _manufacturerService.GetAllManufacturersAsync(showHidden: true)).Where(p => !p.Deleted);
            var bytes = await _exportManager.ExportManufacturersToXlsxAsync(manufacturers);

            return File(bytes, MimeTypes.TextXlsx, "manufacturers.xlsx");
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ImportFromXlsx()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        //a vendor cannot import manufacturers
        if (await _workContext.GetCurrentVendorAsync() != null)
            return AdminApiAccessDenied();

        try
        {
            var importexcelfile = await Request.GetFirstOrDefaultFileAsync();
            if (importexcelfile != null && importexcelfile.Length > 0)
            {
                await _importManager.ImportManufacturersFromXlsxAsync(importexcelfile.OpenReadStream());
            }
            else
            {
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
            }

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Manufacturers.Imported"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    #endregion

    #region Products

    [HttpPost]
    public virtual async Task<IActionResult> ProductList([FromBody] BaseQueryModel<ManufacturerProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a manufacturer with the specified id
        var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(searchModel.ManufacturerId);
        if (manufacturer == null)
            return NotFound("No manufacturer found with the specified id");

        //prepare model
        var model = await _manufacturerModelFactory.PrepareManufacturerProductListModelAsync(searchModel, manufacturer);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductUpdate([FromBody] BaseQueryModel<ManufacturerProductModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product manufacturer with the specified id
        var productManufacturer = await _manufacturerService.GetProductManufacturerByIdAsync(model.Id);
        if (productManufacturer == null)
            return NotFound("No product manufacturer found with the specified id");

        //fill entity from model
        productManufacturer = model.ToEntity(productManufacturer);
        await _manufacturerService.UpdateProductManufacturerAsync(productManufacturer);

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ProductDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        //try to get a product manufacturer with the specified id
        var productManufacturer = await _manufacturerService.GetProductManufacturerByIdAsync(id);
        if (productManufacturer == null)
            return NotFound("No product manufacturer found with the specified id");

        await _manufacturerService.DeleteProductManufacturerAsync(productManufacturer);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{manufacturerId}")]
    public virtual async Task<IActionResult> ProductAddPopup(int manufacturerId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _manufacturerModelFactory.PrepareAddProductToManufacturerSearchModelAsync(new AddProductToManufacturerSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAddPopupList([FromBody] BaseQueryModel<AddProductToManufacturerSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _manufacturerModelFactory.PrepareAddProductToManufacturerListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAddPopup([FromBody] BaseQueryModel<AddProductToManufacturerModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //get selected products
        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        if (selectedProducts.Any())
        {
            var existingProductManufacturers = await _manufacturerService
                .GetProductManufacturersByManufacturerIdAsync(model.ManufacturerId, showHidden: true);
            foreach (var product in selectedProducts)
            {
                //whether product manufacturer with such parameters already exists
                if (_manufacturerService.FindProductManufacturer(existingProductManufacturers, product.Id, model.ManufacturerId) != null)
                    continue;

                //insert the new product manufacturer mapping
                await _manufacturerService.InsertProductManufacturerAsync(new ProductManufacturer
                {
                    ManufacturerId = model.ManufacturerId,
                    ProductId = product.Id,
                    IsFeaturedProduct = false,
                    DisplayOrder = 1
                });
            }
        }

        return Ok(defaultMessage: true);
    }

    #endregion
}