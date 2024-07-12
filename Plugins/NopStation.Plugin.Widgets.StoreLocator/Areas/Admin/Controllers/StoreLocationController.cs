using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Models;
using NopStation.Plugin.Widgets.StoreLocator.Domain;
using NopStation.Plugin.Widgets.StoreLocator.Services;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Controllers
{
    public class StoreLocationController : BaseAdminController
    {
        #region Fields

        private readonly IStoreLocationService _storeLocationService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IStoreLocatorModelFactorey _storeLocatorModelFactorey;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IAddressService _addressService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public StoreLocationController(IStoreLocationService storeLocationService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            INotificationService notificationService,
            IStoreLocatorModelFactorey storeLocatorModelFactorey,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IAddressService addressService,
            ILocalizedEntityService localizedEntityService,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
            IWorkContext workContext)
        {
            _storeLocationService = storeLocationService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _storeLocatorModelFactorey = storeLocatorModelFactorey;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _addressService = addressService;
            _localizedEntityService = localizedEntityService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task UpdateLocalesAsync(StoreLocation storeLocation, StoreLocationModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(storeLocation,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(storeLocation,
                    x => x.ShortDescription,
                    localized.ShortDescription,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(storeLocation,
                    x => x.FullDescription,
                    localized.FullDescription,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(storeLocation,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(storeLocation,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(storeLocation,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(storeLocation,
                    x => x.OpeningHours,
                    localized.OpeningHours,
                    localized.LanguageId);

                //search engine name
                var seName = await _urlRecordService.ValidateSeNameAsync(storeLocation, localized.SeName, localized.Name, false);
                await _urlRecordService.SaveSlugAsync(storeLocation, seName, localized.LanguageId);
            }
        }

        protected virtual async Task SaveStoreMappingsAsync(StoreLocation storeLocation, StoreLocationModel model)
        {
            storeLocation.LimitedToStores = model.SelectedStoreIds.Any();
            await _storeLocationService.UpdateStoreLocationAsync(storeLocation);

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(storeLocation);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        await _storeMappingService.InsertStoreMappingAsync(storeLocation, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
                }
            }
        }

        #endregion

        #region Methods

        #region Store location list / create / edit / delete

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocations))
                return AccessDeniedView();

            var searchModel = await _storeLocatorModelFactorey.PrepareStoreLocationSearchModelAsync(new StoreLocationSearchModel());

            return View(searchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(StoreLocationSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocations))
                return await AccessDeniedDataTablesJson();

            var model = await _storeLocatorModelFactorey.PrepareStoreLocationListModelAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocations))
                return AccessDeniedView();

            var model = await _storeLocatorModelFactorey.PrepareStoreLocationModelAsync(new StoreLocationModel(), null);
            model.Active = true;

            return View(model);
        }

        [CheckAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(StoreLocationModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocations))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity<Address>();

                address.CreatedOnUtc = DateTime.UtcNow;

                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                await _addressService.InsertAddressAsync(address);

                var storeLocation = model.ToEntity<StoreLocation>();
                storeLocation.CreatedOnUtc = DateTime.UtcNow;
                storeLocation.UpdatedOnUtc = DateTime.UtcNow;
                storeLocation.AddressId = address.Id;

                await _storeLocationService.InsertStoreLocationAsync(storeLocation);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(storeLocation, model.SeName, storeLocation.Name, true);
                await _urlRecordService.SaveSlugAsync(storeLocation, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(storeLocation, model);

                //stores
                await SaveStoreMappingsAsync(storeLocation, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.StoreLocator.StoreLocations.Created"));

                return continueEditing ?
                    RedirectToAction("Edit", new { id = storeLocation.Id }) :
                    RedirectToAction("List");
            }

            model = await _storeLocatorModelFactorey.PrepareStoreLocationModelAsync(model, null);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocations))
                return AccessDeniedView();

            var storeLocation = await _storeLocationService.GetStoreLocationByIdAsync(id);
            if (storeLocation == null)
                return RedirectToAction("List");

            var model = await _storeLocatorModelFactorey.PrepareStoreLocationModelAsync(null, storeLocation);

            return View(model);
        }

        [CheckAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(StoreLocationModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocations))
                return AccessDeniedView();

            var storeLocation = await _storeLocationService.GetStoreLocationByIdAsync(model.Id);
            if (storeLocation == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var address = await _addressService.GetAddressByIdAsync(storeLocation.AddressId);
                address = model.Address.ToEntity(address);

                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                await _addressService.UpdateAddressAsync(address);

                storeLocation = model.ToEntity(storeLocation);
                storeLocation.UpdatedOnUtc = DateTime.UtcNow;
                await _storeLocationService.UpdateStoreLocationAsync(storeLocation);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(storeLocation, model.SeName, storeLocation.Name, true);
                await _urlRecordService.SaveSlugAsync(storeLocation, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(storeLocation, model);

                //stores
                await SaveStoreMappingsAsync(storeLocation, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.StoreLocator.StoreLocations.Updated"));

                return continueEditing ?
                    RedirectToAction("Edit", new { id = storeLocation.Id }) :
                    RedirectToAction("List");
            }

            model = await _storeLocatorModelFactorey.PrepareStoreLocationModelAsync(model, storeLocation);

            return View(model);
        }

        [CheckAccess, HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocations))
                return AccessDeniedView();

            var storeLocation = await _storeLocationService.GetStoreLocationByIdAsync(id);
            if (storeLocation == null)
                return RedirectToAction("List");

            await _storeLocationService.DeleteStoreLocationAsync(storeLocation);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.StoreLocator.StoreLocations.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #region Store location pictures

        public virtual async Task<IActionResult> StoreLocationPictureAdd(int pictureId, int displayOrder,
            string overrideAltAttribute, string overrideTitleAttribute, int storeLocationId)
        {
            if (!await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocations))
                return AccessDeniedView();

            if (pictureId == 0)
                throw new ArgumentException();

            //try to get a storeLocation with the specified id
            var storeLocation = await _storeLocationService.GetStoreLocationByIdAsync(storeLocationId)
                ?? throw new ArgumentException("No store location found with the specified id");

            if ((await _storeLocationService.GetStoreLocationPicturesByStoreLocationIdAsync(storeLocationId)).Any(p => p.PictureId == pictureId))
                return Json(new { Result = false });

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(pictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType,
                picture.SeoFilename,
                overrideAltAttribute,
                overrideTitleAttribute);

            await _pictureService.SetSeoFilenameAsync(pictureId, await _pictureService.GetPictureSeNameAsync(storeLocation.Name));

            await _storeLocationService.InsertStoreLocationPictureAsync(new StoreLocationPicture
            {
                PictureId = pictureId,
                StoreLocationId = storeLocationId,
                DisplayOrder = displayOrder
            });

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> StoreLocationPictureList(StoreLocationPictureSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocations))
                return await AccessDeniedDataTablesJson();

            //try to get a storeLocation with the specified id
            var storeLocation = await _storeLocationService.GetStoreLocationByIdAsync(searchModel.StoreLocationId)
                ?? throw new ArgumentException("No store location found with the specified id");

            //prepare model
            var model = await _storeLocatorModelFactorey.PrepareStoreLocationPictureListModelAsync(searchModel, storeLocation);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> StoreLocationPictureUpdate(StoreLocationPictureModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocations))
                return AccessDeniedView();

            //try to get a storeLocation picture with the specified id
            var storeLocationPicture = await _storeLocationService.GetStoreLocationPictureByIdAsync(model.Id)
                ?? throw new ArgumentException("No store location picture found with the specified id");

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(storeLocationPicture.PictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);

            storeLocationPicture.DisplayOrder = model.DisplayOrder;
            await _storeLocationService.UpdateStoreLocationPictureAsync(storeLocationPicture);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> StoreLocationPictureDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StoreLocatorPermissionProvider.ManageStoreLocations))
                return AccessDeniedView();

            //try to get a storeLocation picture with the specified id
            var storeLocationPicture = await _storeLocationService.GetStoreLocationPictureByIdAsync(id)
                ?? throw new ArgumentException("No store location picture found with the specified id");

            var pictureId = storeLocationPicture.PictureId;
            await _storeLocationService.DeleteStoreLocationPictureAsync(storeLocationPicture);

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(pictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            await _pictureService.DeletePictureAsync(picture);

            return new NullJsonResult();
        }

        #endregion

        #endregion
    }
}
