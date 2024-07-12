using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Widgets.AffiliateStation.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Services;
using NopStation.Plugin.Misc.Core.Components;
using Nop.Services.Affiliates;

namespace NopStation.Plugin.Widgets.AffiliateStation.Components
{
    public class AffiliateStationViewComponent : NopStationViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IAffiliateCustomerService _affiliateCustomerService;
        private readonly IAffiliateService _affiliateService;

        public AffiliateStationViewComponent(IWorkContext workContext,
            IAffiliateCustomerService affiliateCustomerService,
            IAffiliateService affiliateService)
        {
            _workContext = workContext;
            _affiliateCustomerService = affiliateCustomerService;
            _affiliateService = affiliateService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var affiliateCustomer = await _affiliateCustomerService.GetAffiliateCustomerByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);

            var model = new PublicModel();

            if (affiliateCustomer != null)
            {
                var affiliate = await _affiliateService.GetAffiliateByIdAsync(affiliateCustomer.AffiliateId);
                if (affiliate != null && !affiliate.Deleted)
                {
                    model.AlreadyApplied = true;
                    model.ApplyStatus = affiliateCustomer.ApplyStatus;
                    model.Active = affiliate.Active;
                }
            }

            return View(model);
        }
    }
}
