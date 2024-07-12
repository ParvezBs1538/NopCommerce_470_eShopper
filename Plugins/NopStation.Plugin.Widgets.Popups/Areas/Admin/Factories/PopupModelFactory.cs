using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.Popups.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Popups.Domains;
using NopStation.Plugin.Widgets.Popups.Services;

namespace NopStation.Plugin.Widgets.Popups.Areas.Admin.Factories;

public class PopupModelFactory : IPopupModelFactory
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
    private readonly ILocalizedModelFactory _localizedModelFactory;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IPopupService _newsletterPopupService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IScheduleModelFactory _scheduleModelFactory;
    private readonly IConditionModelFactory _conditionModelFactory;
    private readonly IAclSupportedModelFactory _aclSupportedModelFactory;

    #endregion

    #region Ctor

    public PopupModelFactory(IStoreContext storeContext,
        IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
        ILocalizedModelFactory localizedModelFactory,
        IBaseAdminModelFactory baseAdminModelFactory,
        ILocalizationService localizationService,
        ISettingService settingService,
        IPopupService newsletterPopupService,
        IDateTimeHelper dateTimeHelper,
        IScheduleModelFactory scheduleModelFactory,
        IConditionModelFactory conditionModelFactory,
        IAclSupportedModelFactory aclSupportedModelFactory)
    {
        _storeContext = storeContext;
        _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        _localizedModelFactory = localizedModelFactory;
        _baseAdminModelFactory = baseAdminModelFactory;
        _localizationService = localizationService;
        _settingService = settingService;
        _newsletterPopupService = newsletterPopupService;
        _dateTimeHelper = dateTimeHelper;
        _scheduleModelFactory = scheduleModelFactory;
        _conditionModelFactory = conditionModelFactory;
        _aclSupportedModelFactory = aclSupportedModelFactory;
    }

    #endregion

    #region Utilities

    protected async Task PrepareActiveOptionsAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        items.Add(new SelectListItem()
        {
            Text = await _localizationService.GetResourceAsync("Admin.NopStation.Popups.Popups.List.SearchActive.Active"),
            Value = "1"
        });
        items.Add(new SelectListItem()
        {
            Text = await _localizationService.GetResourceAsync("Admin.NopStation.Popups.Popups.List.SearchActive.Inactive"),
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

    public virtual async Task<PopupSearchModel> PreparePopupSearchModelAsync(PopupSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        await PrepareActiveOptionsAsync(searchModel.AvailableActiveOptions);

        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<PopupListModel> PreparePopupListModelAsync(PopupSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get parameters to filter comments
        var overridePublished = searchModel.SearchActiveId == 0 ? null : (bool?)(searchModel.SearchActiveId == 1);

        //get popups
        var popups = await _newsletterPopupService.GetAllPopupsAsync(
            storeId: searchModel.SearchStoreId,
            overridePublished: overridePublished,
            pageSize: searchModel.PageSize,
            pageIndex: searchModel.Page - 1);

        //prepare list model
        var model = await new PopupListModel().PrepareToGridAsync(searchModel, popups, () =>
        {
            return popups.SelectAwait(async popup =>
            {
                return await PreparePopupModelAsync(null, popup, true);
            });
        });

        return model;
    }

    public async Task<PopupModel> PreparePopupModelAsync(PopupModel model, Popup popup, bool excludeProperties = false)
    {
        Func<PopupLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (popup != null)
        {
            if (model == null)
            {
                model = popup.ToModel<PopupModel>();
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(popup.CreatedOnUtc);
                model.ColumnTypeStr = await _localizationService.GetLocalizedEnumAsync(popup.ColumnType);
                model.DeviceTypeStr = await _localizationService.GetLocalizedEnumAsync(popup.DeviceType);
            }

            if (!excludeProperties)
            {
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(popup, entity => entity.Name, languageId, false, false);
                    locale.Column1Text = await _localizationService.GetLocalizedAsync(popup, entity => entity.Column1Text, languageId, false, false);
                    locale.Column1DesktopPictureId = await _localizationService.GetLocalizedAsync(popup, entity => entity.Column1DesktopPictureId, languageId, false, false);
                    locale.Column1PopupUrl = await _localizationService.GetLocalizedAsync(popup, entity => entity.Column1PopupUrl, languageId, false, false);
                    locale.Column2Text = await _localizationService.GetLocalizedAsync(popup, entity => entity.Column2Text, languageId, false, false);
                    locale.Column2DesktopPictureId = await _localizationService.GetLocalizedAsync(popup, entity => entity.Column2DesktopPictureId, languageId, false, false);
                    locale.Column2PopupUrl = await _localizationService.GetLocalizedAsync(popup, entity => entity.Column2PopupUrl, languageId, false, false);
                };

                //prepare customer condition mapping model
                await _conditionModelFactory.PrepareCustomerConditionMappingSearchModelAsync(model, popup);

                //prepare product condition mapping model
                await _conditionModelFactory.PrepareProductConditionMappingSearchModelAsync(model, popup);
            }
        }

        //set default values for the new model
        if (popup == null)
        {
            model.Active = true;
            model.EnableStickyButton = true;
            model.OpenPopupOnLoadPage = true;
            model.DelayTime = 2000;
            model.AllowCustomerToSelectDoNotShowThisPopupAgain = true;
            model.PreSelectedDoNotShowThisPopupAgain = true;
        }

        if (!excludeProperties)
        {
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, popup, excludeProperties);

            //prepare model schedule mappings
            await _scheduleModelFactory.PrepareScheduleMappingModelAsync(model, popup);

            //prepare model customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, popup, excludeProperties);

            model.AvailableColumnTypes = (await ColumnType.DoubleColumn.ToSelectListAsync()).ToList();
            model.AvailableContentTypes = (await ContentType.Image.ToSelectListAsync()).ToList();
            model.AvailablePositions = (await Position.Left.ToSelectListAsync()).ToList();
            model.AvailableDeviceTypes = (await DeviceType.Both.ToSelectListAsync()).ToList();
        }

        return model;
    }

    #endregion
}
