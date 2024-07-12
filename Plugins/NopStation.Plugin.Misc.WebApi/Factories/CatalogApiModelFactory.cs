using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using NopStation.Plugin.Misc.WebApi.Infrastructure.Cache;
using NopStation.Plugin.Misc.WebApi.Models.Catalog;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Factories
{
    public class CatalogApiModelFactory : ICatalogApiModelFactory
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
        private readonly MediaSettings _mediaSettings;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly WebApiSettings _webApiSettings;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ICategoryIconService _categoryIconService;

        public CatalogApiModelFactory(IManufacturerService manufacturerService,
            ICategoryService categoryService,
            MediaSettings mediaSettings,
            IWorkContext workContext,
            IWebHelper webHelper,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
            WebApiSettings webApiSettings,
            IProductService productService,
            IProductModelFactory productModelFactory,
            ICategoryIconService categoryIconService,
            IStaticCacheManager cacheManager)
        {
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _mediaSettings = mediaSettings;
            _workContext = workContext;
            _webHelper = webHelper;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _webApiSettings = webApiSettings;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _categoryIconService = categoryIconService;
            _cacheManager = cacheManager;
        }

        #region Utilities

        protected async Task<IList<Product>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var categoryIds = new List<int> { categoryId };
            if (_webApiSettings.ShowSubCategoryProducts)
            {
                categoryIds.AddRange((await _categoryService.GetAllCategoriesByParentCategoryIdAsync(categoryId)).Select(x => x.Id).ToList());
            }

            //products
            var products = await _productService.SearchProductsAsync(
                categoryIds: categoryIds,
                storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                visibleIndividuallyOnly: true,
                pageSize: _webApiSettings.NumberOfHomepageCategoryProducts);

            return products;
        }

        protected async Task<IList<HomepageCategoryModel>> GetSubCategoriesAsync(int categoryId)
        {
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;
            var subcategories = (await _categoryService.GetAllCategoriesByParentCategoryIdAsync(categoryId))
                .SelectAwait(async subcategory =>
                {
                    var subCategoryModel = new HomepageCategoryModel
                    {
                        Name = await _localizationService.GetLocalizedAsync(subcategory, x => x.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(subcategory),
                        Id = subcategory.Id
                    };
                    return subCategoryModel;
                })
                .ToListAsync();

            return await subcategories;
        }

        protected async Task<IList<CategoryTreeModel>> PrepareCategoryModelAsync(int categoryId)
        {
            var models = new List<CategoryTreeModel>();
            var categories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(categoryId);
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;

            foreach (var category in categories)
            {
                var categoryPicture = await _pictureService.GetPictureByIdAsync(category.PictureId);

                models.Add(new CategoryTreeModel()
                {
                    CategoryId = category.Id,
                    Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(category),
                    IconUrl = (await _pictureService.GetPictureUrlAsync(categoryPicture, pictureSize)).Url,
                    SubCategories = await PrepareCategoryModelAsync(category.Id)
                });
            }

            return models;
        }

        #endregion

        public async Task<IList<CategoryTreeModel>> PrepareCategoryTreeModelAsync()
        {
            var categories = await PrepareCategoryModelAsync(0);
            return categories;
        }

        public async Task<IList<HomepageCategoryModel>> PrepareHomepageCategoriesWithProductsModelAsync()
        {
            var pictureSize = _mediaSettings.CategoryThumbPictureSize;

            var model = (await _categoryService.GetAllCategoriesDisplayedOnHomepageAsync())
                .SelectAwait(async category =>
                {
                    var catModel = new HomepageCategoryModel
                    {
                        Id = category.Id,
                        Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(category)
                    };

                    var products = await GetProductsByCategoryIdAsync(category.Id);
                    catModel.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, null)).ToList();
                    catModel.SubCategories = await GetSubCategoriesAsync(category.Id);
                    return catModel;
                })
                .ToListAsync();

            return await model;
        }

        public async Task<IList<ManufacturerModel>> PrepareHomepageManufacturerModelsAsync()
        {
            var model = new List<ManufacturerModel>();
            var store = await _storeContext.GetCurrentStoreAsync();
            var manufacturers = await _manufacturerService.GetAllManufacturersAsync(storeId: store.Id, pageSize: _webApiSettings.NumberOfManufacturers);
            foreach (var manufacturer in manufacturers)
            {
                var modelMan = new ManufacturerModel
                {
                    Id = manufacturer.Id,
                    Name = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Name),
                    Description = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Description),
                    MetaKeywords = await _localizationService.GetLocalizedAsync(manufacturer, x => x.MetaKeywords),
                    MetaDescription = await _localizationService.GetLocalizedAsync(manufacturer, x => x.MetaDescription),
                    MetaTitle = await _localizationService.GetLocalizedAsync(manufacturer, x => x.MetaTitle),
                    SeName = await _urlRecordService.GetSeNameAsync(manufacturer),
                };

                //prepare picture model
                var pictureSize = _mediaSettings.ManufacturerThumbPictureSize;

                var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ApiModelCacheDefaults.ManufacturerPictureModelKey,
                       manufacturer.Id, pictureSize, true, (await _workContext.GetWorkingLanguageAsync()).Id, _webHelper.IsCurrentConnectionSecured(),
                       store.Id);

                modelMan.PictureModel = await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    var picture = await _pictureService.GetPictureByIdAsync(manufacturer.PictureId);
                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                        ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, pictureSize)).Url,
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Manufacturer.ImageLinkTitleFormat"), modelMan.Name),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Manufacturer.ImageAlternateTextFormat"), modelMan.Name)
                    };
                    return pictureModel;
                });
                model.Add(modelMan);
            }

            return model;
        }
    }
}
