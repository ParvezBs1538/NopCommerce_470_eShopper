using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using NopStation.Plugin.Widgets.VendorShop.Models;
using NopStation.Plugin.Widgets.VendorShop.Models.ProfileTabs;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Factories
{
    public class ProductReviewModelFactory : IProductReviewModelFactory
    {
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IVendorProfileService _vendorProfileService;

        public ProductReviewModelFactory(IProductService productService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            IDateTimeHelper dateTimeHelper,
            IVendorProfileService vendorProfileService)
        {
            _dateTimeHelper = dateTimeHelper;
            _vendorProfileService = vendorProfileService;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
        }

        private async Task PrepareSortingOptionsAsync(VendorProfileModel model, VendorReviewsCommand command)
        {
            //get active sorting options
            var activeSortingOptionsIds = Enum.GetValues(typeof(VendorReviewsSortingEnum)).Cast<int>().ToList();
            var activeFilterOptionsIds = Enum.GetValues(typeof(VendorReviewsFilterEnum)).Cast<int>().ToList();

            //set the default option
            model.OrderBy = command.OrderBy;
            model.FilterBy = command.FilterBy;
            command.OrderBy = model.OrderBy ?? command.OrderBy;
            command.FilterBy = model.FilterBy ?? command.FilterBy;
            //prepare available model sorting options
            var prefix = "Admin.NopStation.VendorShop.ProfileTabs.Enum";
            foreach (var id in activeSortingOptionsIds)
            {
                model.AvailableSortOptions.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync(prefix + ((VendorReviewsSortingEnum)id).ToString()),
                    Value = id.ToString(),
                    Selected = id == command.OrderBy
                });
            }
            foreach (var id in activeFilterOptionsIds)
            {
                model.AvailableFilterOptions.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync(prefix + ((VendorReviewsFilterEnum)id).ToString()),
                    Value = id.ToString(),
                    Selected = id == command.FilterBy
                });
            }
        }

        private Task PreparePageSizeOptionsAsync(VendorProfileModel model, VendorReviewsCommand command,
           bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize)
        {
            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            if (allowCustomersToSelectPageSize && pageSizeOptions != null)
            {
                var pageSizes = pageSizeOptions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pageSizes.Any())
                {
                    // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                    if (command.PageSize <= 0 || !pageSizes.Contains(command.PageSize.ToString()))
                    {
                        if (int.TryParse(pageSizes.FirstOrDefault(), out var temp))
                        {
                            if (temp > 0)
                                command.PageSize = temp;
                        }
                    }

                    foreach (var pageSize in pageSizes)
                    {
                        if (!int.TryParse(pageSize, out var temp))
                            continue;

                        if (temp <= 0)
                            continue;
                    }
                }
            }
            else
            {
                //customer is not allowed to select a page size
                command.PageSize = fixedPageSize;
            }

            //ensure pge size is specified
            if (command.PageSize <= 0)
            {
                command.PageSize = fixedPageSize;
            }

            return Task.CompletedTask;
        }

        private async Task PrepareCatalogReviewsAsync(VendorProfileModel model, IPagedList<ProductReview> reviews)
        {
            if (!string.IsNullOrEmpty(model.WarningMessage))
                return;

            if (reviews.Count == 0)
                model.NoResultMessage = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.ProfileTabs.Reviews.NoResult");
            else
            {
                foreach (var review in reviews)
                {
                    var product = await _productService.GetProductByIdAsync(review.ProductId);
                    var reviewModel = new VendorProductReviewModel()
                    {
                        Title = review.Title,
                        ProductId = review.ProductId,
                        ProductName = product.Name,
                        ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                        Rating = review.Rating,
                        ReviewText = review.ReviewText,
                        WrittenOnStr = (await _dateTimeHelper.ConvertToUserTimeAsync(review.CreatedOnUtc, DateTimeKind.Utc)).ToString("g"),

                    };
                    model.Reviews.Add(reviewModel);
                }
                model.LoadPagedList(reviews);
            }
        }

        public virtual async Task<VendorProfileModel> PrepareReviewsModelAsync(int vendorId, VendorReviewsCommand command)
        {
            if (vendorId == 0)
                throw new ArgumentNullException(nameof(vendorId));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var model = new VendorProfileModel
            {
                UseAjaxLoading = true,
                VendorId = vendorId,
            };


            //sorting
            await PrepareSortingOptionsAsync(model, command);

            //page size
            await PreparePageSizeOptionsAsync(model, command, false, null, 10);

            //reviews
            var reviews = await _vendorProfileService.GetVendorProductReviewsAsync(model.VendorId, model.FilterBy ?? 0, model.OrderBy ?? 0, command.PageNumber, command.PageSize);

            await PrepareCatalogReviewsAsync(model, reviews);

            var allReviews = await (await _vendorProfileService.GetVendorProductReviewsAsync(model.VendorId, 0, 0, 1, int.MaxValue)).ToListAsync();
            var totalRating = allReviews.Sum(review => review.Rating) * 1.00;
            var maxPossibleRating = 5 * 1.00;
            var numberOfReviews = allReviews.Count;
            var overallPercentage = 0.0000;
            model.ReviewOverview = new VendorReviewOverviewModel();
            if (numberOfReviews > 0)
            {
                model.HasReview = true;
                overallPercentage = (totalRating / (maxPossibleRating * numberOfReviews)) * 100.00;
                model.ReviewOverview = new VendorReviewOverviewModel
                {
                    OverallRating = string.Format(await _localizationService.GetResourceAsync("NopStation.VendorShop.OverallPositive"), (int)overallPercentage),
                    BasedOnTotalReview = string.Format(await _localizationService.GetResourceAsync("NopStation.VendorShop.BasedOnTotalReview"), numberOfReviews)
                };
            }

            return model;
        }
    }
}
