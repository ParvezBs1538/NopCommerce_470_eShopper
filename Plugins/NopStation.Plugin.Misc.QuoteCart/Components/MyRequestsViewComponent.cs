using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Web.Models.Customer;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Misc.QuoteCart.Components;

[ViewComponent(Name = QuoteCartDefaults.MY_REQUESTS_COMPONENT_NAME)]
public class MyRequestsViewComponent : NopStationViewComponent
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly QuoteCartSettings _quoteCartSettings;

    #endregion

    #region Ctor

    public MyRequestsViewComponent(IPermissionService permissionService, QuoteCartSettings quoteCartSettings)
    {
        _permissionService = permissionService;
        _quoteCartSettings = quoteCartSettings;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(string _, object additionalData)
    {
        if (_quoteCartSettings.EnableQuoteCart && await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.SendQuoteRequest) && additionalData is CustomerNavigationModel model)
        {
            return View(model);
        }

        return Content(string.Empty);
    }

    #endregion
}
