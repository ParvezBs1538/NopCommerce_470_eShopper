using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Factories
{
    public interface IProductQAModelFactory
    {
        Task<ProductQAListModel> PrepareProdouctQAListModelAsync(ProductQASearchModel searchModel);
        Task<ProductQASearchModel> PrepareProductQASearchModelAsync(ProductQASearchModel searchModel);
        Task<ProductQAModel> PrepareModelForEditPageAsync(int productQAId);
        Task<bool> IsAccessToEditAsync();
    }
}
