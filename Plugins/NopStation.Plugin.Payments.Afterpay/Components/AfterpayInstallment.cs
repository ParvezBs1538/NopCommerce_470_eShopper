using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Payments;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Payments.Afterpay.Components
{
    public class AfterpayInstallmentViewComponent : NopStationViewComponent
    {
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public AfterpayInstallmentViewComponent(IPaymentPluginManager paymentPluginManager,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _paymentPluginManager = paymentPluginManager;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            if (!await _paymentPluginManager.IsPluginActiveAsync(AfterpayPaymentDefaults.PLUGIN_SYSTEM_NAME, customer, store?.Id ?? 0))
                return Content(string.Empty);

            var price = (decimal)additionalData;
            return View(price);
        }
    }
}