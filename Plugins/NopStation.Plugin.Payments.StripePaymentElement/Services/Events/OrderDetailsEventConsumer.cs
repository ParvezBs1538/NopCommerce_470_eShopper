using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Order;

namespace NopStation.Plugin.Payments.StripePaymentElement.Services.Events;

public class OrderDetailsEventConsumer : IConsumer<ModelPreparedEvent<BaseNopModel>>
{
    private readonly IOrderService _orderService;

    public OrderDetailsEventConsumer(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task HandleEventAsync(ModelPreparedEvent<BaseNopModel> eventMessage)
    {
        if (eventMessage.Model is not OrderDetailsModel model)
            return;

        var order = await _orderService.GetOrderByIdAsync(model.Id);

        if (order is null)
            return;

        var paymentMethodSystemName = order.PaymentMethodSystemName;

        if (paymentMethodSystemName != StripeDefaults.SystemName)
            return;

        if (!string.IsNullOrEmpty(order.CardType))
            model.PaymentMethod = char.ToUpper(order.CardType[0]) + order.CardType[1..];
    }
}
