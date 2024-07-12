using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.CategoryBanners.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.CategoryBanners.Areas.Admin.Models;
using NopStation.Plugin.Widgets.CategoryBanners.Domains;
using NopStation.Plugin.Widgets.CategoryBanners.Services;
using NopStation.Plugin.Widgets.CategoryBanners.Services.Cache;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Mvc;
using Nop.Services.Catalog;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Widgets.CategoryBanners.Areas.Admin.Controllers
{
    public class CategoryBannerController : NopStationAdminController
    {
        #region Firlds

        private readonly ICategoryService _categoryService;
        private readonly IPermissionService _permissionService;
        private readonly ICategoryBannerService _categoryBannerService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ICategoryBannerModelFactory _categoryBannerModelFactory;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public CategoryBannerController(ICategoryService categoryService,
            IPermissionService permissionService,
            ICategoryBannerService categoryBannerService,
            IPictureService pictureService,
            ISettingService settingService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ICategoryBannerModelFactory categoryBannerModelFactory,
            IStaticCacheManager cacheManager)
        {
            _categoryService = categoryService;
            _permissionService = permissionService;
            _categoryBannerService = categoryBannerService;
            _pictureService = pictureService;
            _settingService = settingService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _categoryBannerModelFactory = categoryBannerModelFactory;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Utilities


        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(CategoryBannerPermissionProvider.ManageCategoryBanner))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var categoryBannerSettings = await _settingService.LoadSettingAsync<CategoryBannerSettings>(storeId);
            
            var model = new ConfigurationModel()
            {
                HideInPublicStore = categoryBannerSettings.HideInPublicStore,
                AutoPlay = categoryBannerSettings.AutoPlay,
                Nav = categoryBannerSettings.Nav,
                Loop = categoryBannerSettings.Loop,
                AutoPlayHoverPause = categoryBannerSettings.AutoPlayHoverPause,
                AutoPlayTimeout = categoryBannerSettings.AutoPlayTimeout,
                BannerPictureSize = categoryBannerSettings.BannerPictureSize
            };

            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId > 0)
            {
                model.HideInPublicStore_OverrideForStore = await _settingService.SettingExistsAsync(categoryBannerSettings, x => x.HideInPublicStore, storeId);
                model.Nav_OverrideForStore = await _settingService.SettingExistsAsync(categoryBannerSettings, x => x.Nav, storeId);
                model.AutoPlay_OverrideForStore = await _settingService.SettingExistsAsync(categoryBannerSettings, x => x.AutoPlay, storeId);
                model.Loop_OverrideForStore = await _settingService.SettingExistsAsync(categoryBannerSettings, x => x.Loop, storeId);
                model.AutoPlayHoverPause_OverrideForStore = await _settingService.SettingExistsAsync(categoryBannerSettings, x => x.AutoPlayHoverPause, storeId);
                model.AutoPlayTimeout_OverrideForStore = await _settingService.SettingExistsAsync(categoryBannerSettings, x => x.AutoPlayTimeout, storeId);
                model.BannerPictureSize_OverrideForStore = await _settingService.SettingExistsAsync(categoryBannerSettings, x => x.BannerPictureSize, storeId);
            }

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(CategoryBannerPermissionProvider.ManageCategoryBanner))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var categoryBannerSettings = await _settingService.LoadSettingAsync<CategoryBannerSettings>(storeScope);

            categoryBannerSettings.HideInPublicStore = model.HideInPublicStore;
            categoryBannerSettings.Nav = model.Nav;
            categoryBannerSettings.AutoPlay = model.AutoPlay;
            categoryBannerSettings.Loop = model.Loop;
            categoryBannerSettings.AutoPlayHoverPause = model.AutoPlayHoverPause;
            categoryBannerSettings.AutoPlayTimeout = model.AutoPlayTimeout;
            categoryBannerSettings.BannerPictureSize = model.BannerPictureSize;

            await _settingService.SaveSettingOverridablePerStoreAsync(categoryBannerSettings, x => x.HideInPublicStore, model.HideInPublicStore_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(categoryBannerSettings, x => x.Nav, model.Nav_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(categoryBannerSettings, x => x.AutoPlay, model.AutoPlay_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(categoryBannerSettings, x => x.Loop, model.Loop_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(categoryBannerSettings, x => x.AutoPlayHoverPause, model.AutoPlayHoverPause_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(categoryBannerSettings, x => x.AutoPlayTimeout, model.AutoPlayTimeout_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(categoryBannerSettings, x => x.BannerPictureSize, model.BannerPictureSize_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.CategoryBannerPicturePrefixCacheKey);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> CategoryBannerList(CategoryBannerSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var category = await _categoryService.GetCategoryByIdAsync(searchModel.CategoryId);
            if (category == null || category.Deleted)
                throw new ArgumentException();

            var model = await _categoryBannerModelFactory.PrepareProductPictureListModelAsync(searchModel, category);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> CategoryBannerUpdate(CategoryBannerModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var banner = await _categoryBannerService.GetCategoryBannerByIdAsync(model.Id)
                ?? throw new ArgumentException("No category banner found with the specified id");

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(banner.PictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            var category = await _categoryService.GetCategoryByIdAsync(banner.CategoryId);
            if (category == null || category.Deleted)
                return Json(new { Result = false });

            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);

            await _pictureService.SetSeoFilenameAsync(banner.PictureId, await _pictureService.GetPictureSeNameAsync(category.Name));

            banner.DisplayOrder = model.DisplayOrder;
            banner.ForMobile = model.ForMobile;

            await _categoryBannerService.UpdateCategoryBannerAsync(banner);

            return new NullJsonResult();
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> CategoryBannerDelete(CategoryBannerModel model)
        {
            if (! await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var banner = await _categoryBannerService.GetCategoryBannerByIdAsync(model.Id)
                ?? throw new ArgumentException("No category banner found with the specified id");
            
            await _categoryBannerService.DeleteCategoryBannerAsync(banner);

            return new NullJsonResult();
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> CreateBanner(CategoryBannerModel model)
        {
            if (! await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var category = await _categoryService.GetCategoryByIdAsync(model.CategoryId);
            if (category == null || category.Deleted)
                return Json(new { Result = false });

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(model.PictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);

            await _pictureService.SetSeoFilenameAsync(model.PictureId, await _pictureService.GetPictureSeNameAsync(category.Name));

            var banner = new CategoryBanner()
            {
                CategoryId = model.CategoryId,
                DisplayOrder = model.DisplayOrder,
                ForMobile = model.ForMobile,
                PictureId = model.PictureId
            };
            await _categoryBannerService.InsertCategoryBannerAsync(banner);

            return Json(new { Result = true });
        }

        #endregion
    }
}
