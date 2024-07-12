using System.Threading.Tasks;
using NopStation.Plugin.Misc.WebApi.Models.Sliders;

namespace NopStation.Plugin.Misc.WebApi.Factories
{
    public interface ISliderModelFactory
    {
        Task<HomePageSliderModel> PrepareHomePageSliderModelAsync();
    }
}