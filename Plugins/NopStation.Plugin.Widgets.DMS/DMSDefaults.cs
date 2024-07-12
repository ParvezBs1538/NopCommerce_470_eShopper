namespace NopStation.Plugin.Widgets.DMS
{
    public class DMSDefaults
    {
        public static string DMSScheduleTaskType => "NopStation.Plugin.Widgets.DMS.DMSScheduleTask";

        public static readonly string ShipperCustomerRoleName = "Shippers";

        public static readonly string Token = "DMS-Token";

        public static readonly string CustomerGuid = "CustomerGuid";

        public static readonly string CustomerId = "DMS-CustomerId";

        public static readonly string NST = "DMS-NST";

        public static readonly string NSTKey = "NST-KEY";

        public static readonly string IAT = "iat";

        public static readonly string CustomerSecret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";
        public static readonly string DummyGoogleAPiKey = "AIzaSyB41DRUbKWJHPxaFjMAwdrzWzbVKartNGg";

        public static string DeviceId_Attribute => "DMS-DeviceId";

        #region Widget Zones

        public static readonly string ShipmentDetailsBottom = "shipment_details_bottom";

        public static readonly string ShipmentDetailsFormBottom = "shipment_details_form_bottom";

        public static string ShipperDeviceListButtonWidget => "admin_dmsshipperdevice_list_buttons";

        public static string SHIPMENT_PAGE_BODY_MIDDLE => "shipment_page_body_middle";

        #endregion

        #region Widget Zone Component

        public const string CUSTOMER_SHIPMENT_NOTE_TABLE_VIEW_COMPONENT_NAME = "CustomerShipmentNoteTable";

        #endregion
    }
}
