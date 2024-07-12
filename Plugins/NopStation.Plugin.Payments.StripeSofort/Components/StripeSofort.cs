using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.StripeSofort.Models;

namespace NopStation.Plugin.Payments.StripeSofort.Components
{
    public class StripeSofortViewComponent : NopStationViewComponent
    {
        #region Field

        private readonly IWorkContext _workContext;
        private readonly StripeSofortPaymentSettings _sofortPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public StripeSofortViewComponent(IWorkContext workContext,
            StripeSofortPaymentSettings sofortPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext)
        {
            _workContext = workContext;
            _sofortPaymentSettings = sofortPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods 

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetLocalizedSettingAsync(_sofortPaymentSettings,
                    x => x.DescriptionText, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.StripeSofort/Views/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}
