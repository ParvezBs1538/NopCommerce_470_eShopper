using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Payments.BlueSnapHosted.Models;
using NopStation.Plugin.Payments.BlueSnapHosted.Services;
using NopStation.Plugin.Misc.Core.Components;
using Nop.Services.Customers;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Components
{
    public class BlueSnapHostedViewComponent : NopStationViewComponent
    {
        private readonly BlueSnapSettings _blueSnapSettings;
        private readonly IBlueSnapServices _blueSnapServices;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;

        public BlueSnapHostedViewComponent(BlueSnapSettings blueSnapSettings,
            IBlueSnapServices blueSnapServices,
            IWorkContext workContext,
            ICustomerService customerService)
        {
            _blueSnapSettings = blueSnapSettings;
            _blueSnapServices = blueSnapServices;
            _workContext = workContext;
            _customerService = customerService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                IsSandBox = _blueSnapSettings.IsSandBox,
                Token = await _blueSnapServices.GetTokenAsync(),
                FullName = await _customerService.GetCustomerFullNameAsync(await _workContext.GetCurrentCustomerAsync())
            };

            return View("~/Plugins/NopStation.Plugin.Payments.BlueSnapHosted/Views/PaymentInfo.cshtml", model);
        }
    }
}
