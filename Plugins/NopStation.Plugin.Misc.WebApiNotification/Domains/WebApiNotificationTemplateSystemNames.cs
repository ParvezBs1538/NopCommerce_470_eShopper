namespace NopStation.Plugin.Misc.WebApiNotification.Domains
{
    public class WebApiNotificationTemplateSystemNames
    {
        #region Customer

        /// <summary>
        /// Represents system name of notification about new registration
        /// </summary>
        public const string CUSTOMER_REGISTERED_NOTIFICATION = "NewCustomer.Notification";

        /// <summary>
        /// Represents system name of customer welcome message
        /// </summary>
        public const string CUSTOMER_WELCOME_NOTIFICATION = "Customer.WelcomeNotification";

        /// <summary>
        /// Represents system name of customer welcome message
        /// </summary>
        public const string CUSTOMER_REGISTERED_WELCOME_NOTIFICATION = "Customer.RegisteredWelcomeNotification";

        /// <summary>
        /// Represents system name of email validation message
        /// </summary>
        public const string CUSTOMER_EMAIL_VALIDATION_NOTIFICATION = "Customer.EmailValidationNotification";

        #endregion

        #region Order

        /// <summary>
        /// Represents system name of notification customer about paid order
        /// </summary>
        public const string ORDER_PAID_CUSTOMER_NOTIFICATION = "OrderPaid.CustomerNotification";

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
        /// Represents system name of notification customer about completed order
        /// </summary>
        public const string ORDER_COMPLETED_CUSTOMER_NOTIFICATION = "OrderCompleted.CustomerNotification";

        /// <summary>
        /// Represents system name of notification customer about cancelled order
        /// </summary>
        public const string ORDER_CANCELLED_CUSTOMER_NOTIFICATION = "OrderCancelled.CustomerNotification";

        /// <summary>
        /// Represents system name of notification customer about refunded order
        /// </summary>
        public const string ORDER_REFUNDED_CUSTOMER_NOTIFICATION = "OrderRefunded.CustomerNotification";

        #endregion
    }
}
