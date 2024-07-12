using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace NopStation.Plugin.Payments.StripeWallet.Components
{
    public class StripeWalletScript : NopViewComponent
    {
        #region Fields

        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public StripeWalletScript(IPaymentPluginManager paymentPluginManager,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _paymentPluginManager = paymentPluginManager;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            if (!await _paymentPluginManager.IsPluginActiveAsync(StripeDefaults.SystemName, customer, store?.Id ?? 0))
                return Content(string.Empty);


            if (!widgetZone.Equals(PublicWidgetZones.CheckoutPaymentInfoTop) &&
                !widgetZone.Equals(PublicWidgetZones.OpcContentBefore))
                return Content(string.Empty);

            return View();
        }

        #endregion
    }
}
