using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.MegaMenu.Infrastructure.Cache;
using NopStation.Plugin.Widgets.MegaMenu.Models;
using NopStation.Plugin.Widgets.MegaMenu.Services;

namespace NopStation.Plugin.Widgets.MegaMenu.Factories;

public class MegaMenuModelFactory : IMegaMenuModelFactory
{
    #region Fields

    private readonly BlogSettings _blogSettings;
    private readonly ICategoryIconService _categoryIconService;
    private readonly CatalogSettings _catalogSettings;
    private readonly DisplayDefaultMenuItemSettings _displayDefaultMenuItemSettings;
    private readonly ForumSettings _forumSettings;
    private readonly ICategoryService _categoryService;
    private readonly ILocalizationService _localizationService;
    private readonly IPictureService _pictureService;
    private readonly IProductService _productService;
    private readonly IStaticCacheManager _cacheManager;
    private readonly IStoreContext _storeContext;
    private readonly ITopicService _topicService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IWebHelper _webHelper;
    private readonly IWorkContext _workContext;
    private readonly MediaSettings _mediaSettings;
    private readonly MegaMenuSettings _megaMenuSettings;
    private readonly IMegaMenuCoreService _megaMenuCoreService;
    private readonly ICustomerService _customerService;

    #endregion

    #region Ctor

    public MegaMenuModelFactory(BlogSettings blogSettings,
        ICategoryIconService categoryIconService,
        CatalogSettings catalogSettings,
        DisplayDefaultMenuItemSettings displayDefaultMenuItemSettings,
        ForumSettings forumSettings,
        ICategoryService categoryService,
        ILocalizationService localizationService,
        IPictureService pictureService,
        IProductService productService,
        IStaticCacheManager cacheManager,
        IStoreContext storeContext,
        ITopicService topicService,
        IUrlRecordService urlRecordService,
        IWebHelper webHelper,
        IWorkContext workContext,
        MediaSettings mediaSettings,
        MegaMenuSettings megaMenuSettings,
        IMegaMenuCoreService megaMenuCoreService,
        ICustomerService customerService
        )
    {
        _blogSettings = blogSettings;
        _categoryIconService = categoryIconService;
        _catalogSettings = catalogSettings;
        _displayDefaultMenuItemSettings = displayDefaultMenuItemSettings;
        _forumSettings = forumSettings;
        _categoryService = categoryService;
        _localizationService = localizationService;
        _pictureService = pictureService;
        _productService = productService;
        _cacheManager = cacheManager;
        _storeContext = storeContext;
        _topicService = topicService;
        _urlRecordService = urlRecordService;
        _webHelper = webHelper;
        _workContext = workContext;
        _mediaSettings = mediaSettings;
        _megaMenuSettings = megaMenuSettings;
        _megaMenuCoreService = megaMenuCoreService;
        _customerService = customerService;
    }

    #endregion

    protected virtual async Task<List<MegaMenuModel.TopicModel>> PrepareTopicMenuModelsAsync()
    {
        //top menu topics
        var topicCacheKey = _cacheManager.PrepareKeyForDefaultCache(MegaMenuModelCacheEventConsumer.MEGAMENU_TOPICS_MODEL_KEY,
            await _workContext.GetWorkingLanguageAsync(),
            await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
            await _storeContext.GetCurrentStoreAsync());
        var cachedTopicModel = await _cacheManager.GetAsync(topicCacheKey, async () =>
            (await _topicService.GetAllTopicsAsync((await _storeContext.GetCurrentStoreAsync()).Id))
            .Where(t => t.IncludeInTopMenu)
            .SelectAwait(async t => new MegaMenuModel.TopicModel
            {
                Id = t.Id,
                Name = await _localizationService.GetLocalizedAsync(t, x => x.Title),
                SeName = await _urlRecordService.GetSeNameAsync(t)
            })
            .ToListAsync()
        );

        return await cachedTopicModel;
    }

    protected virtual async Task<List<CategoryMenuModel>> PrepareCategoryMenuModelsAsync()
    {
        var loadPicture = _megaMenuSettings.ShowCategoryPicture;
        var loadRightPanelPicture = _megaMenuSettings.ShowMainCategoryPictureRight;
        //load and cache them
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(MegaMenuModelCacheEventConsumer.MEGAMENU_CATEGORIES_MODEL_KEY,
            await _workContext.GetWorkingLanguageAsync(),
            await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
            await _storeContext.GetCurrentStoreAsync());
        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var ids = new List<int>();
            if (!string.IsNullOrWhiteSpace(_megaMenuSettings.SelectedCategoryIds))
                ids = _megaMenuSettings.SelectedCategoryIds.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

            var allCategories = await _megaMenuCoreService.GetCategoriesByIdsAsync((await _storeContext.GetCurrentStoreAsync()).Id, ids);
            return await PrepareCategoryMenuModelsAsync(allCategories, 0, loadPicture, loadRightPanelPicture, skipItems: true);
        });
    }

    protected virtual async Task<List<CategoryMenuModel>> PrepareCategoryMenuModelsAsync(IList<Category> allCategories,
        int rootCategoryId, bool loadPicture = true, bool loadRightPanelPicture = true, bool loadSubCategories = true, bool skipItems = false)
    {
        var result = new List<CategoryMenuModel>();
        var pictureSize = _mediaSettings.CategoryThumbPictureSize;

        var parentCategories = allCategories.Where(c => c.ParentCategoryId == rootCategoryId).ToList();

        foreach (var category in parentCategories)
        {
            var categoryModel = new CategoryMenuModel
            {
                Id = category.Id,
                Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                SeName = await _urlRecordService.GetSeNameAsync(category),
                IncludeInTopMenu = category.IncludeInTopMenu
            };
            if (loadPicture)
            {
                //prepare picture model
                var categoryIconPictureId = 0;
                var categoryIcon = await _categoryIconService.GetCategoryIconByCategoryIdAsync(category.Id);
                if (categoryIcon != null)
                {
                    categoryIconPictureId = categoryIcon.PictureId;
                }
                else
                {
                    categoryIconPictureId = _megaMenuSettings.DefaultCategoryIconId;
                }
                var categoryPictureCacheKey = _cacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryPictureModelKey,
                    categoryIconPictureId,
                    pictureSize,
                    true,
                    await _workContext.GetWorkingLanguageAsync(),
                    _webHelper.IsCurrentConnectionSecured(),
                    await _storeContext.GetCurrentStoreAsync());
                categoryModel.PictureModel = await _cacheManager.GetAsync(categoryPictureCacheKey, async () =>
                {
                    var picture = await _pictureService.GetPictureByIdAsync(categoryIconPictureId);
                    if (picture == null)
                    {
                        picture = await _pictureService.GetPictureByIdAsync(_megaMenuSettings.DefaultCategoryIconId);
                    }
                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                        ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, pictureSize)).Url,
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageLinkTitleFormat"), categoryModel.Name),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageAlternateTextFormat"), categoryModel.Name)
                    };
                    return pictureModel;
                });
            }

            #region RightImage

            if (loadRightPanelPicture)
            {
                var parentCategoryPictureCachekey = _cacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryPictureModelKey,
                category.PictureId,
                pictureSize,
                true,
                await _workContext.GetWorkingLanguageAsync(),
                _webHelper.IsCurrentConnectionSecured(),
                await _storeContext.GetCurrentStoreAsync());
                categoryModel.ParentPicture = await _cacheManager.GetAsync(parentCategoryPictureCachekey, async () =>
                {
                    var picture = _pictureService.GetPictureByIdAsync(category.PictureId);

                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = await _pictureService.GetPictureUrlAsync(picture?.Id ?? 0),
                        ImageUrl = await _pictureService.GetPictureUrlAsync(picture?.Id ?? 0, pictureSize),
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageLinkTitleFormat"), categoryModel.Name),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageAlternateTextFormat"), categoryModel.Name)
                    };
                    return pictureModel;
                });
            }
            #endregion

            //number of products in each category
            if (_megaMenuSettings.ShowNumberOfCategoryProducts)
            {
                var cacheKey = _cacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributePictureModelKey,
                    await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
                    await _storeContext.GetCurrentStoreAsync(),
                    category);
                categoryModel.NumberOfProducts = await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    var categoryIds = new List<int>
                    {
                        category.Id
                    };
                    //include subcategories
                    if (_megaMenuSettings.ShowNumberOfCategoryProductsIncludeSubcategories)
                        categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(category.Id, (await _storeContext.GetCurrentStoreAsync()).Id));

                    return await _productService.GetNumberOfProductsInCategoryAsync(categoryIds, (await _storeContext.GetCurrentStoreAsync()).Id);
                });
            }

            if (loadSubCategories)
            {
                var subCategories = await PrepareCategoryMenuModelsAsync(allCategories, category.Id, _megaMenuSettings.ShowSubcategoryPicture, loadSubCategories);
                categoryModel.SubCategories.AddRange(subCategories);
            }
            result.Add(categoryModel);
        }

        return result;
    }

    protected virtual async Task<List<ManufacturerMenuModel>> PrepareManufactureMenuModelsAsync(List<int> selectedManufactureIds)
    {
        var pictureSize = _mediaSettings.ManufacturerThumbPictureSize;
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(MegaMenuModelCacheEventConsumer.MEGAMENU_MANUFACTURERS_MODEL_KEY,
           await _workContext.GetWorkingLanguageAsync(),
           await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
           await _storeContext.GetCurrentStoreAsync());

        var manufacturers = await _cacheManager.GetAsync(cacheKey, async () =>
        (await _megaMenuCoreService.GetManufacturersByIdsAsync((await _storeContext.GetCurrentStoreAsync()).Id, selectedManufactureIds))
        .SelectAwait(async manufacturer =>
        {
            var model = new ManufacturerMenuModel
            {
                Id = manufacturer.Id,
                Name = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Name),
                SeName = await _urlRecordService.GetSeNameAsync(manufacturer)
            };

            if (_megaMenuSettings.ShowManufacturerPicture)
            {
                var manufacturerPictureCacheKey = _cacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ManufacturerPictureModelKey,
                    manufacturer,
                    pictureSize,
                    true,
                    await _workContext.GetWorkingLanguageAsync(),
                    _webHelper.IsCurrentConnectionSecured(),
                    await _storeContext.GetCurrentStoreAsync());
                model.PictureModel = await _cacheManager.GetAsync(manufacturerPictureCacheKey, async () =>
                {
                    var picture = await _pictureService.GetPictureByIdAsync(manufacturer.PictureId);
                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                        ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, pictureSize)).Url,
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Manufacturer.ImageLinkTitleFormat"), model.Name),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Manufacturer.ImageAlternateTextFormat"), model.Name)
                    };
                    return pictureModel;
                });
            }
            return model;
        }).ToListAsync());

        return await manufacturers;
    }

    public virtual async Task<MegaMenuModel> PrepareMegaMenuModelAsync()
    {
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(MegaMenuModelCacheEventConsumer.MEGAMENU_MODEL_KEY,
                await _workContext.GetWorkingLanguageAsync(),
               await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
               await _storeContext.GetCurrentStoreAsync());
        return await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var model = new MegaMenuModel()
            {
                NewProductsEnabled = _catalogSettings.NewProductsEnabled,
                BlogEnabled = _blogSettings.Enabled,
                ForumEnabled = _forumSettings.ForumsEnabled,
                DisplayHomePageMenuItem = _displayDefaultMenuItemSettings.DisplayHomepageMenuItem,
                DisplayNewProductsMenuItem = _displayDefaultMenuItemSettings.DisplayNewProductsMenuItem,
                DisplayProductSearchMenuItem = _displayDefaultMenuItemSettings.DisplayProductSearchMenuItem,
                DisplayCustomerInfoMenuItem = _displayDefaultMenuItemSettings.DisplayCustomerInfoMenuItem,
                DisplayBlogMenuItem = _displayDefaultMenuItemSettings.DisplayBlogMenuItem,
                DisplayForumsMenuItem = _displayDefaultMenuItemSettings.DisplayForumsMenuItem,
                DisplayContactUsMenuItem = _displayDefaultMenuItemSettings.DisplayContactUsMenuItem,
                MaxCategoryLevelsToShow = _megaMenuSettings.MaxCategoryLevelsToShow,
                HideManufacturers = _megaMenuSettings.HideManufacturers
            };

            model.Topics = await PrepareTopicMenuModelsAsync();
            model.Categories = await PrepareCategoryMenuModelsAsync();

            if (!_megaMenuSettings.HideManufacturers)
            {
                var selectedManufacturerIds = new List<int>();
                if (!string.IsNullOrWhiteSpace(_megaMenuSettings.SelectedManufacturerIds))
                    selectedManufacturerIds = _megaMenuSettings.SelectedManufacturerIds.Split(',').Select(int.Parse).ToList();

                model.Manufacturers = await PrepareManufactureMenuModelsAsync(selectedManufacturerIds);
            }

            return model;
        });
    }
}
