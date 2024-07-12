using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.ProductBadge.Domains;
using NopStation.Plugin.Widgets.ProductBadge.Models;
using NopStation.Plugin.Widgets.ProductBadge.Services;

namespace NopStation.Plugin.Widgets.ProductBadge.Factories;

public class BadgeModelFactory : IBadgeModelFactory
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IPictureService _pictureService;
    private readonly IBadgeService _badgeService;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IStaticCacheManager _cacheManager;
    private readonly ICurrencyService _currencyService;
    private readonly IWebHelper _webHelper;
    private readonly INopStationContext _nopStationContext;
    private readonly ProductBadgeSettings _productBadgeSettings;

    #endregion

    #region Ctor

    public BadgeModelFactory(ILocalizationService localizationService,
        IPictureService pictureService,
        IBadgeService badgeService,
        IPriceCalculationService priceCalculationService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IStaticCacheManager cacheManager,
        ICurrencyService currencyService,
        IWebHelper webHelper,
        INopStationContext nopStationContext,
        ProductBadgeSettings productBadgeSettings)
    {
        _localizationService = localizationService;
        _pictureService = pictureService;
        _badgeService = badgeService;
        _priceCalculationService = priceCalculationService;
        _workContext = workContext;
        _storeContext = storeContext;
        _cacheManager = cacheManager;
        _currencyService = currencyService;
        _webHelper = webHelper;
        _nopStationContext = nopStationContext;
        _productBadgeSettings = productBadgeSettings;
    }

    #endregion

    #region Utilities

    protected async Task<BadgeModel> PrepareBadgeModelAsync(Badge badge, Product product, bool detailsPage = true)
    {
        var pictureSize = badge.Size switch
        {
            Size.Small => _productBadgeSettings.SmallBadgeWidth,
            Size.Medium => _productBadgeSettings.MediumBadgeWidth,
            _ => _productBadgeSettings.LargeBadgeWidth,
        };

        if (detailsPage)
            pictureSize = (int)((100 + _productBadgeSettings.IncreaseWidthInDetailsPageByPercentage) / 100 * pictureSize);

        var cacheKey = _cacheManager.PrepareKeyForDefaultCache(CacheDefaults.BadgeModelKey, badge.Id,
            _nopStationContext.MobileDevice, await _workContext.GetWorkingLanguageAsync(),
            _storeContext.GetCurrentStore(), _webHelper.IsCurrentConnectionSecured(), pictureSize);

        var cachedModel = await _cacheManager.GetAsync(cacheKey, async () =>
        {
            var model = new BadgeModel()
            {
                BackgroundColor = badge.BackgroundColor,
                ContentType = badge.ContentType,
                CssClass = badge.CssClass,
                Id = badge.Id,
                PositionType = badge.PositionType,
                ShapeType = badge.ShapeType,
                Text = await _localizationService.GetLocalizedAsync(badge, b => b.Text),
                Size = badge.Size,
                FontColor = badge.FontColor
            };

            if (badge.ContentType == ContentType.Picture)
                model.PictureUrl = await _pictureService.GetPictureUrlAsync(badge.PictureId, pictureSize);

            return model;
        });

        if (badge.ContentType == ContentType.Text && badge.BadgeType == BadgeType.DiscountedProducts)
        {
            var (_, _, appliedDiscountAmount, appliedDiscounts) = await _priceCalculationService.GetFinalPriceAsync(product, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync());
            var discount = appliedDiscounts.FirstOrDefault();

            if (discount.UsePercentage)
            {
                var percentage = Math.Round(discount.DiscountPercentage, 2).ToString("0.##");
                cachedModel.Text = await GetFormattedTextAsync(badge, $"{percentage}%");
            }
            else
            {
                var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                var amount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(discount.DiscountAmount, currentCurrency);
                var amountStr = GetCurrencyString(amount, currentCurrency);

                cachedModel.Text = await GetFormattedTextAsync(badge, amountStr);
            }
        }

        return cachedModel;
    }

    protected virtual string GetCurrencyString(decimal amount, Currency currency)
    {
        string result;
        if (!string.IsNullOrEmpty(currency.CustomFormatting))
            //custom formatting specified by a store owner
            result = amount.ToString(currency.CustomFormatting);
        else
        {
            if (!string.IsNullOrEmpty(currency.DisplayLocale))
                //default behavior
                result = amount.ToString("C", new CultureInfo(currency.DisplayLocale));
            else
            {
                //not possible because "DisplayLocale" should be always specified
                //but anyway let's just handle this behavior
                result = $"{amount:N} ({currency.CurrencyCode})";
                return result;
            }
        }

        result = $"{result} ({currency.CurrencyCode})";
        return result;
    }

    protected async Task<string> GetFormattedTextAsync(Badge badge, string amountStr)
    {
        var text = await _localizationService.GetLocalizedAsync(badge, b => b.Text);

        if (badge.DiscountBadgeTextFormat == DiscountBadgeTextFormat.DiscountAfterText)
            return $"{text} {amountStr}";

        return $"{amountStr} {text}";
    }

    #endregion

    #region Methods

    public async Task<BadgeInfoModel> PrepareProductBadgeInfoModelAsync(Product product, bool detailsPage = true)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        var model = new BadgeInfoModel()
        {
            ProductId = product.Id,
            DetailsPage = detailsPage
        };

        var badges = await _badgeService.GetProductBadgesAsync(product);
        foreach (var badge in badges)
            model.Badges.Add(await PrepareBadgeModelAsync(badge, product, detailsPage));

        return model;
    }

    #endregion
}