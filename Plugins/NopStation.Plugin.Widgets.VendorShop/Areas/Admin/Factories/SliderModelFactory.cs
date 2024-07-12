using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.SliderVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Domains.SliderVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Helpers;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Factories
{
    public partial class SliderModelFactory : ISliderModelFactory
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ISliderService _sliderService;

        #endregion

        #region Ctor

        public SliderModelFactory(IStoreContext storeContext,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            ILocalizedModelFactory localizedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ILocalizationService localizationService,
            IPictureService pictureService,
            ISettingService settingService,
            IDateTimeHelper dateTimeHelper,
            ISliderService sliderService)
        {
            _storeContext = storeContext;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _localizedModelFactory = localizedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _settingService = settingService;
            _dateTimeHelper = dateTimeHelper;
            _sliderService = sliderService;
        }

        #endregion

        #region Utilities

        protected async Task PrepareCustomWidgetZonesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //prepare available activity log types
            var availableWidgetZones = WidgetZonelHelper.GetCustomWidgetZoneSelectList();
            foreach (var zone in availableWidgetZones)
            {
                items.Add(zone);
            }

            if (withSpecialDefaultItem)
                items.Insert(0, new SelectListItem()
                {
                    Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                    Value = "0"
                });
        }

        protected async Task PrepareActiveOptionsAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            items.Add(new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.List.SearchActive.Active"),
                Value = "1"
            });
            items.Add(new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.List.SearchActive.Inactive"),
                Value = "2"
            });

            if (withSpecialDefaultItem)
                items.Insert(0, new SelectListItem()
                {
                    Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                    Value = "0"
                });
        }

        #endregion

        #region Methods

        public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
        {
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var vendorShopSettings = await _settingService.LoadSettingAsync<VendorShopSettings>(storeId);

            var model = vendorShopSettings.ToSettingsModel<ConfigurationModel>();

            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return model;

            model.EnableSlider_OverrideForStore = await _settingService.SettingExistsAsync(vendorShopSettings, x => x.EnableSlider, storeId);

            return model;
        }

        public virtual async Task<SliderSearchModel> PrepareSliderSearchModelAsync(SliderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            await PrepareCustomWidgetZonesAsync(searchModel.AvailableWidgetZones, true);
            await PrepareActiveOptionsAsync(searchModel.AvailableActiveOptions, true);

            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<SliderListModel> PrepareSliderListModelAsync(SliderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var widgetZoneIds = searchModel.SearchWidgetZones?.Contains(0) ?? true ? null : searchModel.SearchWidgetZones.ToList();

            bool? active = null;
            if (searchModel.SearchActiveId == 1)
                active = true;
            else if (searchModel.SearchActiveId == 2)
                active = false;

            //get carousels
            var sliders = await _sliderService.GetAllSlidersAsync(widgetZoneIds, searchModel.SearchStoreId, searchModel.VendorId,
                active, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = await new SliderListModel().PrepareToGridAsync(searchModel, sliders, () =>
            {
                return sliders.SelectAwait(async slider =>
                {
                    return await PrepareSliderModelAsync(null, slider, true);
                });
            });

            return model;
        }

        public async Task<SliderModel> PrepareSliderModelAsync(SliderModel model, Slider slider,
            bool excludeProperties = false)
        {
            Func<SliderLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (slider != null)
            {
                if (model == null)
                {
                    model = slider.ToModel<SliderModel>();
                    model.WidgetZoneStr = WidgetZonelHelper.GetCustomWidgetZone(slider.WidgetZoneId);
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(slider.CreatedOnUtc, DateTimeKind.Utc);
                    model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(slider.UpdatedOnUtc, DateTimeKind.Utc);

                    if (!excludeProperties)
                    {
                        localizedModelConfiguration = async (locale, languageId) =>
                        {
                            locale.Name = await _localizationService.GetLocalizedAsync(slider, entity => entity.Name, languageId, false, false);
                        };
                    }
                }
            }

            if (!excludeProperties)
            {
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
                model.AvailableWidgetZones = WidgetZonelHelper.GetCustomWidgetZoneSelectList();
                model.AvailableAnimationTypes = WidgetZonelHelper.GetSliderAnimationTypesSelectList();

                await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, slider, excludeProperties);
            }

            return model;
        }

        public async Task<SliderItemListModel> PrepareSliderItemListModelAsync(SliderItemSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get sliders
            var sliderItems = await _sliderService.GetSliderItemsBySliderIdAsync(searchModel.SliderId, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = await new SliderItemListModel().PrepareToGridAsync(searchModel, sliderItems, () =>
            {
                return sliderItems.SelectAwait(async sliderItem =>
                {
                    var slider = await _sliderService.GetSliderByIdAsync(sliderItem.SliderId);
                    return await PrepareSliderItemModelAsync(null, slider, sliderItem);
                });
            });

            return model;
        }

        public async Task<SliderItemModel> PrepareSliderItemModelAsync(SliderItemModel model, Slider slider,
            SliderItem sliderItem, bool excludeProperties = false)
        {
            Func<SliderItemLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (sliderItem != null)
            {
                if (model == null)
                {
                    model = sliderItem.ToModel<SliderItemModel>();
                    model.PictureUrl = await _pictureService.GetPictureUrlAsync(sliderItem.PictureId, 200);
                    model.FullPictureUrl = await _pictureService.GetPictureUrlAsync(sliderItem.PictureId);
                    model.MobilePictureUrl = await _pictureService.GetPictureUrlAsync(sliderItem.MobilePictureId, 200);
                    model.MobileFullPictureUrl = await _pictureService.GetPictureUrlAsync(sliderItem.MobilePictureId);
                    model.SliderItemTitle = sliderItem.Title;
                }

                if (!excludeProperties)
                {
                    localizedModelConfiguration = async (locale, languageId) =>
                    {
                        locale.SliderItemTitle = await _localizationService.GetLocalizedAsync(sliderItem, entity => entity.Title, languageId);
                        locale.ShortDescription = await _localizationService.GetLocalizedAsync(sliderItem, entity => entity.ShortDescription, languageId);
                        locale.ImageAltText = await _localizationService.GetLocalizedAsync(sliderItem, entity => entity.ImageAltText, languageId);
                        locale.Link = await _localizationService.GetLocalizedAsync(sliderItem, entity => entity.Link, languageId);
                        locale.ShopNowLink = await _localizationService.GetLocalizedAsync(sliderItem, entity => entity.ShopNowLink, languageId);
                    };
                }
            }

            if (!excludeProperties)
            {
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
            }

            model.SliderId = slider.Id;

            return model;
        }

        #endregion
    }
}
