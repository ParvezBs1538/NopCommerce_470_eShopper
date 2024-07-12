using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.OrderRatings.Domain
{
    public class OrderRating : BaseEntity
    {
        public int OrderId { set; get; }

        public int Rating { set; get; }

        public string Note { get; set; }

        public DateTime? RatedOnUtc { get; set; }
    }
}
