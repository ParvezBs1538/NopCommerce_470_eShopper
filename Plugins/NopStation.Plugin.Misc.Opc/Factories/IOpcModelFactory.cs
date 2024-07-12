using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Services.Payments;
using Nop.Web.Models.Checkout;
using NopStation.Plugin.Misc.Opc.Models;

namespace NopStation.Plugin.Misc.Opc.Factories;

public interface IOpcModelFactory
{
    Task<OpcModel> PrepareOpcModelAsync(IList<ShoppingCartItem> cart);
    Task<CheckoutShippingMethodModel> PrepareShippingMethodModelAsync(IList<ShoppingCartItem> cart, Address shippingAddress);
    Task<CheckoutPaymentMethodModel> PreparePaymentMethodsModelAsync(IList<ShoppingCartItem> cart, int countryId = 0);
    Task<CheckoutPaymentInfoModel> PreparePaymentInfoModelAsync(IPaymentMethod paymentMethod);
    Task<EstimateShippingModel> PrepareEstimateShippingModelAsync(IList<ShoppingCartItem> cart, bool setEstimateShippingDefaultAddress = true);

    Task<EstimateShippingResultModel> PrepareEstimateShippingResultModelAsync(IList<ShoppingCartItem> cart, EstimateShippingModel request, bool cacheOfferedShippingOptions);
}