using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Factories
{
    public class DeviceModelFactory : IDeviceModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IApiDeviceService _deviceService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public DeviceModelFactory(CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IApiDeviceService deviceService,
            ICustomerService customerService,
            IStoreService storeService)
        {
            _catalogSettings = catalogSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _localizationService = localizationService;
            _deviceService = deviceService;
            _customerService = customerService;
            _storeService = storeService;
        }

        #endregion

        #region Utilities

        protected async Task PrepareDeviceTypesAsync(IList<SelectListItem> items, bool excludeDefaultItem = false)
        {
            var selectList = await DeviceType.Android.ToSelectListAsync(false);
            foreach (var item in selectList)
                items.Add(item);

            if (!excludeDefaultItem)
                items.Insert(0, new SelectListItem()
                {
                    Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                    Value = "0"
                });
        }

        #endregion

        #region Methods

        public virtual async Task<DeviceSearchModel> PrepareDeviceSearchModelAsync(DeviceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            await PrepareDeviceTypesAsync(searchModel.AvailableDeviceTypes);
            searchModel.SelectedDeviceTypes.Add(0);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<DeviceListModel> PrepareDeviceListModelAsync(DeviceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var selectedTypes = (searchModel.SelectedDeviceTypes?.Contains(0) ?? true) ? null : searchModel.SelectedDeviceTypes.ToList();

            //get devices
            var devices = await _deviceService.SearchApiDevicesAsync(0, selectedTypes, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = await new DeviceListModel().PrepareToGridAsync(searchModel, devices, () =>
            {
                return devices.SelectAwait(async device =>
                {
                    return await PrepareDeviceModelAsync(null, device);
                });
            });

            return model;
        }

        public virtual async Task<DeviceModel> PrepareDeviceModelAsync(DeviceModel model, ApiDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(ApiSlider));

            if (model == null)
            {
                model = device.ToModel<DeviceModel>();
                model.DeviceTypeStr = await _localizationService.GetLocalizedEnumAsync(device.DeviceType);
            }

            var customer = await _customerService.GetCustomerByIdAsync(device.CustomerId);
            model.CustomerName = customer?.Email ?? await _localizationService.GetResourceAsync("Admin.Customers.Guest");

            model.StoreName = (await _storeService.GetStoreByIdAsync(model.StoreId))?.Name ??
               await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Common.Unknown");
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(device.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(device.UpdatedOnUtc, DateTimeKind.Utc);

            return model;
        }

        #endregion
    }
}
