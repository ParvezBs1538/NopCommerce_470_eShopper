using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Nop.Core;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Components
{
    public class BlueSnapScriptViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly BlueSnapSettings _blueSnapSettings;

        #endregion

        #region Ctor

        public BlueSnapScriptViewComponent(IPaymentPluginManager paymentPluginManager,
            IStoreContext storeContext,
            IWorkContext workContext,
            BlueSnapSettings blueSnapSettings)
        {
            _paymentPluginManager = paymentPluginManager;
            _storeContext = storeContext;
            _workContext = workContext;
            _blueSnapSettings = blueSnapSettings;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            if (!await _paymentPluginManager.IsPluginActiveAsync(BlueSnapDefaults.SystemName, customer, store?.Id ?? 0))
                return Content(string.Empty);


            if (!widgetZone.Equals(PublicWidgetZones.CheckoutPaymentInfoTop) &&
                !widgetZone.Equals(PublicWidgetZones.OpcContentBefore) &&
                !widgetZone.Equals(PublicWidgetZones.ProductDetailsTop) &&
                !widgetZone.Equals(PublicWidgetZones.OrderSummaryContentBefore))
            {
                return Content(string.Empty);
            }

            var scriptUrl = _blueSnapSettings.IsSandBox ? "https://sandbox.bluesnap.com/web-sdk/4/bluesnap.js" : "https://bluesnap.com/web-sdk/4/bluesnap.js";

            var script = $@"<script src=""{scriptUrl}"" ""></script>";

            return new HtmlContentViewComponentResult(new HtmlString(script ?? string.Empty));
        }

        #endregion
    }
}
