using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;
using NopStation.Plugin.Widgets.SmartDealCarousels.Models;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Factories;

public partial interface ISmartDealCarouselModelFactory
{
    Task<IList<CarouselOverviewModel>> PrepareCarouselAjaxListModelAsync(string widgetZone);

    Task<IList<CarouselModel>> PrepareCarouselListModelAsync(string widgetZone);

    Task<CarouselModel> PrepareCarouselModelAsync(SmartDealCarousel carousel);
}
