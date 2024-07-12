using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Helpers;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Factories
{
    public partial class OCarouselModelFactory : IOCarouselModelFactory
    {
        #region Fields
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IOCarouselService _carouselService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public OCarouselModelFactory(IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            ILocalizedModelFactory localizedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            IOCarouselService carouselService,
            IProductService productService,
            IPictureService pictureService,
            IDateTimeHelper dateTimeHelper)
        {
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _localizedModelFactory = localizedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _carouselService = carouselService;
            _productService = productService;
            _pictureService = pictureService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Utilities

        protected async Task PrepareCustomWidgetZonesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //prepare available activity log types
            var availableWidgetZones = WidgetZonelHelper.GetCustomWidgetZoneSelectList();
            foreach (var zone in availableWidgetZones)
                items.Add(zone);

            if (withSpecialDefaultItem)
                items.Insert(0, new SelectListItem()
                {
                    Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                    Value = "0"
                });
        }

        protected async Task PrepareDataSourceTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableDataSourceTypes = (await DataSourceTypeEnum.BestSellers.ToSelectListAsync(false)).ToList();
            foreach (var source in availableDataSourceTypes)
                items.Add(source);

            if (withSpecialDefaultItem)
                items.Insert(0, new SelectListItem()
                {
                    Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                    Value = "0"
                });
        }

        protected async Task PrepareActiveOptionsAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            items.Add(new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchActive.Active"),
                Value = "1"
            });
            items.Add(new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchActive.Inactive"),
                Value = "2"
            });

            if (withSpecialDefaultItem)
                items.Insert(0, new SelectListItem()
                {
                    Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                    Value = "0"
                });
        }

        #endregion

        #region Methods

        #region OCarousel

        public virtual async Task<OCarouselSearchModel> PrepareOCarouselSearchModelAsync(OCarouselSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            await PrepareCustomWidgetZonesAsync(searchModel.AvailableWidgetZones, true);
            await PrepareDataSourceTypesAsync(searchModel.AvailableDataSources, true);
            await PrepareActiveOptionsAsync(searchModel.AvailableActiveOptions, true);

            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            return searchModel;
        }

        public virtual async Task<OCarouselListModel> PrepareOCarouselListModelAsync(OCarouselSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var widgetZoneIds = searchModel.SearchWidgetZones?.Contains(0) ?? true ? null : searchModel.SearchWidgetZones.ToList();
            var dataSources = searchModel.SearchDataSources?.Contains(0) ?? true ? null : searchModel.SearchDataSources.ToList();

            bool? active = null;
            if (searchModel.SearchActiveId == 1)
                active = true;
            else if (searchModel.SearchActiveId == 2)
                active = false;

            //get carousels
            var carousels = await _carouselService.GetAllCarouselsAsync(widgetZoneIds, dataSources, searchModel.SearchStoreId, searchModel.VendorId,
                active, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = new OCarouselListModel().PrepareToGridAsync(searchModel, carousels, () =>
            {
                return carousels.SelectAwait(async carousel =>
                {
                    return await PrepareOCarouselModelAsync(null, carousel, true);
                });
            });

            return await model;
        }

        public async Task<OCarouselModel> PrepareOCarouselModelAsync(OCarouselModel model, OCarousel carousel, bool excludeProperties = false)
        {
            Func<OCarouselLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (carousel != null)
            {
                if (model == null)
                {
                    model = carousel.ToModel<OCarouselModel>();

                    model.DataSourceTypeStr = await _localizationService.GetLocalizedEnumAsync(carousel.DataSourceTypeEnum);
                    model.WidgetZoneStr = WidgetZonelHelper.GetCustomWidgetZone(carousel.WidgetZoneId);
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(carousel.CreatedOnUtc, DateTimeKind.Utc);
                    model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(carousel.UpdatedOnUtc, DateTimeKind.Utc);
                }

                if (!excludeProperties)
                {
                    model.OCarouselItemSearchModel = new OCarouselItemSearchModel()
                    {
                        OCarouselId = carousel.Id
                    };

                    localizedModelConfiguration = async (locale, languageId) =>
                    {
                        locale.Title = await _localizationService.GetLocalizedAsync(carousel, entity => entity.Title, languageId, false, false);
                    };
                }
            }

            if (!excludeProperties)
            {
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
                await PrepareCustomWidgetZonesAsync(model.AvailableWidgetZones, false);
                await PrepareDataSourceTypesAsync(model.AvailableDataSources, false);
                await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, carousel, excludeProperties);
            }

            return model;
        }

        #endregion

        #region OCarousel items

        public async Task<OCarouselItemListModel> PrepareOCarouselItemListModelAsync(OCarouselItemSearchModel searchModel, OCarousel carousel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (carousel == null)
                throw new ArgumentNullException(nameof(carousel));

            var carouselItems = await _carouselService.GetOCarouselItemsByOCarouselIdAsync(carousel.Id, searchModel.Page - 1, searchModel.PageSize);

            //prepare grid model
            var model = await new OCarouselItemListModel().PrepareToGridAsync(searchModel, carouselItems, () =>
            {
                //fill in model values from the entity
                return carouselItems.SelectAwait(async carouselItem =>
                {
                    var product = await _productService.GetProductByIdAsync(carouselItem.ProductId);
                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();

                    var carouselItemModel = carouselItem.ToModel<OCarouselItemModel>();
                    carouselItemModel.ProductName = product.Name;
                    carouselItemModel.PictureUrl = await _pictureService.GetPictureUrlAsync(defaultProductPicture.Id, 75);

                    return carouselItemModel;
                });
            });

            return model;
        }

        public async Task<AddProductToCarouselSearchModel> PrepareAddProductToOCarouselSearchModelAsync(AddProductToCarouselSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            ////prepare available vendors
            //await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare available product types
            await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

            //prepare page parameters
            searchModel.SetPopupGridPageSize();

            return searchModel;
        }

        public async Task<AddProductToCarouselListModel> PrepareAddProductToOCarouselListModelAsync(AddProductToCarouselSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new AddProductToCarouselListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    var productModel = product.ToModel<ProductModel>();
                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                    return productModel;
                });
            });

            return model;
        }

        #endregion

        #endregion
    }
}
