using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Payments;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Payments.Unzer.Components
{
    public class UnzerScript : NopStationViewComponent
    {
        #region Fields

        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public UnzerScript(IPaymentPluginManager paymentPluginManager,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _paymentPluginManager = paymentPluginManager;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async System.Threading.Tasks.Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            if (!await _paymentPluginManager.IsPluginActiveAsync(UnzerPaymentDefaults.SystemName, customer, store?.Id ?? 0))
                return Content(string.Empty);


            if (!widgetZone.Equals(PublicWidgetZones.CheckoutPaymentInfoTop) &&
                !widgetZone.Equals(PublicWidgetZones.OpcContentBefore))
                return Content(string.Empty);

            return View("~/Plugins/NopStation.Plugin.Payments.Unzer/Views/UnzerScript.cshtml");
        }

        #endregion
    }
}
