namespace NopStation.Plugin.Widgets.PinteresteAnalytics
{
    public class PinterestAnalyticsDefaults
    {
        public static string SystemName => "Widgets.PinterestAnalytics";
        public static string TrackedEventsSessionValue => "PinterestAnalyticsTrackedEvents";

        #region Events code

        public static string ADD_TO_CART = "addtocart";

        public static string REMOVE_FROM_CART = "remove_from_cart";

        public static string ADD_TO_WISHLIST = "add_to_wishlist";

        public static string REMOVE_FROM_WISHLIST = "remove_from_wishlist";

        public static string SEARCH = "search";

        public static string CATEGORY_VIEW = "viewcategory";

        public static string CUSTOMER_REGISTER = "customer_registered";

        public static string CUSTOMER_LOGIN = "customer_logged_in";

        public static string VIEW_ITEM = "view_item";

        public static string PURCHASE = "checkout";

        #endregion
    }
}