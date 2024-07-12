using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using NopStation.Plugin.Widgets.CategoryBanners.Models;
using NopStation.Plugin.Widgets.CategoryBanners.Services;
using NopStation.Plugin.Widgets.CategoryBanners.Services.Cache;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.CategoryBanners.Components
{
    public class CategoryBannerViewComponent : NopStationViewComponent
    {
        private readonly ICategoryBannerService _categoryBannerService;
        private readonly CategoryBannerSettings _categoryBannerSettings;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IPictureService _pictureService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly INopStationContext _nopStationContext;

        public CategoryBannerViewComponent(ICategoryBannerService categoryBannerService,
            CategoryBannerSettings categoryBannerSettings,
            IStaticCacheManager staticCacheManager,
            IPictureService pictureService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            INopStationContext nopStationContext)
        {
            _categoryBannerService = categoryBannerService;
            _categoryBannerSettings = categoryBannerSettings;
            _cacheManager = staticCacheManager;
            _pictureService = pictureService;
            _workContext = workContext;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _localizationService = localizationService;
            _nopStationContext = nopStationContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (_categoryBannerSettings.HideInPublicStore)
                return Content("");

            if (additionalData.GetType() != typeof(CategoryModel))
                return Content("");

            var cm = additionalData as CategoryModel;
            var banners = _categoryBannerService.GetCategoryBannersByCategoryId(cm.Id, _nopStationContext.MobileDevice);
            var model = new CategoryBannerModel()
            { 
                AutoPlay = _categoryBannerSettings.AutoPlay,
                AutoPlayHoverPause = _categoryBannerSettings.AutoPlayHoverPause,
                AutoPlayTimeout = _categoryBannerSettings.AutoPlayTimeout,
                Loop = _categoryBannerSettings.Loop,
                Nav = _categoryBannerSettings.Nav,
                Id = cm.Id
            };

            var pictureSize = _categoryBannerSettings.BannerPictureSize;

            foreach (var item in banners)
            {
                var cache = new CacheKey(ModelCacheDefaults.CategoryBannerPictureModelKey);

                var cacheKey = _cacheManager.PrepareKeyForDefaultCache(cache,
                    item.Id, pictureSize, true, (await _workContext.GetWorkingLanguageAsync()).Id, _webHelper.IsCurrentConnectionSecured(),
                    _storeContext.GetCurrentStoreAsync().Id);

                var defaultPictureModel = await _cacheManager.GetAsync(cacheKey, async() =>
                {
                    var picture = await _pictureService.GetPictureByIdAsync(item.PictureId);
                    var pictureModel = new PictureModel
                    {
                        ImageUrl = await _pictureService.GetPictureUrlAsync(picture.Id, pictureSize),
                        FullSizeImageUrl = await _pictureService.GetPictureUrlAsync(picture.Id),
                        //"title" attribute
                        Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                            ? picture.TitleAttribute
                            : string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageLinkTitleFormat"),
                                cm.Name),
                        //"alt" attribute
                        AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                            ? picture.AltAttribute
                            : string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageAlternateTextFormat"),
                                cm.Name)
                    };

                    return pictureModel;
                });

                model.Banners.Add(defaultPictureModel);
            }

            return View(model);
        }
    }
}
