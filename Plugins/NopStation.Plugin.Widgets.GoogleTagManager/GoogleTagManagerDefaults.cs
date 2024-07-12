namespace NopStation.Plugin.Widgets.GoogleTagManager
{
    public class GoogleTagManagerDefaults
    {
        public static string HeadTrackingScript = @"<!-- NS Google Tag Manager (script) -->
                                        <script>
                                        window.dataLayer = window.dataLayer || []; 
                                        dataLayer.push({%TRACKINGINFORMATION%});
                                      </script>
                                      <script>
                                            (function(w,d,s,l,i){w[l]=w[l]||[];w[l].push({'gtm.start': new Date().getTime(),event:'gtm.js'});
                                            var f=d.getElementsByTagName(s)[0], j=d.createElement(s),dl=l!='dataLayer'?'&l='+l:'';
                                            j.async=true;j.src= 'https://www.googletagmanager.com/gtm.js?id='+i+dl;f.parentNode.insertBefore(j,f); })
                                            (window,document,'script','dataLayer','%GTMCONTAINERID%');
                                      </script>";

        public static string BodyTrackingScript = @"<!-- NS Google Tag Manager (noscript) -->
                                        <noscript><iframe src=""https://www.googletagmanager.com/ns.html?id=%GTMCONTAINERID%""
                                        height=""0"" width=""0"" style=""display:none;visibility:hidden""></iframe></noscript>
                                        <!-- End Google Tag Manager (noscript) -->";

        public static string BaseEventScript = @"
                             'event': '%event_name%', 
                             'var_prodid': [%product_ids%],
                             'var_pagetype' : '%page_type%',
                             'var_prodval':%value%,
                             'ecommerce':{
                                            'currency': '%currency%',
                                            'value': %value%,
                                            'items': [%productInformation%] 
                                         }";

        public static string SessionKey => "NopStation.GMTSession{0}";

        #region Events

        public static string REMOVE_TO_CART => "remove_from_cart";

        public static string SEARCH => "search";

        public static string CATEGORY_VIEW => "category";

        public static string CUSTOMER_REGISTER => "customer_registered";

        public static string VIEW_ITEM => "view_item";

        public static string HOME_PAGE => "home_page_visit";

        public static string VIEW_CART => "view_cart";

        public static string PRODUCT => "product";

        public static string BEGIN_CHECKOUT => "begin_checkout";

        public static string CHECKOUT_PAGE => "checkout";

        public static string CART_PAGE => "cart";

        public static string PURCHASE => "purchase";

        public static string VIEW_ITEM_LIST => "view_item_list";

        public static string CONTACT_US => "contact_us";

        #endregion
    }
}
