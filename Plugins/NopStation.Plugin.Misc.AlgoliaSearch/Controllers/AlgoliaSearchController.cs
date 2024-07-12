using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using NopStation.Plugin.Misc.AlgoliaSearch.Factories;
using NopStation.Plugin.Misc.AlgoliaSearch.Models;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Controllers
{
    public class AlgoliaSearchController : NopStationPublicController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly AlgoliaSearchSettings _algoliaSearchSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IAlgoliaCatalogModelFactory _algoliaCatalogModelFactory;

        #endregion

        #region Ctor

        public AlgoliaSearchController(IWorkContext workContext,
            AlgoliaSearchSettings algoliaSearchSettings,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IAlgoliaCatalogModelFactory algoliaCatalogModelFactory)
        {
            _workContext = workContext;
            _algoliaSearchSettings = algoliaSearchSettings;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _algoliaCatalogModelFactory = algoliaCatalogModelFactory;
        }

        #endregion

        public async Task<IActionResult> Search(SearchModel model, AlgoliaPagingFilteringModel command)
        {
            model ??= new SearchModel();

            if (string.IsNullOrWhiteSpace(model.q) || model.q.Length < _algoliaSearchSettings.SearchTermMinimumLength)
                return RedirectToRoute("HomePage");

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(true),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            model = await _algoliaCatalogModelFactory.PrepareSearchModelAsync(model, command);

            return View(model);
        }
    }
}
