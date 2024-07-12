using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.Misc.AjaxFilter.Factories
{
    public class AjaxFilterProductModelFactory : ProductModelFactory
    {
        private readonly ISettingService _settingService;

        #region Ctor

        public AjaxFilterProductModelFactory(CaptchaSettings captchaSettings,
        CatalogSettings catalogSettings,
        CustomerSettings customerSettings,
        ICategoryService categoryService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IDateRangeService dateRangeService,
        IDateTimeHelper dateTimeHelper,
        IDownloadService downloadService,
        IGenericAttributeService genericAttributeService,
        IJsonLdModelFactory jsonLdModelFactory,
        ILocalizationService localizationService,
        IManufacturerService manufacturerService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductService productService,
        IProductTagService productTagService,
        IProductTemplateService productTemplateService,
        IReviewTypeService reviewTypeService,
        IShoppingCartService shoppingCartService,
        ISpecificationAttributeService specificationAttributeService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IStoreService storeService,
        IShoppingCartModelFactory shoppingCartModelFactory,
        ITaxService taxService,
        IUrlRecordService urlRecordService,
        IVendorService vendorService,
        IVideoService videoService,
        IWebHelper webHelper,
        IWorkContext workContext,
        MediaSettings mediaSettings,
        OrderSettings orderSettings,
        SeoSettings seoSettings,
        ShippingSettings shippingSettings,
        VendorSettings vendorSettings,
        ISettingService settingService) : base(captchaSettings,
            catalogSettings,
            customerSettings,
            categoryService,
            currencyService,
            customerService,
            dateRangeService,
            dateTimeHelper,
            downloadService,
            genericAttributeService,
            jsonLdModelFactory,
            localizationService,
            manufacturerService,
            permissionService,
            pictureService,
            priceCalculationService,
            priceFormatter,
            productAttributeParser,
            productAttributeService,
            productService,
            productTagService,
            productTemplateService,
            reviewTypeService,
            shoppingCartService,
            specificationAttributeService,
            staticCacheManager,
            storeContext,
            storeService,
            shoppingCartModelFactory,
            taxService,
            urlRecordService,
            vendorService,
            videoService,
            webHelper,
            workContext,
            mediaSettings,
            orderSettings,
            seoSettings,
            shippingSettings,
            vendorSettings)
        {
            _settingService = settingService;
            _settingService = settingService;
        }

        #endregion

        public override async Task<ProductDetailsModel> PrepareProductDetailsModelAsync(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false)
        {
            var model = await base.PrepareProductDetailsModelAsync(product, updatecartitem, isAssociatedProduct);

            var ajaxFilterSettings = await _settingService.LoadSettingAsync<AjaxFilterSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

            model.CustomProperties.Add("EnableGeneralSpecification", ajaxFilterSettings.EnableGeneralSpecifications.ToString());

            return model;
        }
    }
}
