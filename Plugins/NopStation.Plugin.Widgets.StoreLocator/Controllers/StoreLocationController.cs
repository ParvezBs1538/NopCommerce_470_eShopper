using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Widgets.StoreLocator.Services;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Core.Domain.Common;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.StoreLocator.Models;
using Nop.Core.Domain.Directory;
using NopStation.Plugin.Widgets.StoreLocator.Helpers;
using NopStation.Plugin.Misc.Core.Controllers;
using System;
using Nop.Services.Seo;
using Nop.Web.Models.Media;
using System.Linq;
using Nop.Core.Domain.Media;
using NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Models;
using Nop.Core;

namespace NopStation.Plugin.Widgets.StoreLocator.Controllers
{
    public class StoreLocationController : NopStationPublicController
    {
        #region Fields

        private readonly IStoreLocationService _storeLocationService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IPictureService _pictureService;
        private readonly StoreLocatorSettings _storeLocatorSettings;
        private readonly IAddressService _addressService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly MediaSettings _mediaSettings;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public StoreLocationController(IStoreLocationService storeLocationService,
            ICountryService countryService,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            IPictureService pictureService,
            StoreLocatorSettings storeLocatorSettings,
            IAddressService addressService,
            IUrlRecordService urlRecordService,
            MediaSettings mediaSettings,
            IWorkContext workContext)
        {
            _storeLocationService = storeLocationService;
            _countryService = countryService;
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
            _pictureService = pictureService;
            _storeLocatorSettings = storeLocatorSettings;
            _addressService = addressService;
            _urlRecordService = urlRecordService;
            _mediaSettings = mediaSettings;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        private async Task<string> GetFormattedAddressAsync(Address address, Country country, StateProvince stateProvince)
        {
            if (address == null)
                return null;

            var addressLine = "";
            addressLine += address.FirstName;
            addressLine += " " + address.LastName;
            addressLine += ", " + address.Address1;
            addressLine += ", " + address.City;
            addressLine += ", " + address.County;

            if (stateProvince != null)
                addressLine += ", " + await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name);
            addressLine += " " + address.ZipPostalCode;

            if (country != null)
                addressLine += ", " + await _localizationService.GetLocalizedAsync(country, x => x.Name);

            return addressLine;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Stores(int? countryId, int? sid, LocationsModel command)
        {
            if (!_storeLocatorSettings.EnablePlugin)
                return RedirectToRoute("Homepage");

            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            var pageSize = _storeLocatorSettings.PublicDispalyPageSize < 1 ? int.MaxValue : _storeLocatorSettings.PublicDispalyPageSize;

            var stores = (await _storeLocationService.SearchStoreLocationsAsync(
                stateId: sid ?? 0,
                countryId: countryId ?? 0,
                active: true,
                pageIndex: command.PageIndex,
                pageSize: pageSize));

            var model = new StoreLocatorModel();

            foreach (var store in stores)
            {
                var address = await _addressService.GetAddressByIdAsync(store.AddressId);
                if (address == null)
                    continue;

                var country = await _countryService.GetCountryByAddressAsync(address);
                var stateProvince = await _stateProvinceService.GetStateProvinceByAddressAsync(address);

                model.LocationsModel.Locations.Add(new LocationsModel.LocationModel()
                {
                    Id = store.Id,
                    ShortDescription = await _localizationService.GetLocalizedAsync(store, x => x.ShortDescription, languageId),
                    Latitude = store.Latitude,
                    Longitude = store.Longitude,
                    Name = await _localizationService.GetLocalizedAsync(store, x => x.Name, languageId),
                    OpeningHours = string.IsNullOrWhiteSpace(await _localizationService.GetLocalizedAsync(store, x => x.OpeningHours, languageId)) ? "" : (await _localizationService.GetLocalizedAsync(store, x => x.OpeningHours, languageId)).Replace(Environment.NewLine, "<br>"),
                    ImageUrl = (await _pictureService.GetPictureUrlAsync(await _storeLocationService.GetDefaultPictureByStoreLocationIdAsync(store.Id), _storeLocatorSettings.PictureSize)).Url,
                    Country = country == null ? "" : await _localizationService.GetLocalizedAsync(country, x => x.Name),
                    GoogleMapLocation = store.GoogleMapLocation,
                    State = stateProvince == null ? "" : await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name),
                    FullAddress = store.FullAddress,
                    FormattedAddress = await GetFormattedAddressAsync(address, country, stateProvince),
                    PhoneNumber = address.PhoneNumber,
                    Email = address.Email,
                    SeName = await _urlRecordService.GetSeNameAsync(store),
                    ZipPostalCode = address.ZipPostalCode
                });
            }
            model.GoogleMapApiKey = _storeLocatorSettings?.GoogleMapApiKey;
            model.LocationsModel.LoadPagedList(stores);

            return View(model);
        }

        public async Task<IActionResult> Store(int storelocationid)
        {
            if (!_storeLocatorSettings.EnablePlugin)
                return RedirectToRoute("Homepage");

            var store = await _storeLocationService.GetStoreLocationByIdAsync(storelocationid);
            if (store == null)
                RedirectToAction("HomePage");

            var address = await _addressService.GetAddressByIdAsync(store.AddressId);
            if (address == null)
                RedirectToAction("HomePage");
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            var country = await _countryService.GetCountryByAddressAsync(address);
            var stateProvince = await _stateProvinceService.GetStateProvinceByAddressAsync(address);

            var model = new StoreLocationDetailsModel()
            {
                Id = store.Id,
                FullDescription = await _localizationService.GetLocalizedAsync(store, x => x.FullDescription, languageId),
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                Name = await _localizationService.GetLocalizedAsync(store, x => x.Name, languageId),
                OpeningHours = await _localizationService.GetLocalizedAsync(store, x => x.OpeningHours, languageId),
                Country = country == null ? "" : await _localizationService.GetLocalizedAsync(country, x => x.Name),
                GoogleMapLocation = store.GoogleMapLocation,
                State = stateProvince == null ? "" : await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name),
                FullAddress = store.FullAddress,
                FormattedAddress = await GetFormattedAddressAsync(address, country, stateProvince),
                PhoneNumber = address.PhoneNumber,
                Email = address.Email,
                SeName = await _urlRecordService.GetSeNameAsync(store),
                City = address.City,
                ZipPostalCode = address.ZipPostalCode
            };

            model.OpeningHours = string.IsNullOrWhiteSpace(model.OpeningHours) ? "" : model.OpeningHours.Replace(Environment.NewLine, "<br>");
            var storeName = await _localizationService.GetLocalizedAsync(store, x => x.Name, languageId);
            var pictures = await _storeLocationService.GetPicturesByStoreLocationIdAsync(storelocationid);
            string fullSizeImageUrl, imageUrl, thumbImageUrl;
            if(pictures.Count < 1)
            {
                Picture picture = new Picture();
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(null, _storeLocatorSettings.PictureSize);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);
                var pictureModel = new PictureModel
                {
                    ImageUrl = imageUrl,
                    ThumbImageUrl = thumbImageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    Title = string.Format(await _localizationService.GetResourceAsync("NopStation.StoreLocator.ImageLinkTitleFormat.Details"), storeName),
                    AlternateText = string.Format(await _localizationService.GetResourceAsync("NopStation.StoreLocator.ImageAlternateTextFormat.Details"), storeName),
                };
                model.Pictures.Add(pictureModel);
            }
            for (var i = 0; i < pictures.Count; i++)
            {
                var picture = pictures[i];

                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _storeLocatorSettings.PictureSize);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                var pictureModel = new PictureModel
                {
                    ImageUrl = imageUrl,
                    ThumbImageUrl = thumbImageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    Title = string.Format(await _localizationService.GetResourceAsync("NopStation.StoreLocator.ImageLinkTitleFormat.Details"), storeName),
                    AlternateText = string.Format(await _localizationService.GetResourceAsync("NopStation.StoreLocator.ImageAlternateTextFormat.Details"), storeName),
                };
                //"title" attribute
                pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                    picture.TitleAttribute :
                    string.Format(await _localizationService.GetResourceAsync("NopStation.StoreLocator.ImageLinkTitleFormat.Details"), storeName);
                //"alt" attribute
                pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                    picture.AltAttribute :
                    string.Format(await _localizationService.GetResourceAsync("NopStation.StoreLocator.ImageAlternateTextFormat.Details"), storeName);

                model.Pictures.Add(pictureModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveGeoLocation(GeoLocationModel model)
        {
            await StoreLocatorHelper.SetGeoLocationAsync(model);
            return Json(null);
        }

        #endregion
    }
}
