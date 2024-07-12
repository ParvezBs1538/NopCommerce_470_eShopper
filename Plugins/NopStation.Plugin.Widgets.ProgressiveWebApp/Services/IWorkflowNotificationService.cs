using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using Nop.Services.Messages;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public interface IWorkflowNotificationService
    {
        #region Customer workflow

        Task<IList<int>> SendCustomerRegisteredNotificationMessageAsync(Customer customer, int languageId);

        Task<IList<int>> SendCustomerWelcomeMessageAsync(Customer customer, int languageId);

        Task<IList<int>> SendCustomerEmailValidationMessageAsync(Customer customer, int languageId);

        #endregion

        #region Order workflow

        Task<IList<int>> SendOrderPlacedVendorNotificationAsync(Order order, Vendor vendor, int languageId);

        Task<IList<int>> SendOrderPlacedAdminNotificationAsync(Order order, int languageId);

        Task<IList<int>> SendOrderPaidAdminNotificationAsync(Order order, int languageId);

        Task<IList<int>> SendOrderPaidCustomerNotificationAsync(Order order, int languageId);

        Task<IList<int>> SendOrderPaidVendorNotificationAsync(Order order, Vendor vendor, int languageId);

        Task<IList<int>> SendOrderPlacedCustomerNotificationAsync(Order order, int languageId);

        Task<IList<int>> SendShipmentSentCustomerNotificationAsync(Shipment shipment, int languageId);

        Task<IList<int>> SendShipmentDeliveredCustomerNotificationAsync(Shipment shipment, int languageId);

        Task<IList<int>> SendOrderCompletedCustomerNotificationAsync(Order order, int languageId);

        Task<IList<int>> SendOrderCancelledCustomerNotificationAsync(Order order, int languageId);

        Task<IList<int>> SendOrderRefundedAdminNotificationAsync(Order order, decimal refundedAmount, int languageId);

        Task<IList<int>> SendOrderRefundedCustomerNotificationAsync(Order order, decimal refundedAmount, int languageId);

        #endregion

        #region Forum Notifications

        Task<IList<int>> SendNewForumTopicMessageAsync(Customer customer, ForumTopic forumTopic, Forum forum, int languageId);

        Task<IList<int>> SendNewForumPostMessageAsync(Customer customer, ForumPost forumPost,
            ForumTopic forumTopic, Forum forum, int friendlyForumTopicPageIndex, int languageId);

        Task<IList<int>> SendPrivateMessageNotificationAsync(PrivateMessage privateMessage, int languageId);

        #endregion

        #region Misc

        Task<IList<int>> SendAbandonedCartNotificationAsync(Customer customer, int languageId);

        Task<int> SendNotificationAsync(PushNotificationTemplate pushNotificationTemplate,
            int languageId, IEnumerable<Token> tokens, int storeId, int customerId = 0);

        int SendNotification(string title, string body, string iconUrl, string imageUrl,
            string url, int storeId, int customerId, bool rtl);

        #endregion
    }
}