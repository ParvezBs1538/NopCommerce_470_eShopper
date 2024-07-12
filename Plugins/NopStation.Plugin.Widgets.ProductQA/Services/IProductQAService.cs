using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Domains;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Services
{
    public interface IProductQAService
    {
        Task InsertProductQnAAsync(ProductQnA productQA);
        Task UpdateProductQnAAsync(ProductQnA productQA);
        Task DeleteProductQnAAsync(ProductQnA productQA);
        Task<ProductQnA> GetProductQnAByIdAsync(int productQAId);
        Task<IPagedList<ProductQnA>> GetAllProductQnAsAsync(int storeId = 0, int productId = 0,
            bool? approved = null, bool? hasAnswer = null, DateTime? createdFrom = null, DateTime? createdTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IList<ProductQnA>> GetProductQnAsByProductIdAsync(int productId, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
