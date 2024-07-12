using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;
using NopStation.Plugin.Misc.AmazonPersonalize.Models;
using NopStation.Plugin.Misc.AmazonPersonalize.Services;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Factories
{
    public class PersonalizedRecommendationsModelFactory : IPersonalizedRecommendationsModelFactory
    {
        #region Fields

        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ILocalizationService _localizationService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStaticCacheManager _cacheKeyService;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly IWorkContext _workContext;
        private readonly IPersonalizedRecommendationsService _personalizedRecommendationsService;

        #endregion Fields

        #region Ctor

        public PersonalizedRecommendationsModelFactory(IProductModelFactory productModelFactory,
            IProductService productService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            ILocalizationService localizationService,
            IStaticCacheManager staticCacheManager,
            IStaticCacheManager cacheKeyService,
            IStoreContext storeContext,
            IPictureService pictureService,
            IWorkContext workContext,
            IPersonalizedRecommendationsService personalizedRecommendationsService)
        {
            _productModelFactory = productModelFactory;
            _productService = productService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _localizationService = localizationService;
            _staticCacheManager = staticCacheManager;
            _cacheKeyService = cacheKeyService;
            _storeContext = storeContext;
            _pictureService = pictureService;
            _workContext = workContext;
            _personalizedRecommendationsService = personalizedRecommendationsService;
        }

        #endregion Ctor

        #region Utilities
        protected async Task<IList<ProductOverviewModel>> PrepareProductsModel(string[] itemIds)
        {
            var productIds = Array.ConvertAll(itemIds, int.Parse);
            var sp = (await _productService.GetProductsByIdsAsync(productIds)).Where(p => p.Published).ToList();
            sp = await sp.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && (await _storeMappingService.AuthorizeAsync(p))).ToListAsync();
            
            var products = sp.Where(p => _productService.ProductIsAvailable(p)).ToList();

            return (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
        }

        #endregion Utilities

        #region Methods

        public async Task<RecommendationModel> PrepareRecommendationModelAsync(Recommendation recommendation, Customer customer, AdditionalData additionalData)
        {
            if (recommendation == null)
                throw new ArgumentNullException(nameof(recommendation));

            var model = new RecommendationModel
            {
                Id = recommendation.Id,
                Title = recommendation.Title,
            };
            var recommendedItemIds = (await _personalizedRecommendationsService.GetRecommendationResults(customer.Id.ToString(), recommendation, additionalData.ProductId.ToString())).Select(x => x.ItemId).ToArray();
            model.Products = await PrepareProductsModel(recommendedItemIds);

            return model;
        }

        public async Task<RecommendedForYouModel> PrepareRecommendedForYouModelAsync(string[] itemIds)
        {
            var model = new RecommendedForYouModel();
            var productIds = Array.ConvertAll(itemIds, int.Parse);

            var sp = (await _productService.GetProductsByIdsAsync(productIds)).Where(p => p.Published).ToList();
            sp = await sp.WhereAwait(async p => await _aclService.AuthorizeAsync(p) && (await _storeMappingService.AuthorizeAsync(p))).ToListAsync();
            var products = sp.Where(p => _productService.ProductIsAvailable(p)).ToList();
            model.RecommendedProducts = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();

            return model;
        }

        public Task<RecommendationListModel> PrepareRecommendationListModelAsync(IList<Recommendation> recommendations, int productId = 0)
        {
            if (recommendations == null)
                throw new ArgumentNullException(nameof(recommendations));
            
            var model = new RecommendationListModel();
            foreach (var recommendation in recommendations)
            {
                model.Recommendations.Add(new RecommendationListModel.RecommenderOverviewModel()
                {
                    Title = recommendation.Title,
                    Id = recommendation.Id,
                    ProductId = productId,
                });
            }

            return Task.FromResult(model);
        }

        #endregion Methods
    }
}