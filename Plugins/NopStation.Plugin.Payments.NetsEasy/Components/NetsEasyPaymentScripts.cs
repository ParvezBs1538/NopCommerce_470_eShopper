using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.NetsEasy.Models;

namespace NopStation.Plugin.Payments.NetsEasy.Components
{
    public class NetsEasyPaymentScriptsViewComponent : NopStationViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;

        public NetsEasyPaymentScriptsViewComponent(IStoreContext storeContext,
            ISettingService settingService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            //load settings for a chosen store scope
            var storeScope = (await _storeContext.GetCurrentStoreAsync()).Id;
            var netsEasyPaymentSettings = await _settingService.LoadSettingAsync<NetsEasyPaymentSettings>(storeScope);

            var model = new PaymentScriptModel
            {
                CheckoutScriptUrl = netsEasyPaymentSettings.TestMode
                    ? NetsEasyPaymentDefaults.TestCheckoutScriptUrl
                    : NetsEasyPaymentDefaults.LiveCheckoutScriptUrl
            };

            return View(model);
        }
    }
}
