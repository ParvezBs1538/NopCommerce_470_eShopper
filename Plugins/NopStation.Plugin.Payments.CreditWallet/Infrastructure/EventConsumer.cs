using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using NopStation.Plugin.Payments.CreditWallet.Domain;
using NopStation.Plugin.Payments.CreditWallet.Services;

namespace NopStation.Plugin.Payments.CreditWallet.Infrastructure
{
    public class EventConsumer : IConsumer<OrderStatusChangedEvent>
    {
        private readonly IWalletService _walletService;
        private readonly IActivityHistoryService _activityHistoryService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;

        public EventConsumer(IWalletService walletService,
            IActivityHistoryService activityHistoryService,
            IWorkContext workContext,
            ICustomerService customerService,
            ILocalizationService localizationService)
        {
            _walletService = walletService;
            _activityHistoryService = activityHistoryService;
            _workContext = workContext;
            _customerService = customerService;
            _localizationService = localizationService;
        }


        public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
        {
            if (eventMessage.PreviousOrderStatus != OrderStatus.Cancelled
                && eventMessage.Order.OrderStatus == OrderStatus.Cancelled)
            {
                if (await _walletService.GetWalletByCustomerIdAsync(eventMessage.Order.CustomerId) is Wallet wallet)
                {
                    wallet.AvailableCredit = eventMessage.Order.OrderTotal;
                    wallet.CreditUsed -= eventMessage.Order.OrderTotal;

                    await _walletService.UpdateWalletAsync(wallet);

                    var currCustomer = await _workContext.GetCurrentCustomerAsync();
                    var activity = (await _activityHistoryService.GetWalletActivityHistoryAsync(eventMessage.Order.CustomerId)).FirstOrDefault();

                    var newActivity = new ActivityHistory
                    {
                        PreviousTotalCreditUsed = activity.CurrentTotalCreditUsed,
                        CurrentTotalCreditUsed = activity.CurrentTotalCreditUsed - eventMessage.Order.OrderTotal,
                        CreatedOnUtc = DateTime.UtcNow,
                        ActivityTypeId = (int)ActivityType.OrderCancelled,
                        WalletCustomerId = eventMessage.Order.CustomerId,
                        Note = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.CreditWallet.OrderCancelledActivity"),
                            eventMessage.Order.OrderTotal, eventMessage.Order.Id, currCustomer.Email),
                    };

                    await _activityHistoryService.InsertActivityHistoryAsync(newActivity);
                }
            }
        }
    }
}
