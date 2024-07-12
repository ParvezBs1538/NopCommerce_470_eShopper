using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

public class MegaMenu : BaseEntity, IStoreMappingSupported, ILocalizedEntity, ISoftDeletedEntity, IWidgetZoneSupported
{
    public string Name { get; set; }

    public bool Active { get; set; }

    public int DisplayOrder { get; set; }

    public int ViewTypeId { get; set; }

    public bool WithoutImages { get; set; }

    public string CssClass { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    public bool LimitedToStores { get; set; }

    public bool Deleted { get; set; }

    public bool HasWidgetZoneMappingApplied { get; set; }

    public ViewType ViewType
    {
        get => (ViewType)ViewTypeId;
        set => ViewTypeId = (int)value;
    }
}
