using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Models;
using NopStation.Plugin.Widgets.StoreLocator.Domain;
using NopStation.Plugin.Widgets.StoreLocator.Services;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Factories
{
    public class StoreLocatorModelFactorey : IStoreLocatorModelFactorey
    {
        #region Fields

        private readonly IStoreLocationService _storeLocationService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IPictureService _pictureService;
        private readonly IAddressService _addressService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly CatalogSettings _catalogSettings;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public StoreLocatorModelFactorey(IStoreLocationService storeLocationService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IPictureService pictureService,
            IAddressService addressService,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            CatalogSettings catalogSettings,
            IAddressModelFactory addressModelFactory,
            IUrlRecordService urlRecordService)
        {
            _storeLocationService = storeLocationService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _pictureService = pictureService;
            _addressService = addressService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _catalogSettings = catalogSettings;
            _addressModelFactory = addressModelFactory;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Methods

        protected Task<StoreLocationPictureSearchModel> PrepareStoreLocationPictureSearchModelAsync(StoreLocationPictureSearchModel searchModel, StoreLocation storeLocation)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (storeLocation == null)
                throw new ArgumentNullException(nameof(storeLocation));

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.StoreLocationId = storeLocation.Id;

            return Task.FromResult(searchModel);
        }

        public async Task<StoreLocationSearchModel> PrepareStoreLocationSearchModelAsync(StoreLocationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task<StoreLocationListModel> PrepareStoreLocationListModelAsync(StoreLocationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var storeLocations = await _storeLocationService.SearchStoreLocationsAsync(
                storeName: searchModel.SearchStoreName,
                stateId: searchModel.SearchStoreId,
                pageIndex: searchModel.Page - 1, 
                pageSize: searchModel.PageSize);

            var model = await new StoreLocationListModel().PrepareToGridAsync(searchModel, storeLocations, () =>
            {
                //fill in model values from the entity
                return storeLocations.SelectAwait(async storeLocation =>
                {
                    return await PrepareStoreLocationModelAsync(null, storeLocation, true);
                });
            });

            return model;
        }

        public async Task<StoreLocationModel> PrepareStoreLocationModelAsync(StoreLocationModel model, StoreLocation storeLocation, bool excludeProperties = false)
        {
            Func<StoreLocationLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (storeLocation != null)
            {
                if (model == null)
                {
                    model = storeLocation.ToModel<StoreLocationModel>();
                    var picture = await _storeLocationService.GetDefaultPictureByStoreLocationIdAsync(storeLocation.Id);
                    model.PictureUrl = (await _pictureService.GetPictureUrlAsync(picture, 100)).Url;
                    model.SeName = await _urlRecordService.GetSeNameAsync(storeLocation, 0, true, false);

                    //prepare localized models
                    if (!excludeProperties)
                    { 
                        //define localized model configuration action
                        localizedModelConfiguration = async (locale, languageId) =>
                        {
                            locale.Name = await _localizationService.GetLocalizedAsync(storeLocation, entity => entity.Name, languageId, false, false);
                            locale.ShortDescription = await _localizationService.GetLocalizedAsync(storeLocation, entity => entity.ShortDescription, languageId, false, false);
                            locale.FullDescription = await _localizationService.GetLocalizedAsync(storeLocation, entity => entity.FullDescription, languageId, false, false);
                            locale.MetaKeywords = await _localizationService.GetLocalizedAsync(storeLocation, entity => entity.MetaKeywords, languageId, false, false);
                            locale.MetaDescription = await _localizationService.GetLocalizedAsync(storeLocation, entity => entity.MetaDescription, languageId, false, false);
                            locale.MetaTitle = await _localizationService.GetLocalizedAsync(storeLocation, entity => entity.MetaTitle, languageId, false, false);
                            locale.OpeningHours = await _localizationService.GetLocalizedAsync(storeLocation, entity => entity.OpeningHours, languageId, false, false);
                            locale.SeName = await _urlRecordService.GetSeNameAsync(storeLocation, languageId, false, false);
                        };

                        model.StoreLocationPictureSearchModel = await PrepareStoreLocationPictureSearchModelAsync(new StoreLocationPictureSearchModel(), storeLocation);
                    }
                }
            }

            //prepare address model
            var address = await _addressService.GetAddressByIdAsync(storeLocation?.AddressId ?? 0);
            if (!excludeProperties && address != null)
            {
                model.Address = address.ToModel(model.Address);
                model.Address.PhoneRequired = true;
                model.Address.StreetAddressRequired = true;
            }
            await _addressModelFactory.PrepareAddressModelAsync(model.Address, address);

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            //prepare available stores
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, storeLocation, excludeProperties);

            return model;
        }

        public virtual async Task<StoreLocationPictureListModel> PrepareStoreLocationPictureListModelAsync(StoreLocationPictureSearchModel searchModel, StoreLocation storeLocation)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (storeLocation == null)
                throw new ArgumentNullException(nameof(storeLocation));

            //get product pictures
            var storePictures = (await _storeLocationService.GetStoreLocationPicturesAsync(storeLocation.Id)).ToPagedList(searchModel);

            //prepare grid model
            var model = await new StoreLocationPictureListModel().PrepareToGridAsync(searchModel, storePictures, () =>
            {
                return storePictures.SelectAwait(async productPicture =>
                {
                    //fill in model values from the entity
                    var productPictureModel = productPicture.ToModel<StoreLocationPictureModel>();

                    //fill in additional values (not existing in the entity)
                    var picture = (await _pictureService.GetPictureByIdAsync(productPicture.PictureId))
                        ?? throw new Exception("Picture cannot be loaded");

                    productPictureModel.PictureUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url;

                    productPictureModel.OverrideAltAttribute = picture.AltAttribute;
                    productPictureModel.OverrideTitleAttribute = picture.TitleAttribute;

                    return productPictureModel;
                });
            });

            return model;
        }

        #endregion
    }
}
