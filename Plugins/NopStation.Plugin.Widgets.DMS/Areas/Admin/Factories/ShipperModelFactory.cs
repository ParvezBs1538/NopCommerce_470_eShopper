using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories
{
    public class ShipperModelFactory : IShipperModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IShipperService _shipperService;
        private readonly ICustomerService _customerService;
        private readonly ICourierShipmentModelFactory _courierShipmentModelFactory;
        private readonly IShipperDeviceService _shipperDeviceService;
        private readonly DMSSettings _dMSSettings;

        #endregion

        #region Ctor

        public ShipperModelFactory(CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IShipperService shipperService,
            ICustomerService customerService,
            ICourierShipmentModelFactory courierShipmentModelFactory,
            IShipperDeviceService shipperDeviceService,
            DMSSettings dMSSettings)
        {
            _catalogSettings = catalogSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _localizationService = localizationService;
            _shipperService = shipperService;
            _customerService = customerService;
            _courierShipmentModelFactory = courierShipmentModelFactory;
            _shipperDeviceService = shipperDeviceService;
            _dMSSettings = dMSSettings;
        }

        #endregion

        #region Methods
        public virtual ShipperSearchModel PrepareShipperSearchModel(ShipperSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<ShipperListModel> PrepareShipperListModelAsync(ShipperSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var shippers = await _shipperService.GetAllShippersAsync(email: searchModel.SearchEmail, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new ShipperListModel().PrepareToGridAsync(searchModel, shippers, () =>
            {
                return shippers.SelectAwait(async shipper =>
                {
                    return await PrepareShipperModelAsync(null, shipper);
                });
            });

            return model;
        }

        public virtual async Task<ShipperModel> PrepareShipperModelAsync(ShipperModel model, Shipper shipper, bool excludeProperties = false)
        {
            if (shipper != null)
            {
                if (!excludeProperties)
                {
                    model = shipper.ToModel<ShipperModel>();
                    model.CustomerName = (await _customerService.GetCustomerByIdAsync(shipper.CustomerId)).Email;
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(shipper.CreatedOnUtc, DateTimeKind.Utc);

                    model.CourierShipmentSearchModel = await _courierShipmentModelFactory.PrepareCourierShipmentSearchModelAsync(new CourierShipmentSearchModel());
                    model.CourierShipmentSearchModel.SearchShipperId = shipper.Id;

                    var device = await _shipperDeviceService.GetShipperDeviceByCustomerIdAsync(shipper.CustomerId);
                    if (device != null)
                    {
                        model.LocationUpdateIntervalInSeconds = (decimal)_dMSSettings.LocationUpdateIntervalInSeconds * 1000M;
                        model.LastLocation = $"{device.Latitude},{device.Longitude}";
                        model.GeoMapId = _dMSSettings.GeoMapId;
                        model.GoogleApiKey = _dMSSettings.GoogleMapApiKey;
                        model.Latitude = device.Latitude;
                        model.Longitude = device.Longitude;
                        model.Longitude = device.Longitude;
                        model.Online = device.Online;
                        if (string.IsNullOrEmpty(model.GoogleApiKey))
                        {
                            model.GoogleApiKey = DMSDefaults.DummyGoogleAPiKey;
                        }
                    }
                }
            }
            return model;
        }
        #endregion
    }
}
