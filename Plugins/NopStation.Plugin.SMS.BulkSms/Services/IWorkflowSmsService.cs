using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using NopStation.Plugin.SMS.BulkSms.Domains;
using Nop.Services.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.BulkSms.Services
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

        Task<IList<int>> SendShipmentDeliveredCustomerOTPNotificationAsync(Shipment shipment, string otp, int languageId);

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
        
        Task<int> SendNotificationAsync(string phoneNumber, SmsTemplate smsTemplate,
            int languageId, IEnumerable<Token> tokens, int storeId, Customer customer);

        Task<int> SendNotificationAsync(string phoneNumber, string body, int storeId, Customer customer);

        #endregion
    }
}