using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Models
{
    public partial record AbandonedCartSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Customer")]
        public string SearchCustomerName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Status")]
        public int StatusId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.Customer")]
        public int CustomerId { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchEmail")]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchFirstName")]
        public string SearchFirstName { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchLastName")]
        public string SearchLastName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AbandonedCarts.Fields.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.ShoppingCartType.Product")]
        public int ProductId { get; set; }
    }
}
