using Nop.Web.Models.Checkout;

namespace NopStation.Plugin.Misc.WebApi.Models.Checkout
{
    public class OpcStepResponseModel
    {
        public OpcStepResponseModel()
        {
            ShippingAddressModel = new CheckoutShippingAddressModel();
            ShippingMethodModel = new CheckoutShippingMethodModel();
            PaymentMethodModel = new CheckoutPaymentMethodModel();
            PaymentInfoModel = new CheckoutPaymentInfoModel();
            ConfirmModel = new CheckoutConfirmOrderModel();
            CompletedModel = new CheckoutCompletedModel();
        }

        public OpcStep NextStep { get; set; }

        public CheckoutBillingAddressModel BillingAddressModel { get; set; }

        public CheckoutShippingAddressModel ShippingAddressModel { get; set; }

        public CheckoutShippingMethodModel ShippingMethodModel { get; set; }

        public CheckoutPaymentMethodModel PaymentMethodModel { get; set; }

        public CheckoutPaymentInfoModel PaymentInfoModel { get; set; }

        public CheckoutConfirmOrderModel ConfirmModel { get; set; }

        public CheckoutCompletedModel CompletedModel { get; set; }
    }
}
