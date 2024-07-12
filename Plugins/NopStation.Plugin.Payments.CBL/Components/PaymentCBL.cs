using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Payments;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Payments.CBL.Components
{
    public class PaymentCBLViewComponent : NopStationViewComponent
    {
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public PaymentCBLViewComponent(IPaymentPluginManager paymentPluginManager,
            IWorkContext workContext,
            IStoreContext storeContext)
        {
            _paymentPluginManager = paymentPluginManager;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            if (!await _paymentPluginManager.IsPluginActiveAsync(CBLPaymentDefaults.PLUGIN_SYSTEM_NAME, customer, store?.Id ?? 0))
                return Content(string.Empty);

            return View();
        }
    }
}
