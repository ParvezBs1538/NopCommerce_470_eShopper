using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Flipbooks.Domains;

namespace NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Factories
{
    public interface IFlipbookModelFactory
    {
        Task<FlipbookListModel> PrepareFlipbookListModelAsync(FlipbookSearchModel searchModel);

        Task<FlipbookModel> PrepareFlipbookModelAsync(FlipbookModel model, Flipbook flipbook, bool excludeProperties = false);

        Task<FlipbookSearchModel> PrepareFlipbookSearchModelAsync(FlipbookSearchModel searchModel);

        Task<FlipbookContentListModel> PrepareFlipbookContentListModelAsync(FlipbookContentSearchModel searchModel);

        Task<FlipbookContentModel> PrepareFlipbookContentModelAsync(FlipbookContentModel model, FlipbookContent flipbookContent, Flipbook flipbook);

        Task<FlipbookContentProductListModel> PrepareFlipbookContentProductListModelAsync(FlipbookContentProductSearchModel searchModel);

        Task<AddProductToFlipbookContentListModel> PrepareAddProductToFlipbookContentListModelAsync(AddProductToFlipbookContentSearchModel searchModel);

        Task<AddProductToFlipbookContentSearchModel> PrepareAddProductToFlipbookContentSearchModelAsync(AddProductToFlipbookContentSearchModel searchModel);
    }
}
