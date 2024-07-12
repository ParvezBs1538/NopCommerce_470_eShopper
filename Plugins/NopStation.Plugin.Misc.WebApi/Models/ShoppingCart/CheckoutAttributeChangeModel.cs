using System.Collections.Generic;
using Nop.Web.Models.ShoppingCart;

namespace NopStation.Plugin.Misc.WebApi.Models.ShoppingCart
{
    public class CheckoutAttributeChangeModel
    {
        public CheckoutAttributeChangeModel()
        {
            EnabledAttributeIds = new List<int>();
            DisabledAttributeIds = new List<int>();
            OrderTotals = new OrderTotalsModel();
        }

        public ShoppingCartModel Cart { get; set; }

        public OrderTotalsModel OrderTotals { get; set; }

        public string SelectedCheckoutAttributess { get; set; }

        public IList<int> EnabledAttributeIds { get; set; }

        public IList<int> DisabledAttributeIds { get; set; }
    }
}
