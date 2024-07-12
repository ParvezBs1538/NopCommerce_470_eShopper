using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.Announcement.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Announcement.Domains;
using NopStation.Plugin.Widgets.Announcement.Services;

namespace NopStation.Plugin.Widgets.Announcement.Areas.Admin.Factories;

public class AnnouncementItemModelFactory : IAnnouncementItemModelFactory
{
    #region Fields

    private readonly IAnnouncementItemService _announcementItemService;
    private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ILocalizedModelFactory _localizedModelFactory;
    private readonly ILocalizationService _localizationService;
    private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    private readonly IScheduleModelFactory _scheduleModelFactory;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly IConditionModelFactory _conditionModelFactory;

    #endregion

    #region Ctor

    public AnnouncementItemModelFactory(IAnnouncementItemService announcementItemService,
        IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
        IDateTimeHelper dateTimeHelper,
        ILocalizedModelFactory localizedModelFactory,
        ILocalizationService localizationService,
        IAclSupportedModelFactory aclSupportedModelFactory,
        IScheduleModelFactory scheduleModelFactory,
        IStoreContext storeContext,
        ISettingService settingService,
        IConditionModelFactory conditionModelFactory)
    {
        _announcementItemService = announcementItemService;
        _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        _dateTimeHelper = dateTimeHelper;
        _localizedModelFactory = localizedModelFactory;
        _localizationService = localizationService;
        _aclSupportedModelFactory = aclSupportedModelFactory;
        _scheduleModelFactory = scheduleModelFactory;
        _storeContext = storeContext;
        _settingService = settingService;
        _conditionModelFactory = conditionModelFactory;
    }

    #endregion

    #region Methods

    public virtual AnnouncementItemSearchModel PrepareAnnouncementItemSearchModel(AnnouncementItemSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<AnnouncementItemListModel> PrepareAnnouncementItemListModelAsync(AnnouncementItemSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var items = (await _announcementItemService.GetAllAnnouncementItemsAsync()).ToPagedList(searchModel);

        var model = await new AnnouncementItemListModel().PrepareToGridAsync(searchModel, items, () =>
        {
            return items.SelectAwait(async announcementItem =>
            {
                return await PrepareAnnouncementItemModelAsync(null, announcementItem, true);
            });
        });

        return model;
    }

    public virtual async Task<AnnouncementItemModel> PrepareAnnouncementItemModelAsync(AnnouncementItemModel model, AnnouncementItem announcementItem, bool excludeProperties = false)
    {
        Func<AnnouncementItemLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (announcementItem != null)
        {
            if (model == null)
            {
                model = new AnnouncementItemModel()
                {
                    Name = announcementItem.Name,
                    Color = announcementItem.Color,
                    DisplayOrder = announcementItem.DisplayOrder,
                    Id = announcementItem.Id,
                    Active = announcementItem.Active,
                    Title = announcementItem.Title,
                    Description = announcementItem.Description
                };
            }

            //define localized model configuration action
            localizedModelConfiguration = async (locale, languageId) =>
            {
                locale.Title = await _localizationService.GetLocalizedAsync(announcementItem, entity => entity.Title, languageId, false, false);
                locale.Description = await _localizationService.GetLocalizedAsync(announcementItem, entity => entity.Description, languageId, false, false);
            };
        }

        if (!excludeProperties)
        {
            //prepare model schedule mappings
            await _scheduleModelFactory.PrepareScheduleMappingModelAsync(model, announcementItem);

            //prepare customer condition mapping model
            await _conditionModelFactory.PrepareCustomerConditionMappingSearchModelAsync(model, announcementItem);

            //prepare model customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, announcementItem, excludeProperties);

            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, announcementItem, excludeProperties);
        }

        if (announcementItem == null)
        {
            model.Active = true;
        }

        return model;
    }

    public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
    {
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var announcementSettings = await _settingService.LoadSettingAsync<AnnouncementSettings>(storeId);

        var model = announcementSettings.ToSettingsModel<ConfigurationModel>();

        model.ActiveStoreScopeConfiguration = storeId;

        if (storeId <= 0)
            return model;

        model.DisplayTypeId_OverrideForStore = await _settingService.SettingExistsAsync(announcementSettings, x => x.DisplayTypeId, storeId);
        model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(announcementSettings, x => x.EnablePlugin, storeId);
        model.ItemSeparator_OverrideForStore = await _settingService.SettingExistsAsync(announcementSettings, x => x.ItemSeparator, storeId);
        model.WidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(announcementSettings, x => x.WidgetZone, storeId);
        model.AllowCustomersToMinimize_OverrideForStore = await _settingService.SettingExistsAsync(announcementSettings, x => x.AllowCustomersToMinimize, storeId);
        model.AllowCustomersToClose_OverrideForStore = await _settingService.SettingExistsAsync(announcementSettings, x => x.AllowCustomersToClose, storeId);

        return model;
    }

    #endregion
}
