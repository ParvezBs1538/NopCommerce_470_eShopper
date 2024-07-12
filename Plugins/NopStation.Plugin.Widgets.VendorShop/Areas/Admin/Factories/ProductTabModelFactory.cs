using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.ProductTabs;
using NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Helpers;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Factories
{
    public partial class ProductTabModelFactory : IProductTabModelFactory
    {
        #region Fields

        private readonly IProductTabService _productTabService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ProductTabModelFactory(IProductTabService productTabService,
            ILocalizationService localizationService,
            IProductService productService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IUrlRecordService urlRecordService,
            IDateTimeHelper dateTimeHelper,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            ILocalizedModelFactory localizedModelFactory,
            IWorkContext workContext)
        {
            _productTabService = productTabService;
            _localizationService = localizationService;
            _productService = productService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _urlRecordService = urlRecordService;
            _dateTimeHelper = dateTimeHelper;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _localizedModelFactory = localizedModelFactory;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        protected async Task PrepareCustomWidgetZonesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //prepare available activity log types
            var availableWidgetZones = ProductTabHelper.GetCustomWidgetZoneSelectList();
            foreach (var zone in availableWidgetZones)
                items.Add(zone);

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
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.List.SearchActive.Active"),
                Value = "1"
            });
            items.Add(new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.List.SearchActive.Inactive"),
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

        #region Product tabs

        public async Task<ProductTabSearchModel> PrepareOCarouselSearchModelAsync(ProductTabSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            await PrepareCustomWidgetZonesAsync(searchModel.AvailableWidgetZones, true);
            await PrepareActiveOptionsAsync(searchModel.AvailableActiveOptions, true);

            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            return searchModel;
        }

        public virtual async Task<ProductTabListModel> PrepareProductTabListModelAsync(ProductTabSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var widgetZoneIds = searchModel.SearchWidgetZones?.Contains(0) ?? true ? null : searchModel.SearchWidgetZones.ToList();

            bool? active = null;
            if (searchModel.SearchActiveId == 1)
                active = true;
            else if (searchModel.SearchActiveId == 2)
                active = false;

            var vendorId = (await _workContext.GetCurrentVendorAsync())?.Id ?? 0;

            //get productTabs
            var productTabs = await _productTabService.GetAllProductTabsAsync(vendorId, widgetZoneIds, false,
                searchModel.SearchStoreId, active, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = new ProductTabListModel().PrepareToGridAsync(searchModel, productTabs, () =>
            {
                return productTabs.SelectAwait(async productTab =>
                {
                    return await PrepareProductTabModelAsync(null, productTab, true);
                });
            });

            return await model;
        }

        public async Task<ProductTabModel> PrepareProductTabModelAsync(ProductTabModel model, ProductTab productTab,
            bool excludeProperties = false)
        {
            Func<ProductTabLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (productTab != null)
            {
                if (model == null)
                {
                    model = productTab.ToModel<ProductTabModel>();
                    model.WidgetZoneStr = ProductTabHelper.GetCustomWidgetZone(productTab.WidgetZoneId);
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(productTab.CreatedOnUtc, DateTimeKind.Utc);
                    model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(productTab.UpdatedOnUtc, DateTimeKind.Utc);
                }

                if (!excludeProperties)
                {
                    model.ProductTabItemSearchModel = new ProductTabItemSearchModel()
                    {
                        ProductTabId = productTab.Id
                    };

                    localizedModelConfiguration = async (locale, languageId) =>
                    {
                        locale.Name = await _localizationService.GetLocalizedAsync(productTab, entity => entity.Name, languageId, false, false);
                        locale.TabTitle = await _localizationService.GetLocalizedAsync(productTab, entity => entity.TabTitle, languageId, false, false);
                    };
                }
            }

            if (!excludeProperties)
            {
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
                await PrepareCustomWidgetZonesAsync(model.AvailableWidgetZones, false);
                await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, productTab, excludeProperties);
            }

            return model;
        }

        #endregion

        #region Product tab items

        public async Task<ProductTabItemListModel> PrepareProductTabItemListModelAsync(ProductTabItemSearchModel searchModel, ProductTab productTab)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (productTab == null)
                throw new ArgumentNullException(nameof(productTab));

            var productTabItems = _productTabService.GetProductTabItemsByProductTabId(productTab.Id).OrderBy(x => x.DisplayOrder).ToList().ToPagedList(searchModel);

            //prepare grid model
            var model = new ProductTabItemListModel().PrepareToGridAsync(searchModel, productTabItems, () =>
            {
                //fill in model values from the entity
                return productTabItems.SelectAwait(async productTabItem =>
                {
                    var productTabItemModel = new ProductTabItemModel
                    {
                        Id = productTabItem.Id,
                        DisplayOrder = productTabItem.DisplayOrder,
                        Name = productTabItem.Name,
                        ProductTabId = productTabItem.ProductTabId
                    };
                    return await PrepareProductTabItemModelAsync(productTabItemModel, productTabItem, productTab, true);
                });
            });

            return await model;
        }

        public async Task<ProductTabItemModel> PrepareProductTabItemModelAsync(ProductTabItemModel model, ProductTabItem productTabItem,
            ProductTab productTab, bool excludeProperties = false)
        {
            Func<ProductTabItemLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (productTabItem != null)
                if (!excludeProperties)
                    localizedModelConfiguration = async (locale, languageId) =>
                    {
                        locale.Name = await _localizationService.GetLocalizedAsync(productTabItem, entity => entity.Name, languageId, false, false);
                    };
                else
                if (!excludeProperties)
                    try
                    {
                        model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
                    }
                    catch (Exception ex)
                    {
                        var myException = ex.InnerException.Message;
                    }

            return model;
        }
        #endregion

        #region Product tab items

        public async Task<ProductTabItemProductListModel> PrepareProductTabItemProductListModelAsync(ProductTabItemProductSearchModel searchModel, ProductTabItem productTabItem)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (productTabItem == null)
                throw new ArgumentNullException(nameof(productTabItem));

            var productTabItemProducts = _productTabService.GetProductTabItemProductsByProductTabItemId(productTabItem.Id).OrderBy(x => x.DisplayOrder).ToList().ToPagedList(searchModel);
            //productTabItem.ProductTabItemProducts.OrderBy(x => x.DisplayOrder).ToList().ToPagedList(searchModel);

            //prepare grid model
            var model = new ProductTabItemProductListModel().PrepareToGridAsync(searchModel, productTabItemProducts, () =>
            {
                //fill in model values from the entity
                return productTabItemProducts.SelectAwait(async product =>
                {
                    return await PrepareProductTabItemProductModelAsync(null, product, productTabItem);
                });
            });

            return await model;
        }

        protected async Task<ProductTabItemProductModel> PrepareProductTabItemProductModelAsync(ProductTabItemProductModel model, ProductTabItemProduct itemProduct,
            ProductTabItem productTabItem)
        {
            if (itemProduct != null)
                if (model == null)
                {
                    model = itemProduct.ToModel<ProductTabItemProductModel>();
                    var product = await _productService.GetProductByIdAsync(itemProduct.ProductId);
                    model.ProductName = product?.Name;
                }

            return model;
        }

        public async Task<AddProductToProductTabItemSearchModel> PrepareAddProductToProductTabItemSearchModelAsync(AddProductToProductTabItemSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available product types
            await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

            //prepare page parameters
            searchModel.SetPopupGridPageSize();

            return searchModel;
        }

        public async Task<AddProductToProductTabItemListModel> PrepareAddProductToProductTabItemListModelAsync(AddProductToProductTabItemSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendorId = (await _workContext.GetCurrentVendorAsync())?.Id ?? 0;

            //get products
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: vendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = new AddProductToProductTabItemListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    var productModel = product.ToModel<ProductModel>();
                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                    return productModel;
                });
            });

            return await model;
        }

        #endregion

        #endregion
    }
}
