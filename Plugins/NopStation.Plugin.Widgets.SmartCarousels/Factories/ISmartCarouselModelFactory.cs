using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;
using NopStation.Plugin.Widgets.SmartCarousels.Models;

namespace NopStation.Plugin.Widgets.SmartCarousels.Factories;

public partial interface ISmartCarouselModelFactory
{
    Task<IList<CarouselOverviewModel>> PrepareCarouselAjaxListModelAsync(string widgetZone);

    Task<IList<CarouselModel>> PrepareCarouselListModelAsync(string widgetZone);

    Task<CarouselModel> PrepareCarouselModelAsync(SmartCarousel carousel);
}
