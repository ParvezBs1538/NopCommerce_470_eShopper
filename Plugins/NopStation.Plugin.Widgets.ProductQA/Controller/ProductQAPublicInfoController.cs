using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Domains;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Factories;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Infrastructure.Cache;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Models;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Services;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Controller
{
    public class ProductQAPublicInfoController : NopStationPublicController
    {
        #region Fields
        private readonly IProductQAModelFactory _productQAModelFactory;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductQAService _productQAService;
        private readonly IStoreContext _storeContext;
        #endregion

        #region Ctor
        public ProductQAPublicInfoController(IProductQAModelFactory productQAModelFactory,
            IStaticCacheManager cacheManager,
            IWorkContext workContext,
            IProductService productService,
            ILocalizationService localizationService,
            IProductQAService productQAService,
            IStoreContext storeContext)
        {
            _productQAModelFactory = productQAModelFactory;
            _cacheManager = cacheManager;
            _workContext = workContext;
            _productService = productService;
            _localizationService = localizationService;
            _productQAService = productQAService;
            _storeContext = storeContext;
        }
        #endregion

        #region Utility

        private async Task<string> GetQuestionAnswerHtml(ProductQuestionAnswerModel model)
        {
            var prepareModel = await _productQAModelFactory.PrepareGetProductQuestionAnswerModelAsync(model);

            var html = await RenderPartialViewToStringAsync("_ProductQuestionAnswer", prepareModel);
            return html;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> SaveQuestion(ProductQuestionSaveModel model)
        {
            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product == null || product.Deleted || !product.Published)
                return Json(new { html = "" });

            var reloadHtml = false;
            var html = "";
            var question = new ProductQnA()
            {
                IsApproved = false,
                CustomerId = _workContext == null ? 0 : (await _workContext.GetCurrentCustomerAsync()).Id,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                ProductId = model.ProductId,
                QuestionText = model.QuestionText,
                IsAnonymous = model.IsAnonymous,
                StoreId = _storeContext == null ? 0 : (await _storeContext.GetCurrentStoreAsync()).Id,
                Deleted = false
            };
            if (_workContext != null)
            {
                reloadHtml = true;
            }

            await _productQAService.InsertProductQnAAsync(question);

            if (reloadHtml)
            {
                var comModel = new ProductQuestionAnswerModel() { CurrentPageNumber = 1, Id = model.ProductId };
                html = await GetQuestionAnswerHtml(comModel);
            }

            return Json(
                new
                {
                    success = true,
                    message = await _localizationService.GetResourceAsync("NopStation.ProductQuestionAnswer.ProductQnA.Saved"),
                    reloadHtml = reloadHtml,
                    html = html
                });
        }

        public async Task<IActionResult> GetQuestionListByProductIdForPublicInfo(ProductQuestionAnswerModel model)
        {
            var product = await _productService.GetProductByIdAsync(model.Id);
            if (product == null || product.Deleted || !product.Published)
                return Json(new { html = "" });

            string html = await GetQuestionAnswerHtml(model);
            return Json(new { html });
        }
        #endregion
    }
}
