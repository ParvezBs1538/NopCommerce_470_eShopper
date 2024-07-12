using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Flipbooks.Domains;
using NopStation.Plugin.Widgets.Flipbooks.Services;
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
using Nop.Web.Framework.Extensions;

namespace NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Factories
{
    public class FlipbookModelFactory : IFlipbookModelFactory
    {
        #region Fields

        private readonly IFlipbookService _flipbookService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        public FlipbookModelFactory(IFlipbookService flipbookService,
            IPictureService pictureService,
            IProductService productService,
            IUrlRecordService urlRecordService,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            IDateTimeHelper dateTimeHelper,
            CatalogSettings catalogSettings)
        {
            _flipbookService = flipbookService;
            _pictureService = pictureService;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Utilities

        protected async Task<string> PrepareContentsAsync(FlipbookContent flipbookContent)
        {
            if (flipbookContent == null)
                throw new ArgumentNullException(nameof(flipbookContent));

            if (flipbookContent.IsImage)
                return string.Empty;

            var products = await _flipbookService.GetProductsByFlipbookContentIdAsync(flipbookContent.Id);
            var content = "";
            var i = 1;

            foreach (var product in products)
            {
                content += $"{i}. {await _localizationService.GetLocalizedAsync(product, x => x.Name)} </br>";
                i++;
            }

            return content;
        }

        protected async Task<FlipbookContentSearchModel> PrepareFlipbookContentSearchModelAsync(FlipbookContentSearchModel searchModel, Flipbook flipbook)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (flipbook == null)
                throw new ArgumentNullException(nameof(flipbook));

            searchModel.FlipbookId = flipbook.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return await Task.FromResult(searchModel);
        }

        #endregion

        #region Methods

        #region Flipbooks

        public async Task<FlipbookSearchModel> PrepareFlipbookSearchModelAsync(FlipbookSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            searchModel.AvailableActiveOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.Flipbooks.Flipbooks.List.SearchActive.All")
            });
            searchModel.AvailableActiveOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.Flipbooks.Flipbooks.List.SearchActive.ActiveOnly")
            });
            searchModel.AvailableActiveOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.Flipbooks.Flipbooks.List.SearchActive.InactiveOnly")
            });

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task<FlipbookListModel> PrepareFlipbookListModelAsync(FlipbookSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            bool? active = null;
            if (searchModel.SearchActiveId > 0)
                active = searchModel.SearchActiveId == 1;

            var flipbooks = await _flipbookService.SearchFlipbooksAsync(searchModel.SearchName,
                showHidden: true,
                active: active,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new FlipbookListModel().PrepareToGridAsync(searchModel, flipbooks, () =>
            {
                return flipbooks.SelectAwait(async flipbook =>
                {
                    return await PrepareFlipbookModelAsync(null, flipbook, true);
                });
            });

            return model;
        }

        public async Task<FlipbookModel> PrepareFlipbookModelAsync(FlipbookModel model, Flipbook flipbook, bool excludeProperties = false)
        {
            Func<FlipbookLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (flipbook != null)
            {
                if (model == null)
                {
                    model = flipbook.ToModel<FlipbookModel>();
                    model.SeName = await _urlRecordService.GetSeNameAsync(flipbook, 0, true, false);
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(flipbook.CreatedOnUtc, DateTimeKind.Utc);
                    model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(flipbook.UpdatedOnUtc, DateTimeKind.Utc);

                    if (flipbook.AvailableStartDateTimeUtc.HasValue)
                        model.AvailableStartDateTime = await _dateTimeHelper.ConvertToUserTimeAsync(flipbook.AvailableStartDateTimeUtc.Value, DateTimeKind.Utc);
                    if (flipbook.AvailableEndDateTimeUtc.HasValue)
                        model.AvailableEndDateTime = await _dateTimeHelper.ConvertToUserTimeAsync(flipbook.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc);

                    //prepare localized models
                    if (!excludeProperties)
                    {
                        await PrepareFlipbookContentSearchModelAsync(model.FlipbookContentSearchModel, flipbook);

                        //define localized model configuration action
                        localizedModelConfiguration = async (locale, languageId) =>
                        {
                            locale.Name = await _localizationService.GetLocalizedAsync(flipbook, entity => entity.Name, languageId, false, false);
                            locale.MetaKeywords = await _localizationService.GetLocalizedAsync(flipbook, entity => entity.MetaKeywords, languageId, false, false);
                            locale.MetaDescription = await _localizationService.GetLocalizedAsync(flipbook, entity => entity.MetaDescription, languageId, false, false);
                            locale.MetaTitle = await _localizationService.GetLocalizedAsync(flipbook, entity => entity.MetaTitle, languageId, false, false);
                            locale.SeName = await _urlRecordService.GetSeNameAsync(flipbook, languageId, false, false);
                        };
                    }
                }
            }

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            //prepare available stores
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, flipbook, excludeProperties);

            //prepare model customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, flipbook, excludeProperties);

            return model;
        }

        #endregion

        #region Flipbook contents

        public async Task<FlipbookContentListModel> PrepareFlipbookContentListModelAsync(FlipbookContentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var activeFlipbookContents = (await _flipbookService.GetFlipbookContentsByFlipbookIdAsync(searchModel.FlipbookId))
                .ToPagedList(searchModel);

            var model = await new FlipbookContentListModel().PrepareToGridAsync(searchModel, activeFlipbookContents, () =>
            {
                return activeFlipbookContents.SelectAwait(async x =>
                {
                    var cm = new FlipbookContentModel
                    {
                        Id = x.Id,
                        DisplayOrder = x.DisplayOrder,
                        ImageUrl = x.ImageId > 0 ? await _pictureService.GetPictureUrlAsync(x.ImageId) : "",
                        ImageId = x.ImageId,
                        IsImage = x.IsImage,
                        RedirectUrl = x.RedirectUrl,
                        Content = await PrepareContentsAsync(x)
                    };

                    return cm;
                });
            });

            return model;
        }

        public async Task<FlipbookContentModel> PrepareFlipbookContentModelAsync(FlipbookContentModel model, FlipbookContent flipbookContent, Flipbook flipbook)
        {
            if (flipbook == null)
                throw new ArgumentNullException(nameof(flipbook));

            if (flipbookContent != null)
            {
                if (model == null)
                {
                    model = new FlipbookContentModel()
                    {
                        DisplayOrder = flipbookContent.DisplayOrder,
                        Id = flipbookContent.Id,
                        ImageId = flipbookContent.ImageId,
                        IsImage = flipbookContent.IsImage,
                        RedirectUrl = flipbookContent.IsImage ? flipbookContent.RedirectUrl : ""
                    };

                    model.ImageUrl = await _pictureService.GetPictureUrlAsync(flipbookContent.ImageId);
                    model.FlipbookContentProductSearchModel.FlipbookContentId = flipbookContent.Id;
                }
            }

            model.FlipbookId = flipbook.Id;
            return model;
        }

        #endregion

        #region Content products

        public async Task<FlipbookContentProductListModel> PrepareFlipbookContentProductListModelAsync(FlipbookContentProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var products = (await _flipbookService.GetFlipbookContentProductsByContentIdAsync(searchModel.FlipbookContentId)).ToPagedList(searchModel);

            var model = await new FlipbookContentProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async p =>
                {
                    var product = await _productService.GetProductByIdAsync(p.ProductId);
                    var cm = new FlipbookContentProductModel
                    {
                        Id = p.Id,
                        DisplayOrder = p.DisplayOrder,
                        FlipbookContentId = p.FlipbookContentId,
                        ProductId = p.ProductId,
                        ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name)
                    };

                    return cm;
                });
            });

            return model;
        }

        public async Task<AddProductToFlipbookContentListModel> PrepareAddProductToFlipbookContentListModelAsync(AddProductToFlipbookContentSearchModel searchModel)
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
            var model = await new AddProductToFlipbookContentListModel().PrepareToGridAsync(searchModel, products, () =>
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

        public async Task<AddProductToFlipbookContentSearchModel> PrepareAddProductToFlipbookContentSearchModelAsync(AddProductToFlipbookContentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare available product types
            await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

            //prepare page parameters
            searchModel.SetPopupGridPageSize();

            return searchModel;
        }

        #endregion

        #endregion
    }
}
