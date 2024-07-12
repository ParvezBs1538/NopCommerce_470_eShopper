using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.OrderRatings.Areas.Admin.Models
{
    public record OrderRatingModel : BaseNopEntityModel
    {
        public int OrderId { set; get; }

        [NopResourceDisplayName("Admin.NopStation.OrderRatings.OrderRating.Fields.Rating")]
        public int Rating { set; get; }

        [NopResourceDisplayName("Admin.NopStation.OrderRatings.OrderRating.Fields.Note")]
        public string Note { get; set; }

        [NopResourceDisplayName("Admin.NopStation.OrderRatings.OrderRating.Fields.RatedOn")]
        public DateTime? RatedOn { get; set; }
    }
}
