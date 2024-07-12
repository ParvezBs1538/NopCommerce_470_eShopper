using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.UI;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Services
{
    public class EventConsumer : IConsumer<EntityInsertedEvent<RecurringPayment>>,
        IConsumer<PageRenderingEvent>
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;

        #endregion

        #region Ctor

        public EventConsumer(IOrderService orderService,
            IPaymentPluginManager paymentPluginManager)
        {
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
        }

        #endregion

        #region Methods

        public async Task HandleEventAsync(EntityInsertedEvent<RecurringPayment> eventMessage)
        {
            var recurringPayment = eventMessage?.Entity;
            if (recurringPayment == null)
                return;

            //add first payment to history right after creating recurring payment
            await _orderService.InsertRecurringPaymentHistoryAsync(new RecurringPaymentHistory
            {
                RecurringPaymentId = recurringPayment.Id,
                OrderId = recurringPayment.InitialOrderId,
                CreatedOnUtc = DateTime.UtcNow
            });
        }

        public async Task HandleEventAsync(PageRenderingEvent eventMessage)
        {
            if (eventMessage.GetRouteName() == null)
                return;

            //check whether the plugin is active
            if (!await _paymentPluginManager.IsPluginActiveAsync(BlueSnapDefaults.SystemName))
                return;
        }

        #endregion
    }
}