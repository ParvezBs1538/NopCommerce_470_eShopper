using Nop.Web.Framework.Models;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.ShoppingCart;

namespace NopStation.Plugin.Misc.Opc.Models;

public partial record OpcModel : BaseNopModel
{
    public bool DisableBillingAddressCheckoutStep { get; set; }
    public CheckoutBillingAddressModel BillingAddressModel { get; set; }

    public bool ShippingRequired { get; set; }
    public CheckoutShippingAddressModel ShippingAddressModel { get; set; }

    public bool BypassShippingMethodSelection { get; set; }
    public CheckoutShippingMethodModel ShippingMethodModel { get; set; }

    public bool PaymentWorkflowRequired { get; set; }
    public bool BypassPaymentMethodSelection { get; set; }
    public CheckoutPaymentMethodModel PaymentMethodModel { get; set; }

    public bool ShowShoppingCart { get; set; }
    public ShoppingCartModel ShoppingCartModel { get; set; }

    public CheckoutConfirmModel ConfirmOrder { get; set; }

    public bool ShowDiscountBox { get; set; }
    public bool ShowGiftCardBox { get; set; }
    public bool ShowCheckoutAttributes { get; set; }
    public bool ShowOrderReviewData { get; set; }
    public bool ShowEstimateShipping { get; set; }
}