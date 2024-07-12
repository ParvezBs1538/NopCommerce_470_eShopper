using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartSliders.Domains;
using NopStation.Plugin.Widgets.SmartSliders.Helpers;
using NopStation.Plugin.Widgets.SmartSliders.Services;

namespace NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Factories;

public partial class SmartSliderModelFactory : ISmartSliderModelFactory
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
    private readonly ILocalizedModelFactory _localizedModelFactory;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ISmartSliderService _sliderService;
    private readonly IWidgetZoneModelFactory _widgetZoneModelFactory;
    private readonly IScheduleModelFactory _scheduleModelFactory;
    private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    private readonly ILanguageService _languageService;
    private readonly IConditionModelFactory _conditionModelFactory;
    private readonly IPictureService _pictureService;

    #endregion

    #region Ctor

    public SmartSliderModelFactory(IPictureService pictureService,
        IStoreContext storeContext,
        IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
        ILocalizedModelFactory localizedModelFactory,
        IBaseAdminModelFactory baseAdminModelFactory,
        ILocalizationService localizationService,
        ISettingService settingService,
        IDateTimeHelper dateTimeHelper,
        ISmartSliderService sliderService,
        IWidgetZoneModelFactory widgetZoneModelFactory,
        IScheduleModelFactory scheduleModelFactory,
        IAclSupportedModelFactory aclSupportedModelFactory,
        ILanguageService languageService,
        IConditionModelFactory conditionModelFactory)
    {
        _pictureService = pictureService;
        _storeContext = storeContext;
        _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        _localizedModelFactory = localizedModelFactory;
        _baseAdminModelFactory = baseAdminModelFactory;
        _localizationService = localizationService;
        _settingService = settingService;
        _dateTimeHelper = dateTimeHelper;
        _sliderService = sliderService;
        _widgetZoneModelFactory = widgetZoneModelFactory;
        _scheduleModelFactory = scheduleModelFactory;
        _aclSupportedModelFactory = aclSupportedModelFactory;
        _languageService = languageService;
        _conditionModelFactory = conditionModelFactory;
    }

    #endregion

    #region Utilities

    protected async Task PrepareWidgetZonesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        //prepare available widget zones
        var availableWidgetZoneItems = SmartSliderHelper.GetWidgetZoneSelectList();
        foreach (var widgetZoneItem in availableWidgetZoneItems)
        {
            items.Add(widgetZoneItem);
        }

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = ""
            });
    }

    protected async Task PrepareContentTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availableContentTypeItems = await ContentType.Picture.ToSelectListAsync(true);
        foreach (var typeItem in availableContentTypeItems)
        {
            items.Add(typeItem);
        }

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PreparePaginationTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availablePaginationTypeItems = await PaginationType.Bullets.ToSelectListAsync(true);
        foreach (var typeItem in availablePaginationTypeItems)
        {
            items.Add(typeItem);
        }

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PrepareBackgroundTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availableBackgroundTypeItems = await BackgroundType.Picture.ToSelectListAsync(false);
        foreach (var typeItem in availableBackgroundTypeItems)
        {
            items.Add(typeItem);
        }

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    protected async Task PrepareEffectTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var availableEffectTypeItems = await EffectType.Fade.ToSelectListAsync(false);
        foreach (var typeItem in availableEffectTypeItems)
        {
            items.Add(typeItem);
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
            Text = await _localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Sliders.List.SearchActive.Active"),
            Value = "1"
        });
        items.Add(new SelectListItem()
        {
            Text = await _localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Sliders.List.SearchActive.Inactive"),
            Value = "2"
        });

        if (withSpecialDefaultItem)
            items.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
    }

    public virtual Task<SmartSliderItemSearchModel> PrepareSliderItemSearchModelAsync(SmartSliderItemSearchModel searchModel, SmartSlider slider)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        if (slider == null)
            throw new ArgumentNullException(nameof(slider));

        searchModel.SliderId = slider.Id;
        searchModel.SetGridPageSize();

        return Task.FromResult(searchModel);
    }

    #endregion

    #region Methods

    #region Sliders

    public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
    {
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var sliderSettings = await _settingService.LoadSettingAsync<SmartSliderSettings>(storeId);

        var model = sliderSettings.ToSettingsModel<ConfigurationModel>();
        model.SupportedVideoExtensions = string.Join(",", sliderSettings.SupportedVideoExtensions);

        model.ActiveStoreScopeConfiguration = storeId;

        if (storeId <= 0)
            return model;

        model.EnableSlider_OverrideForStore = await _settingService.SettingExistsAsync(sliderSettings, x => x.EnableSlider, storeId);
        model.EnableAjaxLoad_OverrideForStore = await _settingService.SettingExistsAsync(sliderSettings, x => x.EnableAjaxLoad, storeId);

        return model;
    }

    public virtual async Task<SmartSliderSearchModel> PrepareSliderSearchModelAsync(SmartSliderSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        await PrepareActiveOptionsAsync(searchModel.AvailableActiveOptions);
        await PrepareWidgetZonesAsync(searchModel.AvailableWidgetZones);

        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<SmartSliderListModel> PrepareSliderListModelAsync(SmartSliderSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get parameters to filter comments
        var overridePublished = searchModel.SearchActiveId == 0 ? null : (bool?)(searchModel.SearchActiveId == 1);

        //get sliders
        var sliders = await _sliderService.GetAllSlidersAsync(
            showHidden: true,
            overridePublished: overridePublished,
            keywords: searchModel.SearchKeyword,
            storeId: searchModel.SearchStoreId,
            widgetZone: searchModel.SearchWidgetZone,
            pageSize: searchModel.PageSize,
            pageIndex: searchModel.Page - 1);

        //prepare list model
        var model = await new SmartSliderListModel().PrepareToGridAsync(searchModel, sliders, () =>
        {
            return sliders.SelectAwait(async slider =>
            {
                return await PrepareSliderModelAsync(null, slider, true);
            });
        });

        return model;
    }

    public async Task<SmartSliderModel> PrepareSliderModelAsync(SmartSliderModel model, SmartSlider slider,
        bool excludeProperties = false)
    {
        if (slider != null)
        {
            if (model == null)
            {
                model = slider.ToModel<SmartSliderModel>();
            }

            await PrepareSliderItemSearchModelAsync(model.SmartSliderItemSearchModel, slider);

            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(slider.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(slider.UpdatedOnUtc, DateTimeKind.Utc);
        }

        if (!excludeProperties)
        {
            await PrepareBackgroundTypesAsync(model.AvaliableBackgroundTypes, false);
            await PrepareEffectTypesAsync(model.AvailableEffectTypes, false);
            await PreparePaginationTypesAsync(model.AvailablePaginationTypes, false);

            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, slider, excludeProperties);

            //prepare model schedule mappings
            await _scheduleModelFactory.PrepareScheduleMappingModelAsync(model, slider);

            //prepare model customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, slider, excludeProperties);

            //prepare customer condition mapping model
            await _conditionModelFactory.PrepareCustomerConditionMappingSearchModelAsync(model, slider);

            //prepare product condition mapping model
            await _conditionModelFactory.PrepareProductConditionMappingSearchModelAsync(model, slider);

            //prepare model widget zone mappings
            await _widgetZoneModelFactory.PrepareWidgetZoneMappingSearchModelAsync(model, slider);
            await _widgetZoneModelFactory.PrepareAddWidgetZoneMappingModelAsync(model, slider, true, SmartSliderHelper.GetCustomWidgetZones());
        }

        if (slider == null)
        {
            model.Active = true;
            model.EnableAutoPlay = true;
            model.AutoPlayTimeout = 5000;
            model.AutoPlayHoverPause = true;
            model.EnableLoop = true;
            model.EnableLazyLoad = true;
            model.EnableNavigation = true;
        }

        return model;
    }

    #endregion

    #region Slider items

    public async Task<SmartSliderItemListModel> PrepareSliderItemListModelAsync(SmartSliderItemSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get sliders
        var sliderItems = (await _sliderService.GetSliderItemsBySliderIdAsync(searchModel.SliderId)).ToPagedList(searchModel);

        //prepare list model
        var model = await new SmartSliderItemListModel().PrepareToGridAsync(searchModel, sliderItems, () =>
        {
            return sliderItems.SelectAwait(async sliderItem =>
            {
                var slider = await _sliderService.GetSliderByIdAsync(sliderItem.SliderId);
                return await PrepareSliderItemModelAsync(null, sliderItem, slider);
            });
        });

        return model;
    }

    public async Task<SmartSliderItemModel> PrepareSliderItemModelAsync(SmartSliderItemModel model, SmartSliderItem sliderItem,
        SmartSlider slider, bool excludeProperties = false)
    {
        Func<SliderItemLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (sliderItem != null)
        {
            if (model == null)
            {
                model = sliderItem.ToModel<SmartSliderItemModel>();
                if (sliderItem.ContentTypeId == 1)
                {
                    var picture = await _pictureService.GetPictureByIdAsync(sliderItem.MobilePictureId);
                    var (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture);
                    model.Content = $"<img src=\"{imageUrl}\" width=\"100%\" height=\"100\" title=\"\" alt=\"\">";
                }
                else if (sliderItem.ContentTypeId == 3)
                {
                    model.Content = $"<iframe class=\"thumb-item\" src=\"{sliderItem.EmbeddedLink}\" width=\"100%\" height=\"100\" frameborder=\"0\" allow=\"accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture\" allowfullscreen></iframe>";

                }
                else if (model.ContentTypeId == 4)
                {
                    model.Content = sliderItem.Text;
                }

                model.ContentTypeStr = await _localizationService.GetLocalizedEnumAsync(sliderItem.ContentType);

                if (await _languageService.GetLanguageByIdAsync(sliderItem.LanguageId) is Language language)
                    model.LanguageName = language.Name;
                else
                    model.LanguageName = await _localizationService.GetResourceAsync("Admin.Common.All");
            }

            if (!excludeProperties)
            {
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Title = await _localizationService.GetLocalizedAsync(sliderItem, entity => entity.Title);
                    locale.Description = await _localizationService.GetLocalizedAsync(sliderItem, entity => entity.Description);
                    locale.Text = await _localizationService.GetLocalizedAsync(sliderItem, entity => entity.Text);
                    locale.ButtonText = await _localizationService.GetLocalizedAsync(sliderItem, entity => entity.ButtonText);
                    locale.RedirectUrl = await _localizationService.GetLocalizedAsync(sliderItem, entity => entity.RedirectUrl);
                };
            }
        }

        if (!excludeProperties)
        {
            await PrepareContentTypesAsync(model.AvailableContentTypes, false);
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            await _baseAdminModelFactory.PrepareLanguagesAsync(model.AvailableLanguages);
        }

        if (sliderItem == null)
        {
            model.SliderId = slider.Id;
        }

        return model;
    }

    #endregion

    #endregion
}
