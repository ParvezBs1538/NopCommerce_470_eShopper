using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Tax;
using NopStation.Plugin.Widgets.ProductPdf.Services.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace NopStation.Plugin.Widgets.ProductPdf.Services
{
    public class ProductPdfService : IProductPdfService
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IPermissionService _permissionService;
        private readonly OrderSettings _orderSettings;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        
        #endregion

        #region Ctor

        public ProductPdfService(IWorkContext workContext,
            ISettingService settingService,
            IPictureService pictureService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            ISpecificationAttributeService specificationAttributeService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IPermissionService permissionService,
            OrderSettings orderSettings,
            ITaxService taxService,
            ICurrencyService currencyService)
        {
            _workContext = workContext;
            _settingService = settingService;
            _pictureService = pictureService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _specificationAttributeService = specificationAttributeService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _permissionService = permissionService;
            _orderSettings = orderSettings;
            _taxService = taxService;
            _currencyService = currencyService;
        }

        #endregion

        #region Utilities

        protected virtual async Task<string> PrepareGroupedProductOverviewPriceModelAsync(Product product)
        {
            var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
                (await _storeContext.GetCurrentStoreAsync()).Id);

            //compare products
            if (!associatedProducts.Any())
                return "";

            //we have at least one associated product
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                //find a minimum possible price
                decimal? minPossiblePrice = null;
                Product minPriceProduct = null;
                foreach (var associatedProduct in associatedProducts)
                {
                    var (_, tmpMinPossiblePrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(associatedProduct, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync());

                    if (associatedProduct.HasTierPrices)
                    {
                        //calculate price for the maximum quantity if we have tier prices, and choose minimal
                        tmpMinPossiblePrice = Math.Min(tmpMinPossiblePrice,
                            (await _priceCalculationService.GetFinalPriceAsync(associatedProduct, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync(), quantity: int.MaxValue)).priceWithoutDiscounts);
                    }

                    if (minPossiblePrice.HasValue && tmpMinPossiblePrice >= minPossiblePrice.Value)
                        continue;
                    minPriceProduct = associatedProduct;
                    minPossiblePrice = tmpMinPossiblePrice;
                }

                if (minPriceProduct == null || minPriceProduct.CustomerEntersPrice)
                    return "";

                if (minPriceProduct.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                     _workContext.OriginalCustomerIfImpersonated == null))
                {
                    return await _localizationService.GetResourceAsync("Products.CallForPrice");
                }
                else
                {
                    //calculate prices
                    var (finalPriceBase, _) = await _taxService.GetProductPriceAsync(minPriceProduct, minPossiblePrice.Value);
                    var finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceBase, await _workContext.GetWorkingCurrencyAsync());

                    return string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"), await _priceFormatter.FormatPriceAsync(finalPrice));
                }
            }
            else
            {
                //hide prices
                return "";
            }
        }

        protected virtual async Task<string> PrepareSimpleProductOverviewPriceModelAsync(Product product)
        {
            //prices
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                if (product.CustomerEntersPrice)
                    return "";

                if (product.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                     _workContext.OriginalCustomerIfImpersonated == null))
                {
                    //call for price
                    return await _localizationService.GetResourceAsync("Products.CallForPrice");
                }
                else
                {
                    //prices
                    var (_, minPossiblePriceWithDiscount, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync());

                    if (product.HasTierPrices)
                    {
                        var (_, tierPriceMinPossiblePriceWithDiscount, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync(), quantity: int.MaxValue);

                        minPossiblePriceWithDiscount = Math.Min(minPossiblePriceWithDiscount, tierPriceMinPossiblePriceWithDiscount);
                    }

                    var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, minPossiblePriceWithDiscount);
                    var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());

                    //do we have tier prices configured?
                    var tierPrices = new List<TierPrice>();
                    if (product.HasTierPrices)
                    {
                        tierPrices.AddRange(await _productService.GetTierPricesAsync(product, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync()));
                    }
                    //When there is just one tier price (with  qty 1), there are no actual savings in the list.
                    var displayFromMessage = tierPrices.Any() && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
                    if (displayFromMessage)
                    {
                        return string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"), await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount));
                    }
                    else
                    {
                        return await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                    }
                }
            }
            else
            {
                return "";
            }
        }
     
        protected async Task<string> GetProductPriceAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            return product.ProductType switch
            {
                ProductType.GroupedProduct => await PrepareGroupedProductOverviewPriceModelAsync(product),//grouped product
                _ => await PrepareSimpleProductOverviewPriceModelAsync(product),//simple product
            };
        }

        protected async Task<Dictionary<string, string>> GetProductSpecificationAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var specificationAttributes = new Dictionary<string, string>();

            var productSpecificationAttributes = await _specificationAttributeService.GetProductSpecificationAttributesAsync(
                    product.Id, showOnProductPage: true);

            foreach (var psa in productSpecificationAttributes)
            {
                var option = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(psa.SpecificationAttributeOptionId);
                var attribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(option.SpecificationAttributeId);
                specificationAttributes.Add(await _localizationService.GetLocalizedAsync(attribute, x => x.Name), await _localizationService.GetLocalizedAsync(option, x => x.Name));
            }

            return specificationAttributes;
        }

        #endregion

        #region Methods

        public virtual async Task PrintProductToPdfAsync(Stream stream, Product product, Language language = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(currentStore.Id);
            var lang = await _workContext.GetWorkingLanguageAsync();
            var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
            var pictureFilePath = await _pictureService.GetThumbLocalPathAsync(picture, 0, false);

            var source = new ProductSource
            {
                Language = lang,
                PageSize = pdfSettingsByStore.LetterPageSizeEnabled ? PageSizes.Letter : PageSizes.A4,
                FontFamily = pdfSettingsByStore.FontFamily,
                StoreName = await _localizationService.GetLocalizedAsync(currentStore, x => x.Name, lang.Id),
                PicturePath = pictureFilePath,
                Name = await _localizationService.GetLocalizedAsync(product, x => x.Name, lang.Id),
                Sku = await _localizationService.GetLocalizedAsync(product, x => x.Sku, lang.Id),
                Price = await GetProductPriceAsync(product),
                ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription, lang.Id),
                SpecificationAttributes = await GetProductSpecificationAsync(product)
            };

            await using var pdfStream = new MemoryStream();

            new ProductDocument(source, _localizationService)
                .GeneratePdf(pdfStream);

            pdfStream.Position = 0;
            await pdfStream.CopyToAsync(stream);
        }

        #endregion
    }
}
