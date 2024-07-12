using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.SmartProductRating.Models;
using NopStation.Plugin.Widgets.SmartProductRating.Services;

namespace NopStation.Plugin.Widgets.SmartProductRating.Components
{
    public class SmartProductRatingViewComponent : NopStationViewComponent
    {
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreContext _storeContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly SmartProductRatingSettings _smartProductRatingSettings;
        private readonly IWorkContext _workContext;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerService _customerService;
        private readonly IReviewTypeService _reviewTypeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomProductReviewService _customProductReviewService;

        public SmartProductRatingViewComponent(IProductService productService,
            ILocalizationService localizationService,
             IUrlRecordService urlRecordService,
             IStoreContext storeContext,
             CatalogSettings catalogSettings,
             SmartProductRatingSettings smartProductRatingSettings,
             IWorkContext workContext,
             CustomerSettings customerSettings,
             ICustomerService customerService,
             IReviewTypeService reviewTypeService,
             IDateTimeHelper dateTimeHelper,
             ICustomProductReviewService customProductReviewService)
        {
            _productService = productService;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _storeContext = storeContext;
            _catalogSettings = catalogSettings;
            _smartProductRatingSettings = smartProductRatingSettings;
            _workContext = workContext;
            _customerSettings = customerSettings;
            _customerService = customerService;
            _reviewTypeService = reviewTypeService;
            _dateTimeHelper = dateTimeHelper;
            _customProductReviewService = customProductReviewService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_smartProductRatingSettings.EnablePlugin)
                return Content("");

            var productDetailsModel = additionalData as ProductDetailsModel;
            int productId;
            if (productDetailsModel != null)
                productId = productDetailsModel.Id;
            else
                int.TryParse(additionalData.ToString(), out productId);

            if (productId < 1)
                return Content("");
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return Content("");

            var model = await PreparePublicModelAsync(product);
            return View("~/Plugins/NopStation.Plugin.Widgets.SmartProductRating/Views/PublicInfo.cshtml", model);
        }

        private async Task<PublicModel> PreparePublicModelAsync(Product product)
        {
            var storeId = _catalogSettings.ShowProductReviewsPerStore ? _storeContext.GetCurrentStore().Id : 0;

            var result = await _customProductReviewService.GetProductReviewsAsync(storeId, product.Id, _smartProductRatingSettings.NumberOfReviewsInProductDetailsPage);

            var totalReviews = product.ApprovedTotalReviews;
            var ratingSum = product.ApprovedRatingSum;

            var ratings = result.Item2;
            var oneStarRvs = ratings.FirstOrDefault(x => x.Rating == 1)?.Count ?? 0;
            var twoStarRvs = ratings.FirstOrDefault(x => x.Rating == 2)?.Count ?? 0;
            var threeStarRvs = ratings.FirstOrDefault(x => x.Rating == 3)?.Count ?? 0;
            var fourStarRvs = ratings.FirstOrDefault(x => x.Rating == 4)?.Count ?? 0;
            var fiveStarRvs = ratings.FirstOrDefault(x => x.Rating == 5)?.Count ?? 0;

            var model = new PublicModel()
            {
                ProductId = product.Id,
                ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                TotalReviews = totalReviews,
                RatingSum = ratingSum,
                ReviewPercentage = totalReviews > 0 ? ratingSum / (decimal)totalReviews * 20 : 0,
                OneStarReviews = oneStarRvs,
                TwoStarReviews = twoStarRvs,
                ThreeStarReviews = threeStarRvs,
                FourStarReviews = fourStarRvs,
                FiveStarReviews = fiveStarRvs,
                OneStarPercentage = totalReviews > 0 ? oneStarRvs / (decimal)totalReviews * 100 : 0,
                TwoStarPercentage = totalReviews > 0 ? twoStarRvs / (decimal)totalReviews * 100 : 0,
                ThreeStarPercentage = totalReviews > 0 ? threeStarRvs / (decimal)totalReviews * 100 : 0,
                FourStarPercentage = totalReviews > 0 ? fourStarRvs / (decimal)totalReviews * 100 : 0,
                FiveStarPercentage = totalReviews > 0 ? fiveStarRvs / (decimal)totalReviews * 100 : 0
            };

            foreach (var pr in result.Item1)
            {
                var customer = await _customerService.GetCustomerByIdAsync(pr.CustomerId);
                var productReviewModel = new ProductReviewModel
                {
                    Id = pr.Id,
                    CustomerId = pr.CustomerId,
                    CustomerName = await _customerService.FormatUsernameAsync(customer),
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !await _customerService.IsGuestAsync(customer),
                    Title = pr.Title,
                    ReviewText = pr.ReviewText,
                    ReplyText = pr.ReplyText,
                    Rating = pr.Rating,
                    Helpfulness = new ProductReviewHelpfulnessModel
                    {
                        ProductReviewId = pr.Id,
                        HelpfulYesTotal = pr.HelpfulYesTotal,
                        HelpfulNoTotal = pr.HelpfulNoTotal,
                    },
                    WrittenOnStr = (await _dateTimeHelper.ConvertToUserTimeAsync(pr.CreatedOnUtc, DateTimeKind.Utc)).ToString("g"),
                };

                foreach (var q in await _reviewTypeService.GetProductReviewReviewTypeMappingsByProductReviewIdAsync(pr.Id))
                {
                    var reviewType = await _reviewTypeService.GetReviewTypeByIdAsync(q.ReviewTypeId);
                    productReviewModel.AdditionalProductReviewList.Add(new ProductReviewReviewTypeMappingModel
                    {
                        ReviewTypeId = q.ReviewTypeId,
                        ProductReviewId = pr.Id,
                        Rating = q.Rating,
                        Name = await _localizationService.GetLocalizedAsync(reviewType, x => x.Name),
                        VisibleToAllCustomers = reviewType.VisibleToAllCustomers || (await _workContext.GetCurrentCustomerAsync()).Id == pr.CustomerId,
                    });
                }

                model.ProductReviews.Add(productReviewModel);
            }

            return model;
        }
    }
}
