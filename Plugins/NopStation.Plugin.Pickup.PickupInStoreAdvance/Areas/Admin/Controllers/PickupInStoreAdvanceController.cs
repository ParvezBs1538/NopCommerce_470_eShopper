using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Factories;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Mvc;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Controllers
{
    public class PickupInStoreAdvanceController : NopStationAdminController
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStorePickupPointModelFactory _storePickupPointModelFactory;
        private readonly IStorePickupPointService _storePickupPointService;
        private readonly IStoreService _storeService;
        private readonly AddressSettings _addressSettings;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly IPickupInStoreMessageService _pickupInStoreMessageService;
        private readonly IWorkContext _workContext;
        private readonly IPickupPointExportImportService _pickupPointExportImportService;

        #endregion

        #region Ctor

        public PickupInStoreAdvanceController(IAddressService addressService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStateProvinceService stateProvinceService,
            IStorePickupPointModelFactory storePickupPointModelFactory,
            IStorePickupPointService storePickupPointService,
            IStoreService storeService,
            AddressSettings customerSettings,
            ISettingService settingService,
            IStoreContext storeContext,
            INotificationService notificationService,
            IPickupInStoreMessageService pickupInStoreMessageService,
            IWorkContext workContext,
            IPickupPointExportImportService pickupPointExportImportService)
        {
            _addressService = addressService;
            _countryService = countryService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _stateProvinceService = stateProvinceService;
            _storePickupPointModelFactory = storePickupPointModelFactory;
            _storePickupPointService = storePickupPointService;
            _storeService = storeService;
            _addressSettings = customerSettings;
            _settingService = settingService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _pickupInStoreMessageService = pickupInStoreMessageService;
            _workContext = workContext;
            _pickupPointExportImportService = pickupPointExportImportService;
        }

        #endregion

        #region Utilities

        protected async Task<PickupInStoreSettingsModel> GetPickupInStoreSettingsModelAsync(PickupInStoreSettings settings)
        {
            var model = new PickupInStoreSettingsModel
            {
                StorePickupPointSearchModel = await _storePickupPointModelFactory.PrepareStorePickupPointSearchModelAsync(new StorePickupPointSearchModel()),
                AddOrderNote = settings.AddOrderNote,
                NotifyCustomerIfReady = settings.NotifyCustomerIfReady,
                OrderNotesShowToCustomer = settings.OrderNotesShowToCustomer,
                ShowOrderStatusInOrderDetails = settings.ShowOrderStatusInOrderDetails
            };
            return model;
        }

        #endregion

        #region Methods
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<PickupInStoreSettings>(storeScope);
            var model = await GetPickupInStoreSettingsModelAsync(settings);

            var messageTemplate = (await _pickupInStoreMessageService.GetActiveMessageTemplatesAsync(PickupInStoreDefaults.PICKUP_READY_EMAIL_TEMPLATE_SYSTEM_NAME, storeScope)).FirstOrDefault();
            if (messageTemplate != null)
            {
                model.TemplateId = messageTemplate.Id;
            }
            if (storeScope <= 0)
                return View(model);

            model.ShowOrderStatusInOrderDetails_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ShowOrderStatusInOrderDetails, storeScope);
            model.AddOrderNote_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AddOrderNote, storeScope);
            model.OrderNotesShowToCustomer_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.OrderNotesShowToCustomer, storeScope);
            model.NotifyCustomerIfReady_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.NotifyCustomerIfReady, storeScope);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(PickupInStoreSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<PickupInStoreSettings>(storeScope);

            settings.AddOrderNote = model.AddOrderNote;
            settings.NotifyCustomerIfReady = model.NotifyCustomerIfReady;
            settings.OrderNotesShowToCustomer = model.OrderNotesShowToCustomer;
            settings.ShowOrderStatusInOrderDetails = model.ShowOrderStatusInOrderDetails;

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AddOrderNote, model.AddOrderNote_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.NotifyCustomerIfReady, model.NotifyCustomerIfReady_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.OrderNotesShowToCustomer, model.OrderNotesShowToCustomer_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ShowOrderStatusInOrderDetails, model.ShowOrderStatusInOrderDetails_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return Redirect("Configure");
        }

        public virtual async Task<IActionResult> ExportToXlsx()
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            var storePickupPoints = await _storePickupPointService.GetAllStorePickupPointsAsync();
            var pickupPoints = storePickupPoints.Select(x =>
            {
                var address = _addressService.GetAddressByIdAsync(x.AddressId).Result;
                var stateName = string.Empty;
                var countryName = string.Empty;
                var hasAddress = address != null;
                if (hasAddress)
                {
                    if (address.StateProvinceId.HasValue)
                    {
                        stateName = _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId.Value).Result?.Name;
                    }
                    if (address.CountryId.HasValue)
                    {
                        countryName = _countryService.GetCountryByIdAsync(address.CountryId.Value).Result?.Name;
                    }
                }
                var storePickupPointModel = new StorePickupPointsExportImportModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Active = x.Active,
                    DisplayOrder = x.DisplayOrder,
                    TransitDays = x.TransitDays,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    PickupFee = x.PickupFee,
                    StoreId = x.StoreId,
                    OpeningHours = x.OpeningHours,
                    Address1 = hasAddress ? address.Address1 : string.Empty,
                    Address2 = hasAddress ? address.Address2 : string.Empty,
                    City = hasAddress ? address.City : string.Empty,
                    ZipPostalCode = hasAddress ? address.ZipPostalCode : string.Empty,
                    StateName = stateName,
                    CountryName = countryName
                };
                return storePickupPointModel;
            }).ToList();

            var bytes = await _pickupPointExportImportService.ExportToXlsxAsync(pickupPoints);

            return File(bytes, MimeTypes.TextXlsx, "Pickup_Points.xlsx");
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> ImportFromXlsx(IFormFile importexcelfile)
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            //a vendor cannot import pickup points
            if (await _workContext.GetCurrentVendorAsync() != null)
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    await _pickupPointExportImportService.ImportFromXlsxAsync(importexcelfile.OpenReadStream());
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                    return Redirect("Configure");
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.PickupInStoreAdvance.ImportPickupPoints.Success"));

                return Redirect("Configure");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                return Redirect("Configure");
            }
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> List(StorePickupPointSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _storePickupPointModelFactory.PrepareStorePickupPointListModelAsync(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var model = new StorePickupPointModel
            {
                Address =
                {
                    CountryEnabled = _addressSettings.CountryEnabled,
                    StateProvinceEnabled = _addressSettings.StateProvinceEnabled,
                    ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled,
                    CityEnabled = _addressSettings.CityEnabled,
                    CountyEnabled = _addressSettings.CountyEnabled
                }
            };

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

            model.AvailableStores.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.StoreScope.AllStores"), Value = "0" });
            foreach (var store in await _storeService.GetAllStoresAsync())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Create(StorePickupPointModel model)
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var address = new Address
            {
                Address1 = model.Address.Address1,
                City = model.Address.City,
                County = model.Address.County,
                CountryId = model.Address.CountryId,
                StateProvinceId = model.Address.StateProvinceId,
                ZipPostalCode = model.Address.ZipPostalCode,
                CreatedOnUtc = DateTime.UtcNow
            };
            await _addressService.InsertAddressAsync(address);

            var pickupPoint = new StorePickupPoint
            {
                Name = model.Name,
                Description = model.Description,
                Active = model.Active,
                AddressId = address.Id,
                OpeningHours = model.OpeningHours,
                PickupFee = model.PickupFee,
                DisplayOrder = model.DisplayOrder,
                StoreId = model.StoreId,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                TransitDays = model.TransitDays
            };
            await _storePickupPointService.InsertStorePickupPointAsync(pickupPoint);

            ViewBag.RefreshPage = true;

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var pickupPoint = await _storePickupPointService.GetStorePickupPointByIdAsync(id);
            if (pickupPoint == null)
                return RedirectToAction("Configure");

            var model = new StorePickupPointModel
            {
                Id = pickupPoint.Id,
                Name = pickupPoint.Name,
                Description = pickupPoint.Description,
                Active = pickupPoint.Active,
                OpeningHours = pickupPoint.OpeningHours,
                PickupFee = pickupPoint.PickupFee,
                DisplayOrder = pickupPoint.DisplayOrder,
                StoreId = pickupPoint.StoreId,
                Latitude = pickupPoint.Latitude,
                Longitude = pickupPoint.Longitude,
                TransitDays = pickupPoint.TransitDays
            };

            var address = await _addressService.GetAddressByIdAsync(pickupPoint.AddressId);
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
                    CountryEnabled = _addressSettings.CountryEnabled,
                    StateProvinceEnabled = _addressSettings.StateProvinceEnabled,
                    ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled,
                    CityEnabled = _addressSettings.CityEnabled,
                    CountyEnabled = _addressSettings.CountyEnabled
                };
            }

            model.Address.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Address.SelectCountry"), Value = "0" });
            foreach (var country in await _countryService.GetAllCountriesAsync(showHidden: true))
                model.Address.AvailableCountries.Add(new SelectListItem { Text = country.Name, Value = country.Id.ToString(), Selected = (address != null && country.Id == address.CountryId) });

            var states = !model.Address.CountryId.HasValue ? new List<StateProvince>()
                : await _stateProvinceService.GetStateProvincesByCountryIdAsync(model.Address.CountryId.Value, showHidden: true);
            if (states.Any())
            {
                model.Address.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Address.SelectState"), Value = "0" });
                foreach (var state in states)
                    model.Address.AvailableStates.Add(new SelectListItem { Text = state.Name, Value = state.Id.ToString(), Selected = (address != null && state.Id == address.StateProvinceId) });
            }
            else
                model.Address.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Address.Other"), Value = "0" });

            model.AvailableStores.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.StoreScope.AllStores"), Value = "0" });
            foreach (var store in await _storeService.GetAllStoresAsync())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString(), Selected = store.Id == model.StoreId });

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Edit(StorePickupPointModel model)
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Edit(model.Id);

            var pickupPoint = await _storePickupPointService.GetStorePickupPointByIdAsync(model.Id);
            if (pickupPoint == null)
                return RedirectToAction("Configure");

            var address = await _addressService.GetAddressByIdAsync(pickupPoint.AddressId) ?? new Address { CreatedOnUtc = DateTime.UtcNow };
            address.Address1 = model.Address.Address1;
            address.City = model.Address.City;
            address.County = model.Address.County;
            address.CountryId = model.Address.CountryId;
            address.StateProvinceId = model.Address.StateProvinceId;
            address.ZipPostalCode = model.Address.ZipPostalCode;
            if (address.Id > 0)
                await _addressService.UpdateAddressAsync(address);
            else
                await _addressService.InsertAddressAsync(address);

            pickupPoint.Name = model.Name;
            pickupPoint.Description = model.Description;
            pickupPoint.Active = model.Active;
            pickupPoint.AddressId = address.Id;
            pickupPoint.OpeningHours = model.OpeningHours;
            pickupPoint.PickupFee = model.PickupFee;
            pickupPoint.DisplayOrder = model.DisplayOrder;
            pickupPoint.StoreId = model.StoreId;
            pickupPoint.Latitude = model.Latitude;
            pickupPoint.Longitude = model.Longitude;
            pickupPoint.TransitDays = model.TransitDays;
            await _storePickupPointService.UpdateStorePickupPointAsync(pickupPoint);

            ViewBag.RefreshPage = true;

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(PickupInStorePermissionProvider.ManagePickupInStore))
                return AccessDeniedView();
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var pickupPoint = await _storePickupPointService.GetStorePickupPointByIdAsync(id);
            if (pickupPoint == null)
                return RedirectToAction("Configure");

            var address = await _addressService.GetAddressByIdAsync(pickupPoint.AddressId);
            if (address != null)
                await _addressService.DeleteAddressAsync(address);

            await _storePickupPointService.DeleteStorePickupPointAsync(pickupPoint);

            return new NullJsonResult();
        }

        #endregion
    }
}
