namespace NopStation.Plugin.Misc.WebApi.Models.Checkout
{
    public enum OpcStep
    {
        CartPage = 0,
        BillingAddress = 1,
        ShippingAddress = 2,
        ShippingMethod = 3,
        PaymentMethod = 4,
        PaymentInfo = 5,
        ConfirmOrder = 6,
        RedirectToGateway = 7,
        Completed = 8
    }
}
