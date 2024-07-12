using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [Route("api/catalog")]
    public class CatalogApiController : BaseApiController
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPermissionService _permissionService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly ICategoryIconService _categoryIconService;
        private readonly IPictureService _pictureService;

        #endregion

        #region Ctor

        public CatalogApiController(CatalogSettings catalogSettings,
            IAclService aclService,
            ICatalogModelFactory catalogModelFactory,
            ICategoryService categoryService,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IPermissionService permissionService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductTagService productTagService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            VendorSettings vendorSettings,
            ICategoryIconService categoryIconService,
            IPictureService pictureService)
        {
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _catalogModelFactory = catalogModelFactory;
            _categoryService = categoryService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _permissionService = permissionService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _productTagService = productTagService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _vendorSettings = vendorSettings;
            _categoryIconService = categoryIconService;
            _pictureService = pictureService;
        }

        #endregion

        #region Action methods

        #region Categories

        [HttpGet("categories")]
        public async Task<IActionResult> Categories(int currentCategoryId, int currentProductId)
        {
            var model = await _catalogModelFactory.PrepareCategoryNavigationModelAsync(currentCategoryId, currentProductId);
            return OkWrap(model);
        }

        [HttpGet("homepagecategories")]
        public async Task<IActionResult> HomepageCategories()
        {
            var response = new GenericResponseModel<List<CategoryModel>>();
            response.Data = await _catalogModelFactory.PrepareHomepageCategoryModelsAsync();
            return Ok(response);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> Category(int categoryId, CatalogProductsCommand queryModel)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (!await CheckCategoryAvailabilityAsync(category))
                return NotFound();

            var command = queryModel;

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewCategory"), category.Name), category);

            var model = await _catalogModelFactory.PrepareCategoryModelAsync(category, command);

            var categoryIcon = await _categoryIconService.GetCategoryIconByCategoryIdAsync(category.Id);

            if (categoryIcon != null)
            {
                var banner = await _pictureService.GetPictureByIdAsync(categoryIcon.CategoryBannerId);
                var pictureModel = new PictureModel
                {
                    FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(banner)).Url,
                    ImageUrl = (await _pictureService.GetPictureUrlAsync(banner, _mediaSettings.CategoryThumbPictureSize)).Url,
                    Title = string.Format(
                                    await _localizationService.GetResourceAsync("NopStation.WebApi.Category.CategoryIcons.ImageLinkTitleFormat"),
                                    category.Name),
                    AlternateText = string.Format(
                                        await _localizationService.GetResourceAsync("NopStation.WebApi.Category.CategoryIcons.ImageAlternateTextFormat"),
                                        category.Name)
                };
                model.PictureModel = pictureModel;
            }
            return OkWrap(model);
        }

        //ignore SEO friendly URLs checks
        [CheckLanguageSeoCode(true)]
        [HttpGet("category/getcategoryproducts/{categoryId}")]
        public virtual async Task<IActionResult> GetCategoryProducts(int categoryId, CatalogProductsCommand command)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);

            if (!await CheckCategoryAvailabilityAsync(category))
                return NotFound();

            var model = await _catalogModelFactory.PrepareCategoryProductsModelAsync(category, command);

            return OkWrap(model);
        }

        [HttpPost("category/getcatalogroot")]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> GetCatalogRoot()
        {
            var response = new GenericResponseModel<List<CategorySimpleModel>>();
            response.Data = await _catalogModelFactory.PrepareRootCategoriesAsync();

            return Ok(response);
        }

        [HttpPost("category/getcatalogsubcategories/{id}")]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> GetCatalogSubCategories(int id)
        {
            var response = new GenericResponseModel<List<CategorySimpleModel>>();
            response.Data = await _catalogModelFactory.PrepareSubCategoriesAsync(id);

            return Ok(response);
        }

        #endregion

        #region Manufacturers

        [HttpGet("manufacturer/{manufacturerId}")]
        public virtual async Task<IActionResult> Manufacturer(int manufacturerId, CatalogProductsCommand queryModel)
        {
            var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);
            if (!await CheckManufacturerAvailabilityAsync(manufacturer))
                return NotFound();

            var command = queryModel;

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewManufacturer",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewManufacturer"), manufacturer.Name), manufacturer);

            var model = await _catalogModelFactory.PrepareManufacturerModelAsync(manufacturer, command);
            return OkWrap(model);
        }

        //ignore SEO friendly URLs checks
        [CheckLanguageSeoCode(true)]
        [HttpGet("manufacturer/getmanufacturerproducts/{manufacturerId}")]
        public virtual async Task<IActionResult> GetManufacturerProducts(int manufacturerId, CatalogProductsCommand command)
        {
            var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);

            if (!await CheckManufacturerAvailabilityAsync(manufacturer))
                return NotFound();

            var model = await _catalogModelFactory.PrepareManufacturerProductsModelAsync(manufacturer, command);

            return OkWrap(model);
        }

        [HttpGet("manufacturer/all")]
        public virtual async Task<IActionResult> ManufacturerAll()
        {
            var response = new GenericResponseModel<List<ManufacturerModel>>();
            response.Data = await _catalogModelFactory.PrepareManufacturerAllModelsAsync();
            return Ok(response);
        }

        #endregion

        #region Vendors

        [HttpGet("vendor/{vendorId}")]
        public virtual async Task<IActionResult> Vendor(int vendorId, CatalogProductsCommand queryModel)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (!await CheckVendorAvailabilityAsync(vendor))
                return NotFound();

            var command = queryModel;

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            var model = await _catalogModelFactory.PrepareVendorModelAsync(vendor, command);
            return OkWrap(model);
        }

        //ignore SEO friendly URLs checks
        [CheckLanguageSeoCode(true)]
        [HttpGet("vendor/getvendorproducts/{vendorId}")]
        public virtual async Task<IActionResult> GetVendorProducts(int vendorId, CatalogProductsCommand command)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

            if (!await CheckVendorAvailabilityAsync(vendor))
                return NotFound();

            var model = await _catalogModelFactory.PrepareVendorProductsModelAsync(vendor, command);

            return OkWrap(model);
        }

        [HttpGet("vendor/all")]
        public virtual async Task<IActionResult> VendorAll()
        {
            var response = new GenericResponseModel<List<VendorModel>>();
            //we don't allow viewing of vendors if "vendors" block is hidden
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return Ok(response);

            response.Data = await _catalogModelFactory.PrepareVendorAllModelsAsync();
            return Ok(response);
        }

        #endregion

        #region Product tags

        [HttpGet("tag/productsbytag/{productTagId}")]
        public virtual async Task<IActionResult> ProductsByTag(int productTagId, CatalogProductsCommand command)
        {
            var productTag = await _productTagService.GetProductTagByIdAsync(productTagId);
            if (productTag == null)
                return NotFound();

            var model = await _catalogModelFactory.PrepareProductsByTagModelAsync(productTag, command);

            return OkWrap(model);
        }

        //ignore SEO friendly URLs checks
        [CheckLanguageSeoCode(true)]
        [HttpGet("tag/gettagproducts/{tagId}")]
        public virtual async Task<IActionResult> GetTagProducts(int tagId, CatalogProductsCommand command)
        {
            var productTag = await _productTagService.GetProductTagByIdAsync(tagId);
            if (productTag == null)
                return NotFound();

            var model = await _catalogModelFactory.PrepareTagProductsModelAsync(productTag, command);

            return OkWrap(model);
        }

        [HttpGet("tag/producttagsall")]
        public virtual async Task<IActionResult> ProductTagsAll()
        {
            var model = await _catalogModelFactory.PreparePopularProductTagsModelAsync();

            return OkWrap(model);
        }

        [HttpGet("homepageproducts")]
        public async Task<IActionResult> HomepageProducts(int? productThumbPictureSize)
        {
            var products = await _productService.GetAllProductsDisplayedOnHomepageAsync();
            //ACL and store mapping
            products = await products.WhereAwait(async p => (await _aclService.AuthorizeAsync(p))
            && (await _storeMappingService.AuthorizeAsync(p))).ToListAsync();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

            products = products.Where(p => p.VisibleIndividually).ToList();

            var response = new GenericResponseModel<List<ProductOverviewModel>>();

            if (!products.Any())
                return Ok(response);

            var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, productThumbPictureSize)).ToList();

            response.Data = model;
            return Ok(response);
        }

        #endregion

        #region New (recently added) products page

        [HttpGet("getnewproducts")]
        public virtual async Task<IActionResult> GetNewProducts(CatalogProductsCommand command)
        {
            if (!_catalogSettings.NewProductsEnabled)
                return NotFound();

            var model = await _catalogModelFactory.PrepareNewProductsModelAsync(command);

            return OkWrap(model);
        }

        #endregion

        #region Searching

        [HttpGet("search")]
        public virtual async Task<IActionResult> Search(SearchModel model, CatalogProductsCommand command)
        {
            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(true),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            var response = await _catalogModelFactory.PrepareSearchModelAsync(model, command);
            return OkWrap(response);
        }

        [HttpGet("catalog/searchtermautocomplete")]
        public virtual async Task<IActionResult> SearchTermAutoComplete(string term)
        {
            var response = new GenericResponseModel<object>();
            if (string.IsNullOrWhiteSpace(term))
                return Ok(response);

            term = term.Trim();
            if (string.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return LengthRequired();

            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

            var products = await _productService.SearchProductsAsync(0,
                storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                keywords: term,
                languageId: (await _workContext.GetWorkingLanguageAsync()).Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);

            var showLinkToResultSearch = _catalogSettings.ShowLinkToAllResultInSearchAutoComplete && (products.TotalCount > productNumber);

            var models = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, false,
                _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize)).ToList();

            response.Data = (from p in models
                             select new
                             {
                                 label = p.Name,
                                 productid = p.Id,
                                 producturl = Url.RouteUrl<Product>(new { SeName = p.SeName }),
                                 productpictureurl = p.PictureModels.FirstOrDefault()?.ImageUrl,
                                 showlinktoresultsearch = showLinkToResultSearch
                             }).ToList();
            return Ok(response);
        }

        //ignore SEO friendly URLs checks
        [CheckLanguageSeoCode(true)]
        public virtual async Task<IActionResult> SearchProducts(SearchModel searchModel, CatalogProductsCommand command)
        {
            if (searchModel == null)
                searchModel = new SearchModel();

            var model = await _catalogModelFactory.PrepareSearchProductsModelAsync(searchModel, command);

            return OkWrap(model);
        }

        #endregion

        #region Utilities

        private async Task<bool> CheckCategoryAvailabilityAsync(Category category)
        {
            if (category is null)
                return false;

            var isAvailable = true;

            if (category.Deleted)
                isAvailable = false;

            var notAvailable =
                //published?
                !category.Published ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(category) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(category);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories);
            if (notAvailable && !hasAdminAccess)
                isAvailable = false;

            return isAvailable;
        }

        private async Task<bool> CheckManufacturerAvailabilityAsync(Manufacturer manufacturer)
        {
            var isAvailable = true;

            if (manufacturer == null || manufacturer.Deleted)
                isAvailable = false;

            var notAvailable =
                //published?
                !manufacturer.Published ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(manufacturer) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(manufacturer);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageManufacturers);
            if (notAvailable && !hasAdminAccess)
                isAvailable = false;

            return isAvailable;
        }

        private Task<bool> CheckVendorAvailabilityAsync(Vendor vendor)
        {
            var isAvailable = true;

            if (vendor == null || vendor.Deleted || !vendor.Active)
                isAvailable = false;

            return Task.FromResult(isAvailable);
        }

        #endregion

        #endregion
    }
}
