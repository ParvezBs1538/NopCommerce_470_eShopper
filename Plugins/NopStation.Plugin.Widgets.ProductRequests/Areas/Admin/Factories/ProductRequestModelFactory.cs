using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductRequests.Domains;
using NopStation.Plugin.Widgets.ProductRequests.Services;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Factories
{
    public partial class ProductRequestModelFactory : IProductRequestModelFactory
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IProductRequestService _productRequestService;
        private readonly ISettingService _settingService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public ProductRequestModelFactory(IStoreContext storeContext,
            IBaseAdminModelFactory baseAdminModelFactory,
            IProductRequestService productRequestService,
            ISettingService settingService,
            IDateTimeHelper dateTimeHelper,
            ICustomerService customerService,
            IStoreService storeService)
        {
            _storeContext = storeContext;
            _baseAdminModelFactory = baseAdminModelFactory;
            _productRequestService = productRequestService;
            _settingService = settingService;
            _dateTimeHelper = dateTimeHelper;
            _customerService = customerService;
            _storeService = storeService;
        }

        #endregion

        #region Methods

        #region Configuration

        public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
        {
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var productRequestSettings = await _settingService.LoadSettingAsync<ProductRequestSettings>(storeId);

            var model = productRequestSettings.ToSettingsModel<ConfigurationModel>();
            await _baseAdminModelFactory.PrepareCustomerRolesAsync(model.AvailableCustomerRoles, false);

            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return model;

            model.AllowedCustomerRolesIds_OverrideForStore = await _settingService.SettingExistsAsync(productRequestSettings, x => x.AllowedCustomerRolesIds, storeId);
            model.FooterElementSelector_OverrideForStore = await _settingService.SettingExistsAsync(productRequestSettings, x => x.FooterElementSelector, storeId);
            model.IncludeInFooter_OverrideForStore = await _settingService.SettingExistsAsync(productRequestSettings, x => x.IncludeInFooter, storeId);
            model.IncludeInTopMenu_OverrideForStore = await _settingService.SettingExistsAsync(productRequestSettings, x => x.FooterElementSelector, storeId);
            model.DescriptionRequired_OverrideForStore = await _settingService.SettingExistsAsync(productRequestSettings, x => x.DescriptionRequired, storeId);
            model.LinkRequired_OverrideForStore = await _settingService.SettingExistsAsync(productRequestSettings, x => x.LinkRequired, storeId);
            model.MinimumProductRequestCreateInterval_OverrideForStore = await _settingService.SettingExistsAsync(productRequestSettings, x => x.MinimumProductRequestCreateInterval, storeId);

            return model;
        }

        #endregion

        #region Product request

        public async virtual Task<ProductRequestSearchModel> PrepareProductRequestSearchModelAsync(ProductRequestSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            return searchModel;
        }

        public async virtual Task<ProductRequestListModel> PrepareProductRequestListModelAsync(ProductRequestSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get productRequests
            var productRequests = await _productRequestService.GetAllProductRequestsAsync(
                name: searchModel.SearchName,
                customerEmail: searchModel.SearchCustomerEmail,
                storeId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new ProductRequestListModel().PrepareToGridAsync(searchModel, productRequests, () =>
            {
                return productRequests.SelectAwait(async productRequest =>
                {
                    return await PrepareProductRequestModelAsync(null, productRequest, true);
                });
            });

            return model;
        }

        public async Task<ProductRequestModel> PrepareProductRequestModelAsync(ProductRequestModel model, ProductRequest productRequest, bool excludeProperties = false)
        {
            if (productRequest != null)
            {
                if (model == null)
                {
                    model = productRequest.ToModel<ProductRequestModel>();
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(productRequest.CreatedOnUtc, DateTimeKind.Utc);
                    model.Customer = (await _customerService.GetCustomerByIdAsync(productRequest.CustomerId))?.Email;
                    model.Store = (await _storeService.GetStoreByIdAsync(productRequest.StoreId))?.Name;
                }
            }

            if (!excludeProperties)
            {
            }

            return model;
        }

        #endregion

        #endregion
    }
}
