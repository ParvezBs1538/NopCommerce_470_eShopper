using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.Subscriber
{
    public partial record VendorSubscriberSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.NopStation.VendorShop.Subscriber.Search.SearchSubscriberEmail")]
        public string SearchSubscriberEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.Subscriber.Search.SearchSubscriberName")]
        public string SearchSubscriberName { get; set; }

        //Campain message

        [NopResourceDisplayName("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.CampaignName")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.Subject")]
        public string Subject { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.Body")]
        public string Body { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.SendingDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? SendingDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.AllowedTokens")]
        public string AllowedTokens { get; set; }
        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
