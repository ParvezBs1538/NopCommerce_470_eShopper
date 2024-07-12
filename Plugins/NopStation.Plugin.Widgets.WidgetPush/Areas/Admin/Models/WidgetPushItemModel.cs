using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.WidgetPush.Areas.Admin.Models
{
    public record WidgetPushItemModel : BaseNopEntityModel,
        ILocalizedModel<WidgetPushItemLocalizedModel>, IStoreMappingSupportedModel
    {
        public WidgetPushItemModel()
        {
            Locales = new List<WidgetPushItemLocalizedModel>();
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.WidgetZone")]
        public string WidgetZone { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Content")]
        public string Content { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Active")]
        public bool Active { get; set; }

        [UIHint("DateTimeNullable")]
        [NopResourceDisplayName("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.DisplayStartDate")]
        public DateTime? DisplayStartDate { get; set; }

        [UIHint("DateTimeNullable")]
        [NopResourceDisplayName("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.DisplayEndDate")]
        public DateTime? DisplayEndDate { get; set; }

        public string FormattedExistingWidgetZones { get; set; }

        public IList<WidgetPushItemLocalizedModel> Locales { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }

    public record WidgetPushItemLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WidgetPush.WidgetPushItems.Fields.Content")]
        public string Content { get; set; }
    }
}
