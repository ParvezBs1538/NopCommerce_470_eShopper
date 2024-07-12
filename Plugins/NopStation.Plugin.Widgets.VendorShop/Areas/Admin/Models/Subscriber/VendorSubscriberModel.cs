using System;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.Subscriber
{
    public partial record VendorSubscriberModel : BaseNopEntityModel
    {
        public string SubscriberEmail { get; set; }
        public DateTime SubscribedOn { get; set; }

    }
}
