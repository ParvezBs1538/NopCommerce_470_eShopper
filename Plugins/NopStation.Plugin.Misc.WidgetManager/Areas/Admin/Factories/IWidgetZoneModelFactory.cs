using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;

public interface IWidgetZoneModelFactory
{
    Task PrepareWidgetZonesAsync(IList<SelectListItem> items, string[] widgetZones,
        bool withSpecialDefaultItem = true, string defaultItemText = null);

    Task PrepareWidgetZoneMappingSearchModelAsync<TModel, TEntity>(TModel model, TEntity entity)
        where TEntity : BaseEntity, IWidgetZoneSupported
        where TModel : IWidgetZoneSupportedModel;

    Task<WidgetZoneListModel> PrepareWidgetZoneMappingListModelAsync(WidgetZoneSearchModel searchModel);

    Task PrepareAddWidgetZoneMappingModelAsync<TModel, TEntity>(TModel model, TEntity entity,
        bool prepareWidgetZones = false, string[] widgetZones = null)
        where TEntity : BaseEntity, IWidgetZoneSupported
        where TModel : IWidgetZoneSupportedModel;
}

