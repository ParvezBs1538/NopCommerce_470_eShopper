//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Nop.Core;
//using NopStation.Plugin.Misc.Core.Components;
//using NopStation.Plugin.Payments.CreditWallet.Services;
//using NopStation.Plugin.Payments.CreditWallet.Models;
//using Nop.Services.Configuration;
//using Nop.Services.Directory;
//using Nop.Services.Localization;

//namespace NopStation.Plugin.Payments.CreditWallet.Components
//{
//    [ViewComponent]
//    public class CustomerCreditPaymentViewComponent : NopStationViewComponent
//    {
//        private readonly ILocalizationService _localizationService;
//        private readonly ISettingService _settingService;
//        private readonly IStoreContext _storeContext;
//        private readonly IWorkContext _workContext;
//        private IWalletService _customerCreditDetailsService;
//        private ICurrencyService _currencyService;

//        public CustomerCreditPaymentViewComponent(ILocalizationService localizationService,
//            ISettingService settingService,
//            IStoreContext storeContext,
//            IWorkContext workContext,
//            IWalletService customerCreditDetailsService,
//            ICurrencyService currencyService)
//        {
//            _localizationService = localizationService;
//            _settingService = settingService;
//            _storeContext = storeContext;
//            _workContext = workContext;
//            _customerCreditDetailsService = customerCreditDetailsService;
//            _currencyService = currencyService;
//        }

//        public async Task<IViewComponentResult> InvokeAsync()
//        {
//            var currentStore = await _storeContext.GetCurrentStoreAsync();
//            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
//            var creditPaymentSettings = await _settingService.LoadSettingAsync<CustomerCreditPaymentSettings>(currentStore.Id);

//            var customer = await _workContext.GetCurrentCustomerAsync();
//            var customerCreditDetails = await _customerCreditDetailsService.GetWalletByCustomerIdAsync(customer.Id);

//            var currentCurrencey = await _workContext.GetWorkingCurrencyAsync();
//            var currency = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(customerCreditDetails.AvailableCredit, currentCurrencey);
//            var currencyCode = currentCurrencey.CurrencyCode;

//            var model = new PaymentInfoModel
//            {
//                DescriptionText = await _localizationService.GetLocalizedSettingAsync(creditPaymentSettings, x => x.DescriptionText, currentLanguage.Id, 0),
//                AvailableCredit = currency.ToString("0.00"),
//                PrimaryStoreCurrencyCode = currencyCode,
//            };
//            if (creditPaymentSettings.MinimumCredit >= customerCreditDetails.AvailableCredit)
//            {
//                model.HaveWarningMsg = true;
//                model.WarningMsg = "You have lower credit";
//            }

//            return View(model);
//        }
//    }
//}
