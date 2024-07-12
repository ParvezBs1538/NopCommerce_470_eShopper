namespace NopStation.Plugin.SMS.TeleSign.Domains
{
    public class SmsTemplateSystemNames
    {
        #region Customer

        /// <summary>
        /// Represents system name of notification about new registration
        /// </summary>
        public const string CustomerRegisteredNotification = "NewCustomer.Notification";

        /// <summary>
        /// Represents system name of customer welcome message
        /// </summary>
        public const string CustomerWelcomeMessage = "Customer.WelcomeMessage";

        /// <summary>
        /// Represents system name of email validation message
        /// </summary>
        public const string CustomerEmailValidationMessage = "Customer.EmailValidationMessage";
        
        #endregion

        #region Order

        /// <summary>
        /// Represents system name of notification vendor about placed order
        /// </summary>
        public const string OrderPlacedVendorNotification = "OrderPlaced.VendorNotification";

        /// <summary>
        /// Represents system name of notification store owner about placed order
        /// </summary>
        public const string OrderPlacedAdminNotification = "OrderPlaced.AdminNotification";
        
        /// <summary>
        /// Represents system name of notification store owner about paid order
        /// </summary>
        public const string OrderPaidAdminNotification = "OrderPaid.AdminNotification";

        /// <summary>
        /// Represents system name of notification customer about paid order
        /// </summary>
        public const string OrderPaidCustomerNotification = "OrderPaid.CustomerNotification";

        /// <summary>
        /// Represents system name of notification vendor about paid order
        /// </summary>
        public const string OrderPaidVendorNotification = "OrderPaid.VendorNotification";
        
        /// <summary>
        /// Represents system name of notification customer about placed order
        /// </summary>
        public const string OrderPlacedCustomerNotification = "OrderPlaced.CustomerNotification";

        /// <summary>
        /// Represents system name of notification customer about sent shipment
        /// </summary>
        public const string ShipmentSentCustomerNotification = "ShipmentSent.CustomerNotification";

        /// <summary>
        /// Represents system name of notification customer about delivered shipment
        /// </summary>
        public const string ShipmentDeliveredCustomerNotification = "ShipmentDelivered.CustomerNotification";
        
        /// <summary>
        /// Represents system name of notification customer about delivered shipment
        /// </summary>
        public const string ShipmentDeliveredCustomerOTPNotification = "ShipmentDelivered.CustomerOTPNotification";

        /// <summary>
        /// Represents system name of notification customer about completed order
        /// </summary>
        public const string OrderCompletedCustomerNotification = "OrderCompleted.CustomerNotification";

        /// <summary>
        /// Represents system name of notification customer about cancelled order
        /// </summary>
        public const string OrderCancelledCustomerNotification = "OrderCancelled.CustomerNotification";

        /// <summary>
        /// Represents system name of notification store owner about refunded order
        /// </summary>
        public const string OrderRefundedAdminNotification = "OrderRefunded.AdminNotification";

        /// <summary>
        /// Represents system name of notification customer about refunded order
        /// </summary>
        public const string OrderRefundedCustomerNotification = "OrderRefunded.CustomerNotification";
        
        #endregion
            
        #region Forum

        /// <summary>
        /// Represents system name of notification about new forum topic
        /// </summary>
        public const string NewForumTopicMessage = "Forums.NewForumTopic";

        /// <summary>
        /// Represents system name of notification about new forum post
        /// </summary>
        public const string NewForumPostMessage = "Forums.NewForumPost";

        /// <summary>
        /// Represents system name of notification about new private message
        /// </summary>
        public const string PrivateMessageNotification = "Customer.NewPM";

        #endregion
    }
}
