using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.DMS.Components
{
    public class DMSAccountNavigationViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public DMSAccountNavigationViewComponent(IWorkContext workContext,
            ICustomerService customerService)
        {
            _workContext = workContext;
            _customerService = customerService;
        }

        #endregion

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!await _customerService.IsInCustomerRoleAsync(await _workContext.GetCurrentCustomerAsync(), DMSDefaults.ShipperCustomerRoleName))
                return Content("");

            return View();
        }
    }
}
