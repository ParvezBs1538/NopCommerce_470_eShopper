using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.AmazonPersonalize.Factories;
using NopStation.Plugin.Misc.AmazonPersonalize.Models;
using NopStation.Plugin.Misc.AmazonPersonalize.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Controllers
{
    public class RecommendationController : NopStationPublicController
    {
        #region Fields

        private readonly IRecommendationService _recommenderService;
        private readonly IPersonalizedRecommendationsModelFactory _personalizedRecommendatioinsModelFactory;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;

        #endregion Fields

        #region Ctor

        public RecommendationController(IRecommendationService recommenderService,
            IPersonalizedRecommendationsModelFactory personalizedRecommendatioinsModelFactory,
            IWorkContext workContext,
            ICustomerService customerService,
            ILocalizationService localizationService)
        {
            _recommenderService = recommenderService;
            _personalizedRecommendatioinsModelFactory = personalizedRecommendatioinsModelFactory;
            _workContext = workContext;
            _customerService = customerService;
            _localizationService = localizationService;
        }

        #endregion Ctor

        #region Methods

        [HttpPost]
        public async Task<IActionResult> Details(int recommendationId, int productId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var recommendation = await _recommenderService.GetRecommendationByIdAsync(recommendationId);
            if (recommendation == null || !recommendation.Active)
                return Json(new { result = false });
            
            if (!recommendation.AllowForGuestCustomer && !(await _customerService.IsRegisteredAsync(customer)))
                return Json(new { result = false, message = (await _localizationService.GetResourceAsync("NopStation.AmazonPersonalize.Customer.Message")) });

            var additionalData = new AdditionalData();
            if (productId > 0)
                additionalData.ProductId = productId;

            var model = await _personalizedRecommendatioinsModelFactory.PrepareRecommendationModelAsync(recommendation, customer, additionalData);

            if (model.Products.Count == 0)
                return Json(new { result = false, message = await _localizationService.GetResourceAsync("NopStation.AmazonPersonalize.Recommender.NoProducts") });

            var html = await RenderPartialViewToStringAsync("Product", model);

            return Json(new { result = true, html = html, recommendationId = recommendationId });
        }

        #endregion Methods
    }
}