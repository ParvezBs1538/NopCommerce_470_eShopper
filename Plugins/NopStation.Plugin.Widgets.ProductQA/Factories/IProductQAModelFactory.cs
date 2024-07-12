using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Models;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Factories
{
    public interface IProductQAModelFactory
    {
        Task<ProductQuestionAnswerModel> PrepareGetProductQuestionAnswerModelAsync(ProductQuestionAnswerModel model);
        Task<ProductQAPublicInfoModel> PrepareProductQAPublicInfoModelAsync(int productId);
    }
}
