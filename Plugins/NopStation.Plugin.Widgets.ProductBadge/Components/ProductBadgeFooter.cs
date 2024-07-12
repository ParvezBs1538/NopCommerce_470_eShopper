using System;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.ProductBadge.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Components;

public class ProductBadgeFooterViewComponent : NopStationViewComponent
{
    private readonly ProductBadgeSettings _productBadgeSettings;

    public ProductBadgeFooterViewComponent(ProductBadgeSettings productBadgeSettings)
    {
        _productBadgeSettings = productBadgeSettings;
    }

    public IViewComponentResult Invoke(string widgetZone, object additionalData = null)
    {
        var model = new CssModel();
        PrepareDisplayModel(model.Overview.Small, _productBadgeSettings.SmallBadgeWidth);
        PrepareDisplayModel(model.Overview.Medium, _productBadgeSettings.MediumBadgeWidth);
        PrepareDisplayModel(model.Overview.Large, _productBadgeSettings.LargeBadgeWidth);

        var multiplier = (100 + _productBadgeSettings.IncreaseWidthInDetailsPageByPercentage) / 100;
        PrepareDisplayModel(model.Details.Small, _productBadgeSettings.SmallBadgeWidth * multiplier);
        PrepareDisplayModel(model.Details.Medium, _productBadgeSettings.MediumBadgeWidth * multiplier);
        PrepareDisplayModel(model.Details.Large, _productBadgeSettings.LargeBadgeWidth * multiplier);

        return View(model);
    }

    private void PrepareDisplayModel(CssModel.SizeModel size, decimal width)
    {
        size.Width = width;

        size.PentagonSideLength = width / ((1 + (decimal)Math.Sqrt(5)) / 2); //diagonal = a × (1 + √5) / 2
        size.PentagonHeight = size.PentagonSideLength * (decimal)Math.Sqrt(5 + 2 * Math.Sqrt(5)) / 2; //height = a × √(5 + 2√5) / 2
    }
}