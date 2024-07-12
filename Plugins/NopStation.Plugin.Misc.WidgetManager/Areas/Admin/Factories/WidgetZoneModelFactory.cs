using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Data.Mapping;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;
using NopStation.Plugin.Misc.WidgetManager.Services;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;

public class WidgetZoneModelFactory : IWidgetZoneModelFactory
{
    #region Fields

    private readonly IWidgetZoneService _widgetZoneService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public WidgetZoneModelFactory(IWidgetZoneService widgetZoneService,
        ILocalizationService localizationService)
    {
        _widgetZoneService = widgetZoneService;
        _localizationService = localizationService;
    }

    #endregion

    #region Utilities

    protected virtual async Task PrepareDefaultItemAsync(IList<SelectListItem> items, bool withSpecialDefaultItem, string defaultItemText = null, string defaultItemValue = "0")
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        if (!withSpecialDefaultItem)
            return;

        defaultItemText ??= await _localizationService.GetResourceAsync("Admin.Common.All");

        items.Insert(0, new SelectListItem { Text = defaultItemText, Value = defaultItemValue });
    }

    #endregion

    #region Methods

    public virtual async Task PrepareWidgetZonesAsync(IList<SelectListItem> items, string[] widgetZones, bool withSpecialDefaultItem = true, string defaultItemText = null)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        if (widgetZones == null)
            throw new ArgumentNullException(nameof(widgetZones));

        //prepare available activity log types
        foreach (var widgetZone in widgetZones)
            items.Add(new SelectListItem(widgetZone, widgetZone));

        //insert special item for the default value
        await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText);
    }

    public virtual Task PrepareWidgetZoneMappingSearchModelAsync<TModel, TEntity>(TModel model, TEntity entity)
        where TEntity : BaseEntity, IWidgetZoneSupported
        where TModel : IWidgetZoneSupportedModel
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (entity != null)
        {
            var entityId = entity.Id;
            var entityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

            var searchModel = new WidgetZoneSearchModel()
            {
                EntityName = entityName,
                EntityId = entityId
            };
            //prepare page parameters
            searchModel.SetGridPageSize();

            model.WidgetZoneSearchModel = searchModel;
        }

        return Task.CompletedTask;
    }

    public virtual async Task<WidgetZoneListModel> PrepareWidgetZoneMappingListModelAsync(WidgetZoneSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get widget zone mappings
        var widgetZoneMappings = (await _widgetZoneService.GetEntityWidgetZoneMappingsAsync(searchModel.EntityId, searchModel.EntityName)).ToPagedList(searchModel);

        //prepare list model
        var model = new WidgetZoneListModel().PrepareToGrid(searchModel, widgetZoneMappings, () =>
        {
            return widgetZoneMappings.Select(widgetZoneMapping =>
            {
                //fill in model values from the entity
                var widgetZoneMappingModel = widgetZoneMapping.ToModel<WidgetZoneModel>();

                return widgetZoneMappingModel;
            });
        });

        return model;
    }

    public async Task PrepareAddWidgetZoneMappingModelAsync<TModel, TEntity>(TModel model, TEntity entity,
        bool prepareWidgetZones = false, string[] widgetZones = null)
        where TEntity : BaseEntity, IWidgetZoneSupported
        where TModel : IWidgetZoneSupportedModel
    {
        if (model == null)
            throw new ArgumentException(nameof(model));

        if (entity != null)
        {
            var wzModel = new WidgetZoneModel();
            wzModel.EntityId = entity.Id;
            wzModel.EntityName = NameCompatibilityManager.GetTableName(typeof(TEntity));

            if (prepareWidgetZones)
                await PrepareWidgetZonesAsync(wzModel.AvaliableWidgetZones, widgetZones, false);

            model.AddWidgetZoneModel = wzModel;
        }
    }

    #endregion
}
