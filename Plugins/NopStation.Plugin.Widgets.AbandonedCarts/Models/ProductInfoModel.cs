using System;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Models
{
    public record ProductInfoModel : BaseNopModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public string SlugValue { get; set; }
        public int ProductQuantity { get; set; }
        public DateTime CartUpdatedOn { get; set; }
    }
}
