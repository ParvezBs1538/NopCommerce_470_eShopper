using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Payments;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.StripePaymentElement.Services;

namespace NopStation.Plugin.Payments.StripePaymentElement.Components;

public class StripePaymentElementScript : NopStationViewComponent
{
    #region Fields

    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly StripePaymentElementSettings _stripePaymentElementSettings;

    #endregion

    #region Ctor

    public StripePaymentElementScript(IPaymentPluginManager paymentPluginManager,
        IStoreContext storeContext,
        IWorkContext workContext,
        StripePaymentElementSettings stripePaymentElementSettings)
    {
        _paymentPluginManager = paymentPluginManager;
        _storeContext = storeContext;
        _workContext = workContext;
        _stripePaymentElementSettings = stripePaymentElementSettings;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        if (!await _paymentPluginManager.IsPluginActiveAsync(StripeDefaults.SystemName, customer, store?.Id ?? 0))
            return Content(string.Empty);

        if (!StripePaymentElementManager.IsConfigured(_stripePaymentElementSettings))
            return Content(string.Empty);

        if (!widgetZone.Equals(PublicWidgetZones.CheckoutPaymentInfoTop) &&
            !widgetZone.Equals(PublicWidgetZones.OpcContentBefore))
            return Content(string.Empty);

        return View();
    }

    #endregion
}
