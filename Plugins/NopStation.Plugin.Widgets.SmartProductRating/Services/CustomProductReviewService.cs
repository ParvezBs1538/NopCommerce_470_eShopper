using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using NopStation.Plugin.Widgets.SmartProductRating.Services.Models;

namespace NopStation.Plugin.Widgets.SmartProductRating.Services
{
    public class CustomProductReviewService : ICustomProductReviewService
    {
        private readonly CatalogSettings _catalogSettings;
        protected readonly IRepository<ProductReview> _productReviewRepository;

        public CustomProductReviewService(CatalogSettings catalogSettings,
            IRepository<ProductReview> productReviewRepository)
        {
            _catalogSettings = catalogSettings;
            _productReviewRepository = productReviewRepository;
        }

        public virtual async Task<(IList<ProductReview>, List<ProductReviewRating>)> GetProductReviewsAsync(
            int storeId = 0, int productId = 0, int pageSize = int.MaxValue)
        {
            var query = _productReviewRepository.Table
                .Where(pr => pr.ProductId == productId && pr.IsApproved == true);

            if (storeId > 0 && _catalogSettings.ShowProductReviewsPerStore)
                query = query.Where(pr => pr.StoreId == storeId);

            var ratings = query.GroupBy(x => x.Rating).Select(y =>
                new ProductReviewRating
                {
                    Rating = y.Key,
                    Count = y.Count()
                }).ToList();

            query = _catalogSettings.ProductReviewsSortByCreatedDateAscending
                ? query.OrderBy(pr => pr.CreatedOnUtc).ThenBy(pr => pr.Id)
                : query.OrderByDescending(pr => pr.CreatedOnUtc).ThenBy(pr => pr.Id);

            var productReviews = await query.Take(pageSize).ToListAsync();

            return (productReviews, ratings);
        }
    }
}
