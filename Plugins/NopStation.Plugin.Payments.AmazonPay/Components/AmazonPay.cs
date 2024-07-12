using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.AmazonPay.Models;

namespace NopStation.Plugin.Payments.AmazonPay.Components
{
    public class AmazonPayViewComponent : NopStationViewComponent
    {
        private readonly AmazonPaySettings _amazonPaySettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public AmazonPayViewComponent(AmazonPaySettings amazonPaySettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _amazonPaySettings = amazonPaySettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetLocalizedSettingAsync(_amazonPaySettings,
                    x => x.DescriptionText, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.AmazonPay/Views/PaymentInfo.cshtml", model);
        }
    }
}
