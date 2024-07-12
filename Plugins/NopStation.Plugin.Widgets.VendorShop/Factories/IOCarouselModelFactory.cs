using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Models.OCarouselVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Factories
{
    public partial interface IOCarouselModelFactory
    {
        Task<OCarouselListModel> PrepareCarouselListModelAsync(IList<OCarousel> carousels);

        Task<OCarouselModel> PrepareCarouselModelAsync(OCarousel carousel);
    }
}
