using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories
{
    public class WebAppDeviceModelFactory : IWebAppDeviceModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebAppDeviceService _webAppDeviceService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public WebAppDeviceModelFactory(CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IWebAppDeviceService webAppDeviceService,
            ICustomerService customerService,
            IStoreService storeService)
        {
            _catalogSettings = catalogSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _localizationService = localizationService;
            _webAppDeviceService = webAppDeviceService;
            _customerService = customerService;
            _storeService = storeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare webAppDevice search model
        /// </summary>
        /// <param name="searchModel">WebAppDevice search model</param>
        /// <returns>WebAppDevice search model</returns>
        public virtual WebAppDeviceSearchModel PrepareWebAppDeviceSearchModel(WebAppDeviceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged webAppDevice list model
        /// </summary>
        /// <param name="searchModel">WebAppDevice search model</param>
        /// <returns>WebAppDevice list model</returns>
        public virtual async Task<WebAppDeviceListModel> PrepareWebAppDeviceListModelAsync(WebAppDeviceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get webAppDevices
            var webAppDevices = (await _webAppDeviceService.GetWebAppDevicesAsync()).ToPagedList(searchModel);

            //prepare list model
            var model = await new WebAppDeviceListModel().PrepareToGridAsync(searchModel, webAppDevices, () =>
            {
                return webAppDevices.SelectAwait(async webAppDevice =>
                {
                    return await PrepareWebAppDeviceModelAsync(null, webAppDevice, true);
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare webAppDevice model
        /// </summary>
        /// <param name="model">WebAppDevice model</param>
        /// <param name="webAppDevice">WebAppDevice</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>WebAppDevice model</returns>
        public virtual async Task<WebAppDeviceModel> PrepareWebAppDeviceModelAsync(WebAppDeviceModel model,
            WebAppDevice webAppDevice, bool excludeProperties = false)
        {
            if (webAppDevice != null)
            {
                //fill in model values from the entity
                model = model ?? webAppDevice.ToModel<WebAppDeviceModel>();

                var customer = await _customerService.GetCustomerByIdAsync(webAppDevice.CustomerId);
                if (customer == null || customer.Deleted || !await _customerService.IsRegisteredAsync(customer))
                {
                    model.CustomerId = 0;
                    model.CustomerName = "Guest";
                }
                else
                    model.CustomerName = customer.Email;

                var store = await _storeService.GetStoreByIdAsync(webAppDevice.StoreId);
                if (store != null)
                    model.StoreName = store.Name;
                else
                {
                    model.StoreId = 0;
                    model.StoreName = "Unknown";
                }

                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(webAppDevice.CreatedOnUtc, DateTimeKind.Utc);
            }

            if (!excludeProperties)
            {

            }
            return model;
        }

        #endregion
    }
}
