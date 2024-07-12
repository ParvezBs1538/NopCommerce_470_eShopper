using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Discounts;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Media;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;
using NopStation.Plugin.Widgets.SmartDealCarousels.Infrastructure.Cache;
using NopStation.Plugin.Widgets.SmartDealCarousels.Models;
using NopStation.Plugin.Widgets.SmartDealCarousels.Services;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Factories;

public partial class SmartDealCarouselModelFactory : ISmartDealCarouselModelFactory
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IPictureService _pictureService;
    private readonly ILocalizationService _localizationService;
    private readonly IStaticCacheManager _cacheManager;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IStoreContext _storeContext;
    private readonly ISmartDealCarouselService _carouselService;
    private readonly IWorkContext _workContext;
    private readonly INopStationContext _nopStationContext;
    private readonly IDiscountService _discountService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly SmartDealCarouselSettings _carouselSettings;
    private readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public SmartDealCarouselModelFactory(ICustomerService customerService,
        IPictureService pictureService,
        ILocalizationService localizationService,
        IStaticCacheManager staticCacheManager,
        IProductModelFactory productModelFactory,
        IStoreContext storeContext,
        ISmartDealCarouselService carouselService,
        IWorkContext workContext,
        INopStationContext nopStationContext,
        IDiscountService discountService,
        IDateTimeHelper dateTimeHelper,
        SmartDealCarouselSettings carouselSettings,
        IWebHelper webHelper)
    {
        _customerService = customerService;
        _pictureService = pictureService;
        _localizationService = localizationService;
        _cacheManager = staticCacheManager;
        _productModelFactory = productModelFactory;
        _storeContext = storeContext;
        _carouselService = carouselService;
        _workContext = workContext;
        _nopStationContext = nopStationContext;
        _discountService = discountService;
        _dateTimeHelper = dateTimeHelper;
        _carouselSettings = carouselSettings;
        _webHelper = webHelper;
    }

    #endregion

    #region Utlities 

    protected async Task<IList<SmartDealCarousel>> GetCaruselsAsync(string widgetZone)
    {
        var store = _storeContext.GetCurrentStore();
        var productId = 0;

        if ((await _nopStationContext.GetRouteNameAsync()).Equals("Product", StringComparison.InvariantCultureIgnoreCase))
            productId = _nopStationContext.GetRouteValue(NopRoutingDefaults.RouteValue.ProductId, 0);
        ;

        var carousels = await _carouselService.GetAllCarouselsAsync(
            productId: productId,
            storeId: store.Id,
            validScheduleOnly: true,
            overrideProduct: true,
            widgetZone: widgetZone);

        return carousels;
    }

    protected async Task<bool> HasSliderItemsAsync(SmartDealCarousel carousel)
    {
        var storeId = _storeContext.GetCurrentStore().Id;

        if (carousel.ProductSourceType != ProductSourceType.CustomProducts)
        {
            var discount = await _discountService.GetDiscountByIdAsync(carousel.DiscountId);
            if (discount == null)
                return false;
            var customer = await _workContext.GetCurrentCustomerAsync();
            var discountCouponCodes = await _customerService.ParseAppliedDiscountCouponCodesAsync(customer);

            var res = await _discountService.ValidateDiscountAsync(discount, customer, discountCouponCodes);
            if (!res.IsValid)
                return false;

            if (discount.RequiresCouponCode ||
                (discount.StartDateUtc.HasValue && discount.StartDateUtc > DateTime.UtcNow) ||
                !discount.EndDateUtc.HasValue || discount.EndDateUtc.Value < DateTime.UtcNow)
                return false;

            if (discount.DiscountType == DiscountType.AssignedToSkus ||
                discount.DiscountType == DiscountType.AssignedToManufacturers ||
                discount.DiscountType == DiscountType.AssignedToCategories)
            {
                var products = await _carouselService.GetProductsWithAppliedDiscountAsync(discount,
                    storeId: storeId,
                    pageSize: 1);

                return products.Any();
            }

            return false;
        }

        return (await _carouselService.GetProductsByCarouselIdAsync(carousel.Id, storeId, 1)).Any();
    }

    protected async Task<DateTime?> GetUserTimeAsync(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return null;

        return await _dateTimeHelper.ConvertToUserTimeAsync(dateTime.Value, DateTimeKind.Utc);
    }

    #endregion

    #region Methods

    public async Task<IList<CarouselOverviewModel>> PrepareCarouselAjaxListModelAsync(string widgetZone)
    {
        var roles = await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
        var language = await _workContext.GetWorkingLanguageAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var carousels = await GetCaruselsAsync(widgetZone);

        var model = new List<CarouselOverviewModel>();
        foreach (var carousel in carousels)
        {
            if (!await HasSliderItemsAsync(carousel))
                continue;

            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ModelCacheDefaults.CarouselOverviewModelKey,
                carousel, roles, language, store, _webHelper.IsCurrentConnectionSecured());

            var overviewModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var m = new CarouselOverviewModel()
                {
                    Title = await _localizationService.GetLocalizedAsync(carousel, x => x.Title),
                    DisplayTitle = carousel.DisplayTitle,
                    Id = carousel.Id,
                    ShowBackground = carousel.ShowBackground,
                    BackgroundType = carousel.BackgroundType,
                    CustomUrl = await _localizationService.GetLocalizedAsync(carousel, x => x.CustomUrl),
                    CustomCssClass = carousel.CustomCssClass
                };

                if (carousel.ShowBackground)
                {
                    m.ShowBackground = carousel.ShowBackground;
                    m.BackgroundPictureUrl = await _pictureService.GetPictureUrlAsync(carousel.BackgroundPictureId);
                    m.BackgroundType = carousel.BackgroundType;
                    m.BackgroundColor = carousel.BackgroundColor;
                }

                if (carousel.ShowCarouselPicture)
                {
                    m.ShowCarouselPicture = true;
                    m.PicturePosition = carousel.PicturePosition;

                    var picture = await _pictureService.GetPictureByIdAsync(carousel.PictureId);
                    string fullSizeImageUrl, imageUrl;
                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _carouselSettings.CarouselPictureSize);
                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    var name = carousel.DisplayTitle ? carousel.Title : carousel.Name;

                    m.Picture = new PictureModel
                    {
                        ImageUrl = imageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        //"title" attribute
                        Title = string.Format(await _localizationService.GetResourceAsync("NopStation.SmartDealCarousels.ImageLinkTitleFormat"), name),
                        //"alt" attribute
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("NopStation.SmartDealCarousels.ImageAlternateTextFormat"), name)
                    };
                }

                if (carousel.ShowCountdown)
                {
                    m.ShowCountdown = carousel.ShowCountdown;

                    if (carousel.ProductSourceType == ProductSourceType.CustomProducts)
                        m.CountdownUntill = await GetUserTimeAsync(carousel.AvaliableDateTimeToUtc);
                    else
                    {
                        var discount = await _discountService.GetDiscountByIdAsync(carousel.DiscountId);
                        m.CountdownUntill = await GetUserTimeAsync(discount?.EndDateUtc);
                    }
                }

                return m;
            });

            model.Add(overviewModel);
        }

        return model;
    }

    public async Task<IList<CarouselModel>> PrepareCarouselListModelAsync(string widgetZone)
    {
        var carousels = await GetCaruselsAsync(widgetZone);

        return await carousels.WhereAwait(async c => await HasSliderItemsAsync(c))
            .SelectAwait(async c => await PrepareCarouselModelAsync(c)).ToListAsync();
    }

    public async Task<CarouselModel> PrepareCarouselModelAsync(SmartDealCarousel carousel)
    {
        if (carousel == null)
            throw new ArgumentNullException(nameof(carousel));

        if (!await HasSliderItemsAsync(carousel))
            throw new InvalidProgramException(nameof(carousel));

        var store = await _storeContext.GetCurrentStoreAsync();
        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ModelCacheDefaults.CarouselModelKey,
            carousel, _carouselSettings.CarouselPictureSize,
            await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()),
            await _workContext.GetWorkingLanguageAsync(),
            store,
             _webHelper.IsCurrentConnectionSecured());

        var model = await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var m = new CarouselModel
            {
                Id = carousel.Id,
                AutoPlay = carousel.EnableAutoPlay,
                CustomCssClass = carousel.CustomCssClass,
                AutoPlayHoverPause = carousel.AutoPlayHoverPause,
                AutoPlayTimeout = carousel.AutoPlayTimeout,
                Center = carousel.Center,
                LazyLoad = carousel.EnableLazyLoad,
                Loop = carousel.EnableLoop,
                Navigation = carousel.EnableNavigation,
                StartPosition = carousel.StartPosition,
                CustomUrl = await _localizationService.GetLocalizedAsync(carousel, x => x.CustomUrl),
                KeyboardControl = carousel.EnableKeyboardControl,
                KeyboardControlOnlyInViewport = carousel.KeyboardControlOnlyInViewport,
                Pagination = carousel.EnablePagination,
                PaginationClickable = carousel.PaginationClickable,
                PaginationDynamicBullets = carousel.PaginationDynamicBullets,
                PaginationDynamicMainBullets = carousel.PaginationDynamicMainBullets,
                PaginationType = carousel.PaginationType,
                ShowCountdown = carousel.ShowCountdown
            };

            if (carousel.ShowBackground)
            {
                m.ShowBackground = true;
                m.BackgroundPictureUrl = await _pictureService.GetPictureUrlAsync(carousel.BackgroundPictureId);
                m.BackgroundType = carousel.BackgroundType;
                m.BackgroundColor = carousel.BackgroundColor;
            }

            if (carousel.ShowCarouselPicture)
            {
                m.ShowCarouselPicture = true;
                m.PicturePosition = carousel.PicturePosition;

                var picture = await _pictureService.GetPictureByIdAsync(carousel.PictureId);
                string fullSizeImageUrl, imageUrl;
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _carouselSettings.CarouselPictureSize);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                var name = carousel.DisplayTitle ? carousel.Title : carousel.Name;

                m.Picture = new PictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    //"title" attribute
                    Title = string.Format(await _localizationService.GetResourceAsync("NopStation.SmartDealCarousels.ImageLinkTitleFormat"), name),
                    //"alt" attribute
                    AlternateText = string.Format(await _localizationService.GetResourceAsync("NopStation.SmartDealCarousels.ImageAlternateTextFormat"), name)
                };
            }

            if (carousel.DisplayTitle)
            {
                m.DisplayTitle = true;
                m.Title = await _localizationService.GetLocalizedAsync(carousel, x => x.Title);
            }

            return m;
        });

        if (carousel.ProductSourceType == ProductSourceType.Discount)
        {
            var discount = await _discountService.GetDiscountByIdAsync(carousel.DiscountId);

            var products = await _carouselService.GetProductsWithAppliedDiscountAsync(discount,
                storeId: store.Id,
                pageSize: carousel.MaxProductsToShow);

            model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            model.CountdownUntill = await GetUserTimeAsync(discount.EndDateUtc);
        }
        else if (carousel.ProductSourceType == ProductSourceType.CustomProducts)
        {
            var products = await _carouselService.GetProductsByCarouselIdAsync(carousel.Id, _storeContext.GetCurrentStore().Id);
            model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();

            model.CountdownUntill = await GetUserTimeAsync(carousel.AvaliableDateTimeToUtc);
        }

        return model;
    }

    #endregion
}