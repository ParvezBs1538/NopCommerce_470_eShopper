using Nop.Web.Models.Checkout;
using Nop.Web.Models.ShoppingCart;

namespace NopStation.Plugin.Misc.WebApi.Models.Checkout
{
    public class CheckoutConfirmOrderModel
    {
        public CheckoutConfirmOrderModel()
        {
            Cart = new ShoppingCartModel();
            OrderTotals = new OrderTotalsModel();
            Confirm = new CheckoutConfirmModel();
        }

        public ShoppingCartModel Cart { get; set; }

        public OrderTotalsModel OrderTotals { get; set; }

        public CheckoutConfirmModel Confirm { get; set; }

        public string SelectedCheckoutAttributes { get; set; }

        public EstimateShippingModel EstimateShipping { get; set; }
    }
}
