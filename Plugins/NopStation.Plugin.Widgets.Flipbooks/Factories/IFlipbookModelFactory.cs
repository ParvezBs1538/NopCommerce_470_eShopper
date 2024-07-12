using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Flipbooks.Models;

namespace NopStation.Plugin.Widgets.Flipbooks.Factories
{
    public interface IFlipbookModelFactory
    {
        Task<FlipbookContentModel> PrepareFlipbookContentProductsAsync(int flipbookContentId, int pageIndex = 0);

        Task<FlipbookDetailsModel> PrepareFlipbookDetailsModelAsync(Domains.Flipbook flipbook);
    }
}
