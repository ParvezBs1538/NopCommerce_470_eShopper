using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.AffiliateStation.Services;
using NopStation.Plugin.Misc.Core.Components;
using Nop.Web.Areas.Admin.Models.Affiliates;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Components
{
    public class AffiliateStationAdminViewComponent : NopStationViewComponent
    {
        #region Fileds

        private readonly IAffiliateCustomerService _affiliateCustomerService;
        private readonly IAffiliateCustomerModelFactory _affiliateCustomerModelFactory;

        #endregion

        #region Ctor

        public AffiliateStationAdminViewComponent(IAffiliateCustomerService affiliateCustomerService,
            IAffiliateCustomerModelFactory affiliateCustomerModelFactory)
        {
            _affiliateCustomerService = affiliateCustomerService;
            _affiliateCustomerModelFactory = affiliateCustomerModelFactory;
        }

        #endregion

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (additionalData.GetType() != typeof(AffiliateModel))
                return Content("");
            
            var mm = additionalData as AffiliateModel;
            var affiliateCustomer = await _affiliateCustomerService.GetAffiliateCustomerByAffiliateIdAsync(mm.Id);
            if (affiliateCustomer == null)
                return Content("");

            var model = await _affiliateCustomerModelFactory.PrepareAffiliateCustomerModelAsync(null, affiliateCustomer);
            return View(model);
        }
    }
}
