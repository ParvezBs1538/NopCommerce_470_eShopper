using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.OrderRatings.Factories;
using NopStation.Plugin.Widgets.OrderRatings.Services;
using Nop.Services.Customers;

namespace NopStation.Plugin.Widgets.OrderRatings.Components
{
    public class OrderRatingHomepageViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IOrderRatingModelFactory _orderRatingModelFactory;
        private readonly IOrderRatingService _orderRatingService;
        private readonly ICustomerService _customerService;
        private readonly OrderRatingSettings _orderRatingSettings;

        #endregion

        #region Ctor

        public OrderRatingHomepageViewComponent(IWorkContext workContext,
            IOrderRatingModelFactory orderRatingModelFactory, 
            IOrderRatingService orderRatingService,
            ICustomerService customerService,
            OrderRatingSettings orderRatingSettings)
        {
            _workContext = workContext;
            _orderRatingModelFactory = orderRatingModelFactory;
            _orderRatingService = orderRatingService;
            _customerService = customerService;
            _orderRatingSettings = orderRatingSettings;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_orderRatingSettings.ShowOrderRatedDateInDetailsPage)
                return Content("");

            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Content("");

            return View();
        }

        #endregion
    }
}
