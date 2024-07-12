using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Models
{
    public record AbandonmentMaintenanceModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Status")]
        public int StatusId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.LastActivityBefore")]
        [UIHint("Date")]
        public DateTime LastActivityBefore { get; set; }
        public int? NumberOfDeletedItems { get; set; }

    }
}
