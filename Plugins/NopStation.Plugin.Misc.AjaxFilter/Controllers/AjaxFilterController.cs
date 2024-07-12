using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Factories;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Services;
using NopStation.Plugin.Misc.AjaxFilter.Domains;
using NopStation.Plugin.Misc.AjaxFilter.Extensions;
using NopStation.Plugin.Misc.AjaxFilter.Factories;
using NopStation.Plugin.Misc.AjaxFilter.Models;
using NopStation.Plugin.Misc.AjaxFilter.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.Misc.AjaxFilter.Controllers
{
    public class AjaxFilterController : NopStationPublicController
    {
        #region Fields

        private readonly IAjaxFilterService _ajaxFilterService;
        private readonly ISettingService _settingService;
        private readonly IAjaxSearchModelFactory _ajaxSearchModelFactory;
        private readonly IAjaxFilterParentCategoryService _ajaxFilterParentCategoryService;
        private readonly IAjaxFilterModelFactory _ajaxFilterModelFactory;
        private readonly IAjaxFilterSpecificationAttributeService _ajaxFilterSpecificationAttributeService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public AjaxFilterController(
            IAjaxFilterService ajaxFilterService,
            ISettingService settingService,
            IAjaxSearchModelFactory ajaxSearchModelFactory,
            ICatalogModelFactory catalogModelFactory,
            ICategoryService categoryService,
            ILocalizationService localizationService,
            IProductModelFactory productModelFactory,
            IStoreContext storeContext,
            IAjaxFilterParentCategoryService ajaxFilterParentCategoryService,
            IAjaxFilterModelFactory ajaxFilterModelFactory,
            IAjaxFilterSpecificationAttributeService ajaxFilterSpecificationAttributeService)
        {
            _ajaxFilterService = ajaxFilterService;
            _settingService = settingService;
            _ajaxSearchModelFactory = ajaxSearchModelFactory;
            _catalogModelFactory = catalogModelFactory;
            _categoryService = categoryService;
            _localizationService = localizationService;
            _productModelFactory = productModelFactory;
            _storeContext = storeContext;
            _ajaxFilterParentCategoryService = ajaxFilterParentCategoryService;
            _ajaxFilterModelFactory = ajaxFilterModelFactory;
            _ajaxFilterSpecificationAttributeService = ajaxFilterSpecificationAttributeService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> CatalogProducts(int categoryId, CatalogProductsCommand command)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);

            var model = await _catalogModelFactory.PrepareCategoryModelAsync(category, command);

            return View(model);
        }

        public virtual async Task<IActionResult> LoadFilters()
        {
            var complexData = HttpContext.Session.GetComplexData<RouteValueDictionary>("_routeValue");
            var requestParams = HttpContext.Session.GetComplexData<List<RequestParams>>("_requestParams");
            var requestPath = HttpContext.Session.GetComplexData<string>("_requestPath");
            var categoryId = 0;
            var manufacturerId = 0;

            if (Url.ActionContext != null)
            {
                var categoryObj = complexData["categoryId"];
                if (categoryObj != null)
                {
                    int.TryParse(categoryObj.ToString(), out categoryId);
                }
                var manufacturerObj = complexData["manufacturerId"];
                if (manufacturerObj != null)
                {
                    int.TryParse(manufacturerObj.ToString(), out manufacturerId);
                }
            }

            var publicInfoModel = await _ajaxSearchModelFactory.PreparePublicInfoModelAsync(categoryId, manufacturerId, complexData, requestParams);
            publicInfoModel.CategoryId = categoryId;
            publicInfoModel.RequestPath = requestPath;
            publicInfoModel.ManufacturerId = manufacturerId;
            publicInfoModel.EnableFilter = false;
            publicInfoModel.EnablePriceRangeFilter = false;
            publicInfoModel.EnableManufacturersFilter = false;
            publicInfoModel.EnableProductTagsFilter = false;
            publicInfoModel.EnableProductTagsFilter = false;
            publicInfoModel.EnableSpecificationsFilter = false;
            publicInfoModel.EnableVendorsFilter = false;
            publicInfoModel.EnableProductAttributeFilter = false;
            publicInfoModel.EnableMiscFilter = false;

            var selectedPriceParam = requestParams.Where(m => m.Key.Equals("price", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.Value;

            if (selectedPriceParam != null)
            {
                var priceRange = await _ajaxSearchModelFactory.GetConvertedPriceRangeAsync(selectedPriceParam);
                publicInfoModel.FilterPriceModel.CurrentMinPrice = priceRange.MinPrice;
                publicInfoModel.FilterPriceModel.CurrentMaxPrice = priceRange.MaxPrice;
            }

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "nop-ajax-filters",
                    html = await RenderPartialViewToStringAsync("LoadFilters", publicInfoModel),
                    reload = true
                }
            });
        }

        public virtual async Task<IActionResult> ReloadFilters(PublicInfoModel model, string typ)
        {
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var settings = await _settingService.LoadSettingAsync<AjaxFilterSettings>(storeId);

            model.ViewMoreSpecId = 0;
            typ = typ is null ? "" : typ;
            var searchModel = await _ajaxSearchModelFactory.PrepareSearchModelAsync(model, typ);
            var avaliableSpecificationAttributes = new List<AjaxFilterSpecificationAttribute>();

            for (int i = 0; i < searchModel.CategoryIds.Count; i++)
            {
                avaliableSpecificationAttributes.AddRange(await _ajaxFilterService
                    .GetAllAjaxFilterSpecificationAttributeIdsFromCategoryId(searchModel.CategoryIds[i]));
            }

            var specificationIds = avaliableSpecificationAttributes.Select(x => x.SpecificationId).Distinct().ToList();
            var distinctAvailableSpecificationAttributes = new List<AjaxFilterSpecificationAttribute>();
            specificationIds.ForEach(x =>
            {
                distinctAvailableSpecificationAttributes.Add(avaliableSpecificationAttributes.First(y => y.SpecificationId == x));
            });

            searchModel.PageIndex = model.PageNumber;
            searchModel.AvaliableSpecificationAttributes = distinctAvailableSpecificationAttributes;
            searchModel.ViewMoreSpecificationId = model.ViewMoreSpecId;
            searchModel.SelectedSpecificationAttributeOptions = model.SelectedSpecificationAttributeOptions;
            searchModel.SelectedSpecificationAttributes = model.SelectedSpecificationAttributes;
            searchModel.SelectedAttributeOptions = model.ProductAttributeOptionIds;
            model.SpecificationModel.ViewMoreSpecificiationId = model.ViewMoreSpecId;

            //get filters
            var searchFilterResult = await _ajaxFilterService.SearchProducts(searchModel, typ, showproducts: true);
            model = await _ajaxSearchModelFactory.CompletePublicInfoModelAsync(model, searchFilterResult);

            while (searchFilterResult.Products.TotalCount > 0 && searchFilterResult.Products.Count == 0 && searchModel.PageIndex > 0)
            {
                searchModel.PageIndex--;
                searchFilterResult = await _ajaxFilterService.SearchProducts(searchModel, typ, showproducts: true);
                model = await _ajaxSearchModelFactory.CompletePublicInfoModelAsync(model, searchFilterResult);
            }

            model.AjaxProductsModel = new ProductsModel();
            if (model.ViewMoreOrShowLessClicked == null)
            {
                model.AjaxProductsModel.LoadPagedList(searchFilterResult.Products);
                model.AjaxProductsModel.ViewMode = model.ViewMode;
                model.AjaxProductsModel.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(searchFilterResult.Products)).ToList();
            }

            model.AjaxProductsModel.Count = searchFilterResult.Products.TotalCount;
            if (model.AjaxProductsModel.Count == 0)
            {
                model.AjaxProductsModel.NoResultMessage = await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.AjaxFilter.Filters.NoResults");
            }

            model.QuickViewButtonContainerName = await _settingService.GetSettingByKeyAsync<string>("quickviewsettings.buttoncontainername");
            
            var category = await _ajaxFilterParentCategoryService.GetParentCategoryByCategoryIdAsync(model.CategoryId);
            model.AjaxFilterParentCategoryModel = await _ajaxFilterModelFactory.PrepareParentCategoryModelAsync(category);
            model.SpecificationModel.EnableSearchForManufacturer = model.AjaxFilterParentCategoryModel.EnableSearchForManufacturers;
            model.SpecificationModel.EnableSearchForSpecificationAttribute = model.AjaxFilterParentCategoryModel.EnableSearchForSpecifications;
            model.SpecificationModel.ShowProductCountInFilter = settings.EnableProductCount;

            var filterSection = await RenderPartialViewToStringAsync("LoadFilters", model);
            var productsSection = await RenderPartialViewToStringAsync("ReloadFilters", model.AjaxProductsModel);

            return Json(new
            {
                Success = true,
                Type = typ,
                FilterSection = filterSection,
                Url = searchModel.Url,
                Products = productsSection
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> GetAllSpecificationOptionsAsync(PublicInfoModel model, string typ)
        {
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var settings = await _settingService.LoadSettingAsync<AjaxFilterSettings>(storeId);

            model.FilteredPrice = null;
            typ = typ is null ? "" : typ;
            var searchModel = await _ajaxSearchModelFactory.PrepareSearchModelAsync(model, typ);

            searchModel.PageIndex = model.PageIndex;
            searchModel.ViewMoreSpecificationId = model.ViewMoreSpecId;
            var ajaxFilterSpecificationAttribute = await _ajaxFilterSpecificationAttributeService
                .GetAjaxFilterSpecificationAttributeBySpecificationAttributeId(model.ViewMoreSpecId);

            searchModel.AvaliableSpecificationAttributes = new List<AjaxFilterSpecificationAttribute>
            {
                ajaxFilterSpecificationAttribute
            };
            var searchFilterResult = await _ajaxFilterService.SearchProducts(searchModel, typ, showproducts: false);
            model = await _ajaxSearchModelFactory.CompletePublicInfoModelAsync(model, searchFilterResult);
            model.SpecificationModel.ShowProductCountInFilter = settings.EnableProductCount;
            return Json(model);
        }

        #endregion
    }
}
