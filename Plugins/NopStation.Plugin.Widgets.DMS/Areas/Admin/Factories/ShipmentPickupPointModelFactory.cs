using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Directory;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models.ShipmentPickupPoint;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories
{
    public class ShipmentPickupPointModelFactory : IShipmentPickupPointModelFactory
    {
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IShipmentPickupPointService _shipmentPickupPointService;
        private readonly IStateProvinceService _stateProvinceService;
        #region Fields



        #endregion

        #region Ctor

        public ShipmentPickupPointModelFactory(
            IAddressService addressService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IShipmentPickupPointService shipmentPickupPointService,
            IStateProvinceService stateProvinceService
            )
        {
            _addressService = addressService;
            _countryService = countryService;
            _localizationService = localizationService;
            _shipmentPickupPointService = shipmentPickupPointService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods


        public virtual ShipmentPickupPointSearchModel PrepareShipmentPickupPointSearchModel(ShipmentPickupPointSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<ShipmentPickupPointListModel> PrepareShipmentPickupPointListModelAsync(ShipmentPickupPointSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var shipmentPickupPoints = await _shipmentPickupPointService.GetAllShipmentPickupPointsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new ShipmentPickupPointListModel().PrepareToGridAsync(searchModel, shipmentPickupPoints, () =>
            {
                return shipmentPickupPoints.SelectAwait(async shipmentPickupPoint =>
                {
                    var sppModel = shipmentPickupPoint.ToModel<ShipmentPickupPointModel>();

                    await Task.CompletedTask;
                    return sppModel;
                });
            });

            return model;
        }

        public virtual async Task<ShipmentPickupPointModel> PrepareShipmentPickupPointModelAsync(ShipmentPickupPointModel model, ShipmentPickupPoint shipmentPickupPoint, bool excludeProperties = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (shipmentPickupPoint != null)
            {
                model = shipmentPickupPoint.ToModel<ShipmentPickupPointModel>();

                var address = await _addressService.GetAddressByIdAsync(shipmentPickupPoint.AddressId);
                if (address != null)
                {
                    model.Address = new AddressModel
                    {
                        Address1 = address.Address1,
                        City = address.City,
                        County = address.County,
                        CountryId = address.CountryId,
                        StateProvinceId = address.StateProvinceId,
                        ZipPostalCode = address.ZipPostalCode,
                    };
                }
            }

            model.Address.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var country in await _countryService.GetAllCountriesAsync(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = country.Name, Value = country.Id.ToString() });

            var states = !model.Address.CountryId.HasValue ? new List<StateProvince>()
                : await _stateProvinceService.GetStateProvincesByCountryIdAsync(model.Address.CountryId.Value, showHidden: true);
            if (states.Any())
            {
                model.Address.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Address.SelectState"), Value = "0" });
                foreach (var state in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = state.Name, Value = state.Id.ToString() });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Address.Other"), Value = "0" });

            return model;
        }

        #endregion
    }
}
