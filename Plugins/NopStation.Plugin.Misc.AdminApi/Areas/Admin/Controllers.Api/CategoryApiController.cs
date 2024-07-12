using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
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

[Route("api/a/category/[action]")]
public partial class CategoryApiController : BaseAdminApiController
{
    #region Fields

    private readonly IAclService _aclService;
    private readonly ICategoryModelFactory _categoryModelFactory;
    private readonly ICategoryService _categoryService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ICustomerService _customerService;
    private readonly IDiscountService _discountService;
    private readonly IExportManager _exportManager;
    private readonly IImportManager _importManager;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IPermissionService _permissionService;
    private readonly IPictureService _pictureService;
    private readonly IProductService _productService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStoreService _storeService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IWorkContext _workContext;
    private readonly ILogger _logger;

    #endregion

    #region Ctor

    public CategoryApiController(IAclService aclService,
        ICategoryModelFactory categoryModelFactory,
        ICategoryService categoryService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        IDiscountService discountService,
        IExportManager exportManager,
        IImportManager importManager,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IProductService productService,
        IStaticCacheManager staticCacheManager,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        IUrlRecordService urlRecordService,
        IWorkContext workContext,
        ILogger logger)
    {
        _aclService = aclService;
        _categoryModelFactory = categoryModelFactory;
        _categoryService = categoryService;
        _customerActivityService = customerActivityService;
        _customerService = customerService;
        _discountService = discountService;
        _exportManager = exportManager;
        _importManager = importManager;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _permissionService = permissionService;
        _pictureService = pictureService;
        _productService = productService;
        _staticCacheManager = staticCacheManager;
        _storeMappingService = storeMappingService;
        _storeService = storeService;
        _urlRecordService = urlRecordService;
        _workContext = workContext;
        _logger = logger;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(Category category, CategoryModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(category,
                x => x.Name,
                localized.Name,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(category,
                x => x.Description,
                localized.Description,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(category,
                x => x.MetaKeywords,
                localized.MetaKeywords,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(category,
                x => x.MetaDescription,
                localized.MetaDescription,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(category,
                x => x.MetaTitle,
                localized.MetaTitle,
                localized.LanguageId);

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(category, localized.SeName, localized.Name, false);
            await _urlRecordService.SaveSlugAsync(category, seName, localized.LanguageId);
        }
    }

    protected virtual async Task UpdatePictureSeoNamesAsync(Category category)
    {
        var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
        if (picture != null)
            await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(category.Name));
    }

    protected virtual async Task SaveCategoryAclAsync(Category category, CategoryModel model)
    {
        category.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
        await _categoryService.UpdateCategoryAsync(category);

        var existingAclRecords = await _aclService.GetAclRecordsAsync(category);
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        foreach (var customerRole in allCustomerRoles)
        {
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
            {
                //new role
                if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                    await _aclService.InsertAclRecordAsync(category, customerRole.Id);
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

    protected virtual async Task SaveStoreMappingsAsync(Category category, CategoryModel model)
    {
        category.LimitedToStores = model.SelectedStoreIds.Any();
        await _categoryService.UpdateCategoryAsync(category);

        var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(category);
        var allStores = await _storeService.GetAllStoresAsync();
        foreach (var store in allStores)
        {
            if (model.SelectedStoreIds.Contains(store.Id))
            {
                //new store
                if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                    await _storeMappingService.InsertStoreMappingAsync(category, store.Id);
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
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _categoryModelFactory.PrepareCategorySearchModelAsync(new CategorySearchModel());

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<CategorySearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _categoryModelFactory.PrepareCategoryListModelAsync(searchModel);

        return Json(model);
    }

    #endregion

    #region Create / Edit / Delete

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _categoryModelFactory.PrepareCategoryModelAsync(new CategoryModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<CategoryModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var category = model.ToEntity<Category>();
            category.CreatedOnUtc = DateTime.UtcNow;
            category.UpdatedOnUtc = DateTime.UtcNow;
            await _categoryService.InsertCategoryAsync(category);

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(category, model.SeName, category.Name, true);
            await _urlRecordService.SaveSlugAsync(category, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(category, model);

            //discounts
            var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToCategories, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    await _categoryService.InsertDiscountCategoryMappingAsync(new DiscountCategoryMapping { DiscountId = discount.Id, EntityId = category.Id });
            }

            await _categoryService.UpdateCategoryAsync(category);

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(category);

            //ACL (customer roles)
            await SaveCategoryAclAsync(category, model);

            //stores
            await SaveStoreMappingsAsync(category, model);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCategory"), category.Name), category);

            return Created(category.Id, await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Added"));
        }

        //prepare model
        model = await _categoryModelFactory.PrepareCategoryModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        //try to get a category with the specified id
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null || category.Deleted)
            return NotFound("No category found with the specified id");

        //prepare model
        var model = await _categoryModelFactory.PrepareCategoryModelAsync(null, category);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<CategoryModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a category with the specified id
        var category = await _categoryService.GetCategoryByIdAsync(model.Id);
        if (category == null || category.Deleted)
            return NotFound("No category found with the specified id");

        if (ModelState.IsValid)
        {
            var prevPictureId = category.PictureId;

            //if parent category changes, we need to clear cache for previous parent category
            if (category.ParentCategoryId != model.ParentCategoryId)
            {
                await _staticCacheManager.RemoveByPrefixAsync(NopCatalogDefaults.CategoriesByParentCategoryPrefix, category.ParentCategoryId);
                await _staticCacheManager.RemoveByPrefixAsync(NopCatalogDefaults.CategoriesChildIdsPrefix, category.ParentCategoryId);
            }

            category = model.ToEntity(category);
            category.UpdatedOnUtc = DateTime.UtcNow;
            await _categoryService.UpdateCategoryAsync(category);

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(category, model.SeName, category.Name, true);
            await _urlRecordService.SaveSlugAsync(category, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(category, model);

            //discounts
            var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToCategories, showHidden: true, isActive: null);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (await _categoryService.GetDiscountAppliedToCategoryAsync(category.Id, discount.Id) is null)
                        await _categoryService.InsertDiscountCategoryMappingAsync(new DiscountCategoryMapping { DiscountId = discount.Id, EntityId = category.Id });
                }
                else
                {
                    //remove discount
                    if (await _categoryService.GetDiscountAppliedToCategoryAsync(category.Id, discount.Id) is DiscountCategoryMapping mapping)
                        await _categoryService.DeleteDiscountCategoryMappingAsync(mapping);
                }
            }

            await _categoryService.UpdateCategoryAsync(category);

            //delete an old picture (if deleted or updated)
            if (prevPictureId > 0 && prevPictureId != category.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureByIdAsync(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePictureAsync(prevPicture);
            }

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(category);

            //ACL
            await SaveCategoryAclAsync(category, model);

            //stores
            await SaveStoreMappingsAsync(category, model);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCategory"), category.Name), category);

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Updated"));
        }

        //prepare model
        model = await _categoryModelFactory.PrepareCategoryModelAsync(model, category, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        //try to get a category with the specified id
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound("No category found with the specified id");

        await _categoryService.DeleteCategoryAsync(category);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteCategory",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCategory"), category.Name), category);

        return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return NotFound();

        await _categoryService.DeleteCategoriesAsync(await (await _categoryService.GetCategoriesByIdsAsync(selectedIds.ToArray())).WhereAwait(async p => await _workContext.GetCurrentVendorAsync() == null).ToListAsync());

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Export / Import

    public virtual async Task<IActionResult> ExportXml()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        try
        {
            var xml = await _exportManager.ExportCategoriesToXmlAsync();

            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "categories.xml");
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc);
            return BadRequest(exc.Message);
        }
    }

    public virtual async Task<IActionResult> ExportXlsx()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        try
        {
            var bytes = await _exportManager
                .ExportCategoriesToXlsxAsync((await _categoryService.GetAllCategoriesAsync(showHidden: true)).ToList());

            return File(bytes, MimeTypes.TextXlsx, "categories.xlsx");
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc);
            return BadRequest(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ImportFromXlsx(IFormFile importexcelfile)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        //a vendor cannot import categories
        if (await _workContext.GetCurrentVendorAsync() != null)
            return AdminApiAccessDenied();

        try
        {
            if (importexcelfile != null && importexcelfile.Length > 0)
            {
                await _importManager.ImportCategoriesFromXlsxAsync(importexcelfile.OpenReadStream());
            }
            else
            {
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
            }

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Imported"));
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc);
            return BadRequest(exc.Message);
        }
    }

    #endregion

    #region Products

    [HttpPost]
    public virtual async Task<IActionResult> ProductList([FromBody] BaseQueryModel<CategoryProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a category with the specified id
        var category = await _categoryService.GetCategoryByIdAsync(searchModel.CategoryId);
        if (category == null)
            return NotFound("No category found with the specified id");

        //prepare model
        var model = await _categoryModelFactory.PrepareCategoryProductListModelAsync(searchModel, category);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> ProductUpdate([FromBody] BaseQueryModel<CategoryProductModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product category with the specified id
        var productCategory = await _categoryService.GetProductCategoryByIdAsync(model.Id);
        if (productCategory == null)
            return NotFound();

        //fill entity from product
        productCategory = model.ToEntity(productCategory);
        await _categoryService.UpdateProductCategoryAsync(productCategory);

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ProductDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        //try to get a product category with the specified id
        var productCategory = await _categoryService.GetProductCategoryByIdAsync(id);
        if (productCategory == null)
            return NotFound("No product category found with the specified id");

        await _categoryService.DeleteProductCategoryAsync(productCategory);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{categoryId}")]
    public virtual async Task<IActionResult> ProductAddPopup(int categoryId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _categoryModelFactory.PrepareAddProductToCategorySearchModelAsync(new AddProductToCategorySearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAddPopupList([FromBody] BaseQueryModel<AddProductToCategorySearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _categoryModelFactory.PrepareAddProductToCategoryListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAddPopup([FromBody] BaseQueryModel<AddProductToCategoryModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //get selected products
        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        if (selectedProducts.Any())
        {
            var existingProductCategories = await _categoryService.GetProductCategoriesByCategoryIdAsync(model.CategoryId, showHidden: true);
            foreach (var product in selectedProducts)
            {
                //whether product category with such parameters already exists
                if (_categoryService.FindProductCategory(existingProductCategories, product.Id, model.CategoryId) != null)
                    continue;

                //insert the new product category mapping
                await _categoryService.InsertProductCategoryAsync(new ProductCategory
                {
                    CategoryId = model.CategoryId,
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