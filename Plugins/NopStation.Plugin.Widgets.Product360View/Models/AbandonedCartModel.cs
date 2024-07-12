using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.Product360View.Models
{
    public record AbandonedCartModel : BaseNopEntityModel
    {

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.CustomerId")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.ShoppingCartItemId")]
        public int ShoppingCartItemId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.ProductId")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.OrderItemId")]
        public int OrderItemId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.StatusId")]
        public int StatusId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.StatusChangedOn")]
        public DateTime StatusChangedOn { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.FirstNotificationSentOn")]
        public DateTime FirstNotificationSentOn { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.SecondNotificationSentOn")]
        public DateTime SecondNotificationSentOn { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.FirstNotificationMessageQueueId")]
        public int FirstNotificationMessageQueueId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.SecondNotificationMessageQueueId")]
        public int SecondNotificationMessageQueueId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.IsSoftDeleted")]
        public bool IsSoftDeleted { get; set; }
        
    }
}