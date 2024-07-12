using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Tax;
using NopStation.Plugin.Widgets.ProductRibbon.Infrastructure.Cache;
using NopStation.Plugin.Widgets.ProductRibbon.Models;
using NopStation.Plugin.Widgets.ProductRibbon.Services;

namespace NopStation.Plugin.Widgets.ProductRibbon.Factories
{
    public class ProductRibbonModelFactory : IProductRibbonModelFactory
    {
        #region Fields

        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ProductRibbonSettings _productRibbonSettings;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IBestSellerService _bestSellerService;

        #endregion

        #region Ctor

        public ProductRibbonModelFactory(ITaxService taxService,
            IPriceCalculationService priceCalculationService,
            ProductRibbonSettings productRibbonSettings,
            IStaticCacheManager staticCacheManger,
            IPermissionService permissionService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IProductService productService,
            IBestSellerService bestSellerService)
        {
            _taxService = taxService;
            _priceCalculationService = priceCalculationService;
            _productRibbonSettings = productRibbonSettings;
            _staticCacheManager = staticCacheManger;
            _permissionService = permissionService;
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _productService = productService;
            _bestSellerService = bestSellerService;
        }

        #endregion

        public async Task<ProductRibbonModel> PrepareProductRibbonModelAsync(Product product)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ModelCacheEventConsumer.PRODUCT_RIBBON_MODEL_KEY,
                product.Id, (await _workContext.GetCurrentCustomerAsync()).Id, (await _workContext.GetWorkingLanguageAsync()).Id);

            var cachedModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new ProductRibbonModel();

                if (_productRibbonSettings.EnableBestSellerRibbon)
                {
                    var bestSellerReport = await _bestSellerService.BestSellerReportAsync(product.Id);
                    model.IsBestSeller = bestSellerReport != null
                        && bestSellerReport.TotalAmount > _productRibbonSettings.MinimumAmountSold
                        && bestSellerReport.TotalQuantity > _productRibbonSettings.MinimumQuantitySold;
                }

                if (_productRibbonSettings.EnableNewRibbon)
                {
                    model.IsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow);
                }

                if (_productRibbonSettings.EnableDiscountRibbon &&
                    product.ProductType == ProductType.SimpleProduct &&
                    await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices) &&
                    !product.CustomerEntersPrice && !product.CallForPrice)
                {
                    var (price, taxRate) = await _taxService.GetProductPriceAsync(product,
                        (await _priceCalculationService.GetFinalPriceAsync(product, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync(), includeDiscounts: true)).finalPrice);
                    var specialPrice = (await _productService.GetTierPricesByProductAsync(product.Id)).FirstOrDefault(x => x.Quantity == 0 && x.CustomerRoleId == null);
                    var mRP = product.OldPrice > 0 ? product.OldPrice : product.Price;
                    var salePrice = specialPrice != null ? specialPrice.Price : price;

                    if (mRP > 0)
                    {
                        var save = mRP - salePrice;
                        if (save > 0)
                        {
                            var productPrice = (int)Math.Ceiling(save * 100 / mRP);
                            model.Discount = string.Format(await _localizationService.GetResourceAsync("NopStation.ProductRibbon.RibbonText.Discount"), productPrice);
                        }
                    }
                }

                return model;
            });

            return cachedModel;
        }
    }
}
