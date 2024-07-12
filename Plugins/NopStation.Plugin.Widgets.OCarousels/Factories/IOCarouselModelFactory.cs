using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.OCarousels.Domains;
using NopStation.Plugin.Widgets.OCarousels.Models;

namespace NopStation.Plugin.Widgets.OCarousels.Factories
{
    public partial interface IOCarouselModelFactory
    {
        Task<OCarouselListModel> PrepareCarouselListModelAsync(IList<OCarousel> carousels);

        Task<OCarouselModel> PrepareCarouselModelAsync(OCarousel carousel);
    }
}
