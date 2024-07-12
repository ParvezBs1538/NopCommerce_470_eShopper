using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using NopStation.Plugin.Payments.EveryPay.Models;
using Nop.Services.Directory;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Payments.EveryPay.Components
{
    public class EveryPayViewComponent : NopStationViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly EveryPaySettings _everyPaySettings;

        public EveryPayViewComponent(IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICurrencyService currencyService,
            EveryPaySettings everyPaySettings)
        {
            _storeContext = storeContext;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;
            _everyPaySettings = everyPaySettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, _storeContext.GetCurrentStore().Id);
            var shoppingCartTotalBase = (await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart, usePaymentMethodAdditionalFee: false)).shoppingCartTotal ?? decimal.Zero;
            var amount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase, await _workContext.GetWorkingCurrencyAsync());

            var model = new PaymentInfoModel();
            var installmentArray = string.IsNullOrEmpty(_everyPaySettings.Installments) ? Array.Empty<int>() : _everyPaySettings.Installments.Split(',').Where(x => int.TryParse(x, out _)).Select(int.Parse).ToArray();
            model.Installments = installmentArray;
            model.IsSandbox = _everyPaySettings.UseSandbox;
            model.ApiKey = _everyPaySettings.ApiKey.ToString();
            model.Amount = Convert.ToInt32(amount * 100);

            return View("~/Plugins/NopStation.Plugin.Payments.EveryPay/Views/PaymentInfo.cshtml", model);
        }
    }
}
