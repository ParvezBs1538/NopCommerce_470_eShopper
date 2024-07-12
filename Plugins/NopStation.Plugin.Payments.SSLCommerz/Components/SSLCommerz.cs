using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using NopStation.Plugin.Payments.SSLCommerz.Models;

namespace NopStation.Plugin.Payments.SSLCommerz.Components
{
    public class SSLCommerzViewComponent : NopViewComponent
    {
        private readonly SSLCommerzPaymentSettings _commerzPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public SSLCommerzViewComponent(SSLCommerzPaymentSettings commerzPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _commerzPaymentSettings = commerzPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetLocalizedSettingAsync(_commerzPaymentSettings,
                    x => x.DescriptionText, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.SSLCommerz/Views/PaymentInfo.cshtml", model);
        }
    }
}
