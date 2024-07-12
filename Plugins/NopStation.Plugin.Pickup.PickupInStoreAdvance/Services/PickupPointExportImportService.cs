using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.ExportImport.Help;
using Nop.Services.Logging;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Services
{
    public class PickupPointExportImportService : IPickupPointExportImportService
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly IStorePickupPointService _storePickupPointService;
        private readonly ILogger _logger;

        public PickupPointExportImportService(CatalogSettings catalogSettings,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            IStorePickupPointService storePickupPointService,
            ILogger logger)
        {
            _catalogSettings = catalogSettings;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _addressService = addressService;
            _storePickupPointService = storePickupPointService;
            _logger = logger;
        }

        public async Task<byte[]> ExportToXlsxAsync(IList<StorePickupPointsExportImportModel> models)
        {
            var manager = new PropertyManager<StorePickupPointsExportImportModel, Language>(new[]
            {
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.Id), (p, l) => p.Id),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.Name), (p, l) => p.Name),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.Description), (p, l) => p.Description),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.DisplayOrder), (p, l) => p.DisplayOrder),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.TransitDays), (p, l) => p.TransitDays),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.PickupFee), (p, l) => p.PickupFee),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.OpeningHours), (p, l) => p.OpeningHours),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.StoreId), (p, l) => p.StoreId),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.Active), (p, l) => p.Active),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.Latitude), (p, l) => p.Latitude),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.Longitude), (p, l) => p.Longitude),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.Address1), (p, l) => p.Address1),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.Address2), (p, l) => p.Address2),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.City), (p, l) => p.City),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.ZipPostalCode), (p, l) => p.ZipPostalCode),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.CountryName), (p, l) => p.CountryName),
                new PropertyByName<StorePickupPointsExportImportModel, Language>(nameof(StorePickupPointsExportImportModel.StateName), (p, l) => p.StateName),

            }, _catalogSettings);

            return await manager.ExportToXlsxAsync(models);
        }

        public async Task ImportFromXlsxAsync(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            // get the first worksheet in the workbook
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            //the columns
            var properties = GetPropertiesByExcelCells<StorePickupPointsExportImportModel>(worksheet);

            var manager = new PropertyManager<StorePickupPointsExportImportModel, Language>(properties, _catalogSettings);
            var iRow = 2;

            var saveNextTime = new List<int>();
            var pickupPoints = new List<StorePickupPointsExportImportModel>();
            while (true)
            {
                var allColumnsAreEmpty = manager.GetDefaultProperties
                    .Select(property => worksheet.Row(iRow).Cell(property.PropertyOrderPosition))
                    .All(cell => string.IsNullOrEmpty(cell?.Value.ToString()));

                if (allColumnsAreEmpty)
                    break;

                //get pickup points by data in xlsx file if it possible
                var pickupPoint = await GetFromXlsxAsync(manager, worksheet, iRow);
                if (pickupPoint != null)
                {
                    pickupPoints.Add(pickupPoint);
                }
                iRow++;
            }
            await InsertPickupPointsAsync(pickupPoints);
        }

        protected async Task InsertPickupPointsAsync(List<StorePickupPointsExportImportModel> pickupPoints)
        {
            var allCountries = await _countryService.GetAllCountriesAsync();
            //var AllStates=_stateProvinceService.getA
            foreach (var pickupPoint in pickupPoints)
            {
                var country = allCountries.Where(x => x.Name.Equals(pickupPoint.CountryName)).FirstOrDefault();
                int? countryId = null;
                int? stateId = null;
                if (country != null)
                {
                    countryId = country.Id;
                    var allStates = await _stateProvinceService.GetStateProvincesByCountryIdAsync(country.Id);
                    var state = allStates.Where(x => x.Name.Equals(pickupPoint.StateName)).FirstOrDefault();
                    if (state != null)
                    {
                        stateId = state.Id;
                    }
                }
                var storePickupPoint = new StorePickupPoint
                {
                    Id = pickupPoint.Id,
                    Name = pickupPoint.Name,
                    OpeningHours = pickupPoint.OpeningHours,
                    PickupFee = pickupPoint.PickupFee,
                    Latitude = pickupPoint.Latitude,
                    Longitude = pickupPoint.Longitude,
                    StoreId = pickupPoint.StoreId,
                    Active = pickupPoint.Active,
                    TransitDays = pickupPoint.TransitDays,
                    Description = pickupPoint.Description,
                    DisplayOrder = pickupPoint.DisplayOrder
                };
                if (storePickupPoint.Id == 0)
                {
                    var address = new Address
                    {
                        Address1 = pickupPoint.Address1,
                        Address2 = pickupPoint.Address2,
                        City = pickupPoint.City,
                        ZipPostalCode = pickupPoint.ZipPostalCode,
                        CountryId = countryId,
                        StateProvinceId = stateId
                    };
                    await _addressService.InsertAddressAsync(address);
                    storePickupPoint.AddressId = address.Id;
                    await _storePickupPointService.InsertStorePickupPointAsync(storePickupPoint);
                }
                else
                {
                    var tempPickupPoint = await _storePickupPointService.GetStorePickupPointByIdAsync(storePickupPoint.Id);
                    if (tempPickupPoint != null)
                    {
                        var address = await _addressService.GetAddressByIdAsync(tempPickupPoint.AddressId);
                        if (address == null)
                        {
                            address = new Address();
                        }
                        address.Address1 = pickupPoint.Address1;
                        address.Address2 = pickupPoint.Address2;
                        address.City = pickupPoint.City;
                        address.ZipPostalCode = pickupPoint.ZipPostalCode;
                        address.CountryId = countryId;
                        address.StateProvinceId = stateId;
                        await _addressService.UpdateAddressAsync(address);
                        await _storePickupPointService.UpdateStorePickupPointAsync(storePickupPoint);
                    }
                    else
                    {
                        var address = new Address
                        {
                            Address1 = pickupPoint.Address1,
                            Address2 = pickupPoint.Address2,
                            City = pickupPoint.City,
                            ZipPostalCode = pickupPoint.ZipPostalCode,
                            CountryId = countryId,
                            StateProvinceId = stateId
                        };
                        await _addressService.InsertAddressAsync(address);
                        storePickupPoint.AddressId = address.Id;
                        await _storePickupPointService.InsertStorePickupPointAsync(storePickupPoint);
                    }

                }
            }
        }

        protected virtual async Task<StorePickupPointsExportImportModel> GetFromXlsxAsync(PropertyManager<StorePickupPointsExportImportModel, Language> manager, IXLWorksheet worksheet, int iRow)
        {
            manager.ReadDefaultFromXlsx(worksheet, iRow);

            //try get category from database by ID
            try
            {
                var fee = 0.0M;
                var displayOrder = 0;
                var id = 0;
                var storeId = 0;
                var latitude = 0.0M;
                var longitude = 0.0M;
                var pickupPoint = new StorePickupPointsExportImportModel
                {
                    Id = int.TryParse(manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.Id)).StringValue, out id) ? id : 0,
                    Name = manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.Name)).StringValue,
                    Description = manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.Description)).StringValue,
                    PickupFee = decimal.TryParse(manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.PickupFee)).StringValue, out fee) ? fee : 0.0M,
                    OpeningHours = manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.OpeningHours)).StringValue,
                    DisplayOrder = int.TryParse(manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.DisplayOrder)).StringValue, out displayOrder) ? displayOrder : 0,
                    StoreId = int.TryParse(manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.StoreId)).StringValue, out storeId) ? storeId : 0,
                    Active = manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.Active)).BooleanValue,
                    Latitude = decimal.TryParse(manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.Latitude)).StringValue, out latitude) ? latitude : (decimal?)null,
                    Longitude = decimal.TryParse(manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.Longitude)).StringValue, out longitude) ? longitude : (decimal?)null,
                    TransitDays = int.TryParse(manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.TransitDays)).StringValue, out storeId) ? storeId : (int?)null,
                    Address1 = manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.Address1)).StringValue,
                    Address2 = manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.Address2)).StringValue,
                    ZipPostalCode = manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.ZipPostalCode)).StringValue,
                    CountryName = manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.CountryName)).StringValue,
                    StateName = manager.GetDefaultProperty(nameof(StorePickupPointsExportImportModel.StateName)).StringValue,
                };
                return pickupPoint;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return null;
            }
        }

        public static IList<PropertyByName<T, Language>> GetPropertiesByExcelCells<T>(IXLWorksheet worksheet)
        {
            var properties = new List<PropertyByName<T, Language>>();
            var poz = 1;
            while (true)
            {
                try
                {
                    var cell = worksheet.Row(1).Cell(poz);

                    if (string.IsNullOrEmpty(cell?.Value.ToString()))
                        break;

                    poz += 1;
                    properties.Add(new PropertyByName<T, Language>(cell.Value.ToString()));
                }
                catch
                {
                    break;
                }
            }

            return properties;
        }
    }
}
