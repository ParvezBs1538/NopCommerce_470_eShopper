using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.ProductBadge.Factories;
using NopStation.Plugin.Widgets.ProductBadge.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Components;

public class ProductBadgeViewComponent : NopStationViewComponent
{
    #region Fields

    private readonly IProductService _productService;
    private readonly ProductBadgeSettings _productBadgeSettings;
    private readonly IBadgeModelFactory _badgeModelFactory;

    #endregion

    #region Ctor

    public ProductBadgeViewComponent(IProductService productService,
        ProductBadgeSettings productBadgeSettings,
        IBadgeModelFactory badgeModelFactory)
    {
        _productService = productService;
        _productBadgeSettings = productBadgeSettings;
        _badgeModelFactory = badgeModelFactory;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
    {
        if (additionalData.GetType() != typeof(ProductDetailsModel) && additionalData.GetType() != typeof(ProductOverviewModel))
            return Content("");

        var detailsPage = additionalData.GetType() == typeof(ProductDetailsModel);
        var productId = detailsPage ? (additionalData as ProductDetailsModel).Id : (additionalData as ProductOverviewModel).Id;

        if (_productBadgeSettings.EnableAjaxLoad)
        {
            var model = new BadgeAjaxModel
            {
                WidgetZone = widgetZone,
                DetailsPage = detailsPage,
                ProductId = productId
            };

            return View("Default.Ajax", model);
        }
        else
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted)
                return Content("");

            var model = await _badgeModelFactory.PrepareProductBadgeInfoModelAsync(product, detailsPage);
            return View(model);
        }
    }

    #endregion
}