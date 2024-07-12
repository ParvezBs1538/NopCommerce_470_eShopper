using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.Instamojo.Models;

namespace NopStation.Plugin.Payments.Instamojo.Components
{
    public class InstamojoViewComponent : NopStationViewComponent
    {
        private readonly InstamojoPaymentSettings _instamojoPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public InstamojoViewComponent(InstamojoPaymentSettings instamojoPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _instamojoPaymentSettings = instamojoPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                Description = await _localizationService.GetLocalizedSettingAsync(_instamojoPaymentSettings,
                    x => x.Description, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.Instamojo/Views/PaymentInfo.cshtml", model);
        }
    }
}
