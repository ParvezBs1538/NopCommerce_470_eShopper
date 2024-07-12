using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Misc.QuoteCart.Services;

namespace NopStation.Plugin.Misc.QuoteCart.Components;

[ViewComponent(Name = QuoteCartDefaults.FLYOUT_QUOTE_CART_COMPONENT_NAME)]
public class FlyoutCartViewComponent : NopStationViewComponent
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IQuoteCartService _quoteCartService;
    private readonly IWorkContext _workContext;
    private readonly QuoteCartSettings _quoteCartSettings;

    #endregion

    #region Ctor

    public FlyoutCartViewComponent(
        IPermissionService permissionService,
        IQuoteCartService quoteCartService,
        IWorkContext workContext,
        QuoteCartSettings quoteCartSettings)
    {
        _quoteCartSettings = quoteCartSettings;
        _permissionService = permissionService;
        _quoteCartService = quoteCartService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (_quoteCartSettings.EnableQuoteCart && await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.SendQuoteRequest))
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var itemTotal = await _quoteCartService.GetQuoteCartAsync(customer);
            return View(itemTotal.Sum(x => x.Quantity));
        }
        return Content(string.Empty);
    }

    #endregion
}
