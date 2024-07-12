using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.Nagad.Models;

namespace NopStation.Plugin.Payments.Nagad.Components
{
    public class NagadPaymentViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly NagadPaymentSettings _nagadPaymentSettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        #endregion

        #region Ctor

        public NagadPaymentViewComponent(IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            ICurrencyService currencyService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            NagadPaymentSettings nagadPaymentSettings,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _currencyService = currencyService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _nagadPaymentSettings = nagadPaymentSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

        #region Method

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart,
                (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                throw new NopException("Cart is empty");

            var currentCustomerCurrencyId = (await _workContext.GetCurrentCustomerAsync()).CurrencyId;
            var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currentCustomerCurrencyId ?? 0);

            var customerCurrency = (currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync())
                ?? throw new NopException("Currency is not available");
            if (customerCurrency.CurrencyCode != "BDT")
                throw new NopException("Currency is not available");

            var shoppingCartTotal = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
            if (!shoppingCartTotal.shoppingCartTotal.HasValue)
                return Content("");

            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetLocalizedSettingAsync(_nagadPaymentSettings,
                    x => x.Description, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View(model);
        }

        #endregion
    }
}
