using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Widgets.VendorShop.Factories;
using NopStation.Plugin.Widgets.VendorShop.Models.ProfileTabs;

namespace NopStation.Plugin.Widgets.VendorShop.Components
{
    public class VendorShopProfileViewComponent : NopViewComponent
    {
        private readonly IProductReviewModelFactory _productReviewModelFactory;
        public VendorShopProfileViewComponent(
            IProductReviewModelFactory productReviewModelFactory)
        {
            _productReviewModelFactory = productReviewModelFactory;
        }
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (additionalData is VendorModel vendorModel)
            {
                var catalogReviewsCommand = new VendorReviewsCommand();
                var vendorProfileModel = await _productReviewModelFactory.PrepareReviewsModelAsync(vendorModel.Id, catalogReviewsCommand);
                return View(vendorProfileModel);
            }
            else
            {
                return Content("Invalid data provided");
            }
        }
    }
}
