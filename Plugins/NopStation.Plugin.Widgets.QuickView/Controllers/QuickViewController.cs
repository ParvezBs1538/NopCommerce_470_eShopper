using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Widgets.QuickView.Models;

namespace NopStation.Plugin.Widgets.QuickView.Controllers;

public class QuickViewController : BasePluginController
{
    #region Fields

    private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IStoreMappingService _storeMappingService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly QuickViewSettings _quickViewSettings;
    private readonly CatalogSettings _catalogSettings;
    private readonly IProductService _productService;
    private readonly IAclService _aclService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IPluginService _pluginService;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly ILogger _logger;

    #endregion

    #region Ctor

    public QuickViewController(IRecentlyViewedProductsService recentlyViewedProductsService,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        IProductModelFactory productModelFactory,
        IStoreMappingService storeMappingService,
        IPermissionService permissionService,
        QuickViewSettings quickViewSettings,
        IProductService productService,
        CatalogSettings catalogSettings,
        IAclService aclService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IPluginService pluginService,
        IWidgetPluginManager widgetPluginManager,
        ILogger logger)
    {
        _recentlyViewedProductsService = recentlyViewedProductsService;
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _storeMappingService = storeMappingService;
        _productModelFactory = productModelFactory;
        _permissionService = permissionService;
        _quickViewSettings = quickViewSettings;
        _productService = productService;
        _catalogSettings = catalogSettings;
        _aclService = aclService;
        _workContext = workContext;
        _storeContext = storeContext;
        _pluginService = pluginService;
        _widgetPluginManager = widgetPluginManager;
        _logger = logger;
    }

    #endregion

    #region Product details page

    //[HttpsRequirement(SslRequirement.No)]
    public async Task<IActionResult> ProductDetails(int productId, int updatecartitemid = 0)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted)
            return NotFound();

        var notAvailable =
            (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
            !await _aclService.AuthorizeAsync(product) ||
            !await _storeMappingService.AuthorizeAsync(product) ||
            !_productService.ProductIsAvailable(product);
        var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts);
        if (notAvailable && !hasAdminAccess)
            return NotFound();

        if (!product.VisibleIndividually)
        {
            var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
            if (parentGroupedProduct == null)
                product = parentGroupedProduct;
            else
                return NotFound();
        }

        var model = new QuickViewProductDetailsModel()
        {
            ProductDetailsModel = await _productModelFactory.PrepareProductDetailsModelAsync(product, null, false),
            ShowAlsoPurchasedProducts = _quickViewSettings.ShowAlsoPurchasedProducts,
            ShowRelatedProducts = _quickViewSettings.ShowRelatedProducts,
            ShowAddToWishlistButton = _quickViewSettings.ShowAddToWishlistButton,
            ShowAvailability = _quickViewSettings.ShowAvailability,
            ShowProductEmailAFriendButton = _quickViewSettings.ShowProductEmailAFriendButton,
            Id = product.Id,
            ShowCompareProductsButton = _quickViewSettings.ShowCompareProductsButton,
            ShowDeliveryInfo = _quickViewSettings.ShowDeliveryInfo,
            ShowFullDescription = _quickViewSettings.ShowFullDescription,
            ShowProductManufacturers = _quickViewSettings.ShowProductManufacturers,
            ShowProductReviewOverview = _quickViewSettings.ShowProductReviewOverview,
            ShowShortDescription = _quickViewSettings.ShowShortDescription,
            ShowProductSpecifications = _quickViewSettings.ShowProductSpecifications,
            ShowProductTags = _quickViewSettings.ShowProductTags,
        };

        if (model.ProductDetailsModel.CustomProperties.ContainsKey("AjaxLoad"))
            model.ProductDetailsModel.CustomProperties.Remove("AjaxLoad");

        model.ProductDetailsModel.CustomProperties["AjaxLoad"] = "true";
        var productTemplateViewPath = await _productModelFactory.PrepareProductTemplateViewPathAsync(product);

        await _recentlyViewedProductsService.AddProductToRecentlyViewedListAsync(product.Id);
        await _customerActivityService.InsertActivityAsync("PublicStore.ViewProduct", await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewProduct"), product);

        try
        {
            var pluginDescriptor = _pluginService.GetPluginDescriptorBySystemNameAsync<IWidgetPlugin>("NopStation.PictureZoom",
                LoadPluginsMode.InstalledOnly, _workContext.GetCurrentCustomerAsync().Result, _storeContext.GetCurrentStoreAsync().Result.Id).Result;

            if (pluginDescriptor != null && _widgetPluginManager.IsPluginActive(pluginDescriptor.Instance<IWidgetPlugin>()) && _quickViewSettings.EnablePictureZoom)
                model.PictureZoomEnabled = true;

            var html = await RenderPartialViewToStringAsync($"QuickView{productTemplateViewPath}", model);

            return Json(new
            {
                html
            });
        }
        catch (System.Exception ex)
        {
            await _logger.ErrorAsync(ex.Message, ex);

            return Json(new
            {
                html = ""
            });
        }
    }

    #endregion
}
