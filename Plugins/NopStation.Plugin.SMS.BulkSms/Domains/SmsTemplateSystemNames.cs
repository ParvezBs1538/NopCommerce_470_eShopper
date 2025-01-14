﻿namespace NopStation.Plugin.SMS.BulkSms.Domains
{
    public class SmsTemplateSystemNames
    {
        #region Customer

        /// <summary>
        /// Represents system name of notification about new registration
        /// </summary>
        public const string CUSTOMER_REGISTERED_NOTIFICATION = "NewCustomer.Notification";

        /// <summary>
        /// Represents system name of customer welcome message
        /// </summary>
        public const string CUSTOMER_WELCOME_MESSAGE = "Customer.WelcomeMessage";

        /// <summary>
        /// Represents system name of email validation message
        /// </summary>
        public const string CUSTOMER_EMAIL_VALIDATION_MESSAGE = "Customer.EmailValidationMessage";
        
        #endregion

        #region Order

        /// <summary>
        /// Represents system name of notification vendor about placed order
        /// </summary>
        public const string ORDER_PLACED_VENDOR_NOTIFICATION = "OrderPlaced.VendorNotification";

        /// <summary>
        /// Represents system name of notification store owner about placed order
        /// </summary>
        public const string ORDER_PLACED_ADMIN_NOTIFICATION = "OrderPlaced.AdminNotification";
        
        /// <summary>
        /// Represents system name of notification store owner about paid order
        /// </summary>
        public const string ORDER_PAID_ADMIN_NOTIFICATION = "OrderPaid.AdminNotification";

        /// <summary>
        /// Represents system name of notification customer about paid order
        /// </summary>
        public const string ORDER_PAID_CUSTOMER_NOTIFICATION = "OrderPaid.CustomerNotification";

        /// <summary>
        /// Represents system name of notification vendor about paid order
        /// </summary>
        public const string ORDER_PAID_VENDOR_NOTIFICATION = "OrderPaid.VendorNotification";
        
        /// <summary>
        /// Represents system name of notification customer about placed order
        /// </summary>
        public const string ORDER_PLACED_CUSTOMER_NOTIFICATION = "OrderPlaced.CustomerNotification";

        /// <summary>
        /// Represents system name of notification customer about sent shipment
        /// </summary>
        public const string SHIPMENT_SENT_CUSTOMER_NOTIFICATION = "ShipmentSent.CustomerNotification";

        /// <summary>
        /// Represents system name of notification customer about delivered shipment
        /// </summary>
        public const string SHIPMENT_DELIVERED_CUSTOMER_NOTIFICATION = "ShipmentDelivered.CustomerNotification";
        
        /// <summary>
        /// Represents system name of notification customer about delivered shipment
        /// </summary>
        public const string SHIPMENT_DELIVERED_CUSTOMER_OTP_NOTIFICATION = "ShipmentDelivered.CustomerOTPNotification";

        /// <summary>
        /// Represents system name of notification customer about completed order
        /// </summary>
        public const string ORDER_COMPLETED_CUSTOMER_NOTIFICATION = "OrderCompleted.CustomerNotification";

        /// <summary>
        /// Represents system name of notification customer about cancelled order
        /// </summary>
        public const string ORDER_CANCELLED_CUSTOMER_NOTIFICATION = "OrderCancelled.CustomerNotification";

        /// <summary>
        /// Represents system name of notification store owner about refunded order
        /// </summary>
        public const string ORDER_REFUNDED_ADMIN_NOTIFICATION = "OrderRefunded.AdminNotification";

        /// <summary>
        /// Represents system name of notification customer about refunded order
        /// </summary>
        public const string ORDER_REFUNDED_CUSTOMER_NOTIFICATION = "OrderRefunded.CustomerNotification";
        
        #endregion
            
        #region Forum

        /// <summary>
        /// Represents system name of notification about new forum topic
        /// </summary>
        public const string NEW_FORUM_TOPIC_MESSAGE = "Forums.NewForumTopic";

        /// <summary>
        /// Represents system name of notification about new forum post
        /// </summary>
        public const string NEW_FORUM_POST_MESSAGE = "Forums.NewForumPost";

        /// <summary>
        /// Represents system name of notification about new private message
        /// </summary>
        public const string PRIVATE_MESSAGE_NOTIFICATION = "Customer.NewPM";

        #endregion
    }
}
