using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Http.Extensions;
using Nop.Services.Media;
using Nop.Services.Payments;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.MPay24.Models;
using NopStation.Plugin.Payments.MPay24.Services;

namespace NopStation.Plugin.Payments.MPay24.Components
{
    public class PaymentMPay24ViewComponent : NopStationViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IPaymentOptionService _paymentOptionService;
        private readonly IPictureService _pictureService;

        public PaymentMPay24ViewComponent(IStoreContext storeContext,
            IPaymentOptionService paymentOptionService,
            IPictureService pictureService)
        {
            _storeContext = storeContext;
            _paymentOptionService = paymentOptionService;
            _pictureService = pictureService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var paymentOptions = await _paymentOptionService.GetAllMPay24PaymentOptionsAsync(
                active: true, storeId: _storeContext.GetCurrentStore().Id);

            var selectedShortName = "";
            var processPaymentRequest = await HttpContext.Session.GetAsync<ProcessPaymentRequest>("OrderPaymentInfo");
            if (processPaymentRequest != null && processPaymentRequest.CustomValues.ContainsKey(MPay24PaymentDefaults.ShortNameLabel))
                selectedShortName = processPaymentRequest.CustomValues[MPay24PaymentDefaults.ShortNameLabel].ToString();

            var model = await paymentOptions.SelectAwait(async option => new PaymentInfoModel
            {
                Description = option.Description,
                Name = option.DisplayName,
                ShortName = option.ShortName,
                LogoUrl = await _pictureService.GetPictureUrlAsync(option.PictureId, 150),
                Selected = selectedShortName == option.ShortName
            }).ToListAsync();

            return View(model);
        }
    }
}
