using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Security;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Misc.QuoteCart.Models;
using NopStation.Plugin.Misc.QuoteCart.Services;

namespace NopStation.Plugin.Misc.QuoteCart.Components;

[ViewComponent(Name = QuoteCartDefaults.ADD_QUOTE_COMPONENT_NAME)]
public class AddQuoteViewComponent : NopStationViewComponent
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IQuoteRequestWhitelistService _quoteRequestWhitelistService;
    private readonly QuoteCartSettings _quoteCartSettings;

    #endregion

    #region Ctor

    public AddQuoteViewComponent(
        IPermissionService permissionService,
        IQuoteRequestWhitelistService quoteRequestWhitelistService,
        QuoteCartSettings quoteCartSettings)
    {
        _permissionService = permissionService;
        _quoteRequestWhitelistService = quoteRequestWhitelistService;
        _quoteCartSettings = quoteCartSettings;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(string _, object additionalData)
    {
        if (!_quoteCartSettings.EnableQuoteCart || !await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.SendQuoteRequest))
            return Content(string.Empty);

        if (additionalData is ProductOverviewModel model)
        {
            if (model.ProductType == ProductType.GroupedProduct || !await _quoteRequestWhitelistService.CanQuoteAsync(model.Id))
                return Content(string.Empty);

            var quoteButtonModel = new QuoteButtonModel
            {
                ProductId = model.Id,
                AddToCartButtonEnabled = true,
            };

            return View(quoteButtonModel);
        }

        if (additionalData is ProductDetailsModel productDetailsModel)
        {
            if (!await _quoteRequestWhitelistService.CanQuoteAsync(productDetailsModel.Id))
                return Content(string.Empty);

            var quoteButtonModel = new QuoteButtonModel
            {
                ProductId = productDetailsModel.Id,
                AddToCartButtonEnabled = !productDetailsModel.AddToCart.DisableBuyButton,
                IsProductDetails = true,
                AllowedQuantities = productDetailsModel.AddToCart.AllowedQuantities,
                EnteredQuantity = 1
            };

            return View(quoteButtonModel);
        }

        return Content(string.Empty);
    }

    #endregion
}
