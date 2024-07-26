using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.GemVisaPayment.Models;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.GemVisaPayment.Components
{
    [ViewComponent(Name = "GemVisaInstallmentMessage")]
    public class GemVisaInstallmentMessageViewComponent : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public GemVisaInstallmentMessageViewComponent(IWorkContext workContext,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _workContext = workContext;
            _settingService = settingService;
            _storeContext = storeContext;
        }
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var price = (decimal)additionalData;
            var storeScope = _storeContext.CurrentStore.Id;
            var gemSettings = _settingService.LoadSetting<GemVisaPaymentSettings>(storeScope);
            if (!gemSettings.ShowInfoInProductDetailsPage || gemSettings.InstallmentMonth == 0)
                return Content(string.Empty);
            var model = new GemPayInstallmentMessageModel
            {
                Currency = _workContext.WorkingCurrency.CurrencyCode,
                Price = price,
                MarchantId = gemSettings.MerchantId,
                PromotionMonths = gemSettings.InstallmentMonth.ToString(),
                Url = gemSettings.UseSandbox ? string.Format(GemVisaPaymentDefaults.SANDBOX_MESSAGE_URL, gemSettings.MerchantId)
                                           : string.Format(GemVisaPaymentDefaults.MESSAGE_URL, gemSettings.MerchantId)
            };
            return View(model);
        }
    }
}
