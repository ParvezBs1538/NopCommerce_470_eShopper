using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Localization;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Media;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.SmartSliders.Domains;
using NopStation.Plugin.Widgets.SmartSliders.Infrastructure.Cache;
using NopStation.Plugin.Widgets.SmartSliders.Models;
using NopStation.Plugin.Widgets.SmartSliders.Services;

namespace NopStation.Plugin.Widgets.SmartSliders.Factories;

public class SmartSliderModelFactory : ISmartSliderModelFactory
{
    #region Fields

    private readonly IPictureService _pictureService;
    private readonly ISmartSliderService _sliderService;
    private readonly ILocalizationService _localizationService;
    private readonly IWorkContext _workContext;
    private readonly INopStationContext _nopStationContext;
    private readonly IStoreContext _storeContext;
    private readonly IStaticCacheManager _cacheManager;
    private readonly ICustomerService _customerService;
    private readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public SmartSliderModelFactory(IPictureService pictureService,
        ISmartSliderService sliderService,
        ILocalizationService localizationService,
        IWorkContext workContext,
        INopStationContext nopStationContext,
        IStoreContext storeContext,
        IStaticCacheManager cacheManager,
        ICustomerService customerService,
        IWebHelper webHelper)
    {
        _pictureService = pictureService;
        _sliderService = sliderService;
        _localizationService = localizationService;
        _workContext = workContext;
        _nopStationContext = nopStationContext;
        _storeContext = storeContext;
        _cacheManager = cacheManager;
        _customerService = customerService;
        _webHelper = webHelper;
    }

    #endregion

    #region Utilities

    protected async Task<IList<SmartSlider>> GetCaruselsAsync(string widgetZone)
    {
        var store = _storeContext.GetCurrentStore();
        var productId = 0;

        if ((await _nopStationContext.GetRouteNameAsync()).Equals("Product", StringComparison.InvariantCultureIgnoreCase))
            productId = _nopStationContext.GetRouteValue(NopRoutingDefaults.RouteValue.ProductId, 0);

        var sliders = await _sliderService.GetAllSlidersAsync(
            productId: productId,
            storeId: store.Id,
            validScheduleOnly: true,
            overrideProduct: true,
            widgetZone: widgetZone);

        return sliders;
    }

    protected async Task<bool> HasSliderItemsAsync(SmartSlider slider, int languageId)
    {
        return (await _sliderService.GetSliderItemsBySliderIdAsync(slider.Id, languageId)).Any();
    }

    protected async Task<IList<SliderModel.SliderItemModel>> PrepareSliderItemListModelAsync(SmartSlider slider, Language language)
    {
        var sliderItems = await _sliderService.GetSliderItemsBySliderIdAsync(slider.Id, language.Id);

        var model = new List<SliderModel.SliderItemModel>();
        foreach (var item in sliderItems)
        {
            var sm = new SliderModel.SliderItemModel
            {
                ButtonText = await _localizationService.GetLocalizedAsync(item, si => si.ButtonText),
                RedirectUrl = await _localizationService.GetLocalizedAsync(item, si => si.RedirectUrl),
                Title = await _localizationService.GetLocalizedAsync(item, si => si.Title),
                Description = await _localizationService.GetLocalizedAsync(item, si => si.Description),
                ContentType = item.ContentType,
                ShowCaption = item.ShowCaption,
                SliderId = item.SliderId,
                Id = item.Id
            };

            switch (item.ContentType)
            {
                case ContentType.Picture:
                    var pictureId = _nopStationContext.MobileDevice ?
                        item.MobilePictureId : item.DesktopPictureId;

                    var picture = await _pictureService.GetPictureByIdAsync(pictureId);
                    var (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture);
                    var pm = new PictureModel
                    {
                        ImageUrl = imageUrl,
                        FullSizeImageUrl = imageUrl,
                        Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                                        ? picture.TitleAttribute : string.Empty,
                        AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                                        ? picture.AltAttribute : string.Empty,
                    };

                    sm.Picture = pm;
                    break;
                case ContentType.Video:
                    //to-do
                    break;
                case ContentType.EmbeddedLink:
                    sm.EmbeddedLink = item.EmbeddedLink;
                    break;
                case ContentType.Text:
                    sm.Text = await _localizationService.GetLocalizedAsync(item, si => si.Text);
                    break;
                default:
                    break;
            }

            model.Add(sm);
        }

        return model;
    }

    #endregion

    #region Methods

    public async Task<IList<SliderOverviewModel>> PrepareSliderAjaxListModelAsync(string widgetZone)
    {
        var sliders = await GetCaruselsAsync(widgetZone);

        var roles = await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
        var language = await _workContext.GetWorkingLanguageAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var model = new List<SliderOverviewModel>();
        foreach (var slider in sliders)
        {
            if (!await HasSliderItemsAsync(slider, language.Id))
                continue;

            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ModelCacheDefaults.SliderOverviewModelKey,
                slider, roles, language, store, _webHelper.IsCurrentConnectionSecured());

            var overviewModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var m = new SliderOverviewModel()
                {
                    Id = slider.Id,
                    ShowBackground = slider.ShowBackground,
                    BackgroundType = slider.BackgroundType,
                    CustomCssClass = slider.CustomCssClass
                };

                if (slider.ShowBackground)
                {
                    m.ShowBackground = slider.ShowBackground;
                    m.BackgroundPictureUrl = await _pictureService.GetPictureUrlAsync(slider.BackgroundPictureId);
                    m.BackgroundType = slider.BackgroundType;
                    m.BackgroundColor = slider.BackgroundColor;
                }

                return m;
            });

            model.Add(overviewModel);
        }

        return model;
    }

    public async Task<IList<SliderModel>> PrepareSliderListModelAsync(string widgetZone)
    {
        var sliders = await GetCaruselsAsync(widgetZone);
        var language = await _workContext.GetWorkingLanguageAsync();

        return await sliders.WhereAwait(async s => await HasSliderItemsAsync(s, language.Id))
            .SelectAwait(async s => await PrepareSliderModelAsync(s)).ToListAsync();
    }

    public async Task<SliderModel> PrepareSliderModelAsync(SmartSlider slider)
    {
        if (slider == null)
            throw new ArgumentNullException(nameof(slider));

        var roles = await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
        var language = await _workContext.GetWorkingLanguageAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ModelCacheDefaults.SliderModelKey,
            slider, _nopStationContext.MobileDevice, roles, language, store, _webHelper.IsCurrentConnectionSecured());

        var model = await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var m = new SliderModel
            {
                Id = slider.Id,
                AutoPlay = slider.EnableAutoPlay,
                CustomCssClass = slider.CustomCssClass,
                AutoPlayHoverPause = slider.AutoPlayHoverPause,
                AutoPlayTimeout = slider.AutoPlayTimeout,
                LazyLoad = slider.EnableLazyLoad,
                Loop = slider.EnableLoop,
                Navigation = slider.EnableNavigation,
                StartPosition = slider.StartPosition,
                KeyboardControl = slider.EnableKeyboardControl,
                KeyboardControlOnlyInViewport = slider.KeyboardControlOnlyInViewport,
                Pagination = slider.EnablePagination,
                PaginationClickable = slider.PaginationClickable,
                PaginationDynamicBullets = slider.PaginationDynamicBullets,
                PaginationDynamicMainBullets = slider.PaginationDynamicMainBullets,
                PaginationType = slider.PaginationType,
                AllowTouchMove = slider.AllowTouchMove,
                AutoHeight = slider.AutoHeight,
                MousewheelControlForceToAxis = slider.MousewheelControlForceToAxis,
                Effect = slider.EnableEffect,
                EffectType = slider.EffectType,
                MousewheelControl = slider.EnableMousewheelControl,
                ToggleZoom = slider.ToggleZoom,
                VerticalDirection = slider.VerticalDirection,
                Zoom = slider.EnableZoom,
                ZoomMaximumRatio = slider.ZoomMaximumRatio,
                ZoomMinimumRatio = slider.ZoomMinimumRatio
            };

            if (slider.ShowBackground)
            {
                m.ShowBackground = slider.ShowBackground;
                m.BackgroundPictureUrl = await _pictureService.GetPictureUrlAsync(slider.BackgroundPictureId);
                m.BackgroundType = slider.BackgroundType;
                m.BackgroundColor = slider.BackgroundColor;
            }

            m.SliderItems = await PrepareSliderItemListModelAsync(slider, language);

            return m;
        });

        return model;
    }

    #endregion
}
