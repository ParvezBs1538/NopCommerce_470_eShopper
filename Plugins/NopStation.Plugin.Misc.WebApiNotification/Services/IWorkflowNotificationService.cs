using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Messages;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Services
{
    public interface IWorkflowNotificationService
    {
        #region Campaigns

        Task<IList<int>> SendCampaignNotificationAsync(WebApiNotificationCampaign campaign);

        #endregion

        #region Customer workflow

        Task<IList<int>> SendCustomerRegisteredNotificationAsync(Customer customer, int languageId = 0);

        Task<IList<int>> SendCustomerCustomerRegisteredWelcomeNotificationAsync(Customer customer, int languageId);

        Task<IList<int>> SendCustomerEmailValidationNotificationAsync(Customer customer, int languageId);

        Task<IList<int>> SendCustomerCustomerWelcomeNotificationAsync(Customer customer, int languageId);

        #endregion

        #region Order workflow

        Task<IList<int>> SendOrderPaidCustomerNotificationAsync(Order order, int languageId);

        Task<IList<int>> SendOrderPlacedCustomerNotificationAsync(Order order, int languageId);

        Task<IList<int>> SendShipmentSentCustomerNotificationAsync(Shipment shipment, int languageId);

        Task<IList<int>> SendShipmentDeliveredCustomerNotificationAsync(Shipment shipment, int languageId);

        Task<IList<int>> SendOrderCompletedCustomerNotificationAsync(Order order, int languageId);

        Task<IList<int>> SendOrderCancelledCustomerNotificationAsync(Order order, int languageId);

        Task<IList<int>> SendOrderRefundedCustomerNotificationAsync(Order order, decimal refundedAmount, int languageId);

        #endregion

        #region Misc

        Task<IList<int>> SendNotificationAsync(Customer customer, WebApiNotificationTemplate template, int languageId, IEnumerable<Token> tokens, int storeId);

        #endregion
    }
}