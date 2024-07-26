using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Payments.AfterpayPayment.Models;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.AfterpayPayment.Components
{
    [ViewComponent(Name = "PaymentAfterpay")]
    public class PaymentAfterpayViewComponent : NopViewComponent
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly TaxSettings _taxSettings;
        private readonly ICurrencyService _currencyService;

        public PaymentAfterpayViewComponent(IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            TaxSettings taxSettings,
            ICurrencyService currencyService)
        {
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _storeContext = storeContext;

            _orderTotalCalculationService = orderTotalCalculationService;
            _taxSettings = taxSettings;
            _currencyService = currencyService;
        }
        public IViewComponentResult Invoke()
        {
            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            var subTotalIncludingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
            _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax, out var _, out var _, out var subTotalWithoutDiscountBase, out var _);
            var subtotalBase = subTotalWithoutDiscountBase;
            var subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
            var amount = new Amount
            {
                PaymentAmount = subtotal.ToString(),
                Currency = _workContext.WorkingCurrency.CurrencyCode
            };
            return View("~/Plugins/Payments.AfterpayPayment/Views/PaymentInfo.cshtml", amount);

        }
    }
}
