using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Widgets.SmartProductRating.Services.Models;

namespace NopStation.Plugin.Widgets.SmartProductRating.Services
{
    public interface ICustomProductReviewService
    {
        Task<(IList<ProductReview>, List<ProductReviewRating>)> GetProductReviewsAsync(
               int storeId = 0, int productId = 0, int pageSize = int.MaxValue);
    }
}