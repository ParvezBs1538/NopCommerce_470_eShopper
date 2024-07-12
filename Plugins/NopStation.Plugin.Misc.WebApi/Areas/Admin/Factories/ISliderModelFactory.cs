using System.Threading.Tasks;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Factories
{
    public interface ISliderModelFactory
    {
        Task<SliderSearchModel> PrepareSliderSearchModelAsync(SliderSearchModel searchModel);

        Task<SliderListModel> PrepareSliderListModelAsync(SliderSearchModel searchModel);

        Task<SliderModel> PrepareSliderModelAsync(SliderModel model, ApiSlider slider, bool excludeProperties = false);
    }
}