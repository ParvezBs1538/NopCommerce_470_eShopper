using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.VendorShop.Factories;
using NopStation.Plugin.Widgets.VendorShop.Models.ProfileTabs;

namespace NopStation.Plugin.Widgets.VendorShop.Controllers
{
    public class VendorProductReviewsController : NopStationPublicController
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IProductReviewModelFactory _productReviewModelFactory;
        public VendorProductReviewsController(
            ICustomerService customerService,
            IWorkContext workContext,
            IProductReviewModelFactory productReviewModelFactory)
        {
            _customerService = customerService;
            _workContext = workContext;
            _productReviewModelFactory = productReviewModelFactory;
        }
        public async Task<IActionResult> ProductReviews(int vendorId, VendorReviewsCommand command)
        {
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var model = await _productReviewModelFactory.PrepareReviewsModelAsync(vendorId, command);

            return PartialView("_ReviewsInLines", model);
        }
    }
}
