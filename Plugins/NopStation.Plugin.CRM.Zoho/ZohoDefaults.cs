using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NopStation.Plugin.CRM.Zoho
{
    public static class ZohoDefaults
    {
        public static string PluginDirectory => "NopStation.Plugin.CRM.Zoho";

        public static string UsersUrl => "{0}users";

        public static string AccountsUrl => "{0}Accounts";

        public static string VendorsUrl => "{0}Vendors";

        public static string ContactsUrl => "{0}Contacts";

        public static string ProductsUrl => "{0}Products";

        public static string SalesOrdersUrl => "{0}Sales_Orders";

        public static string ModulesUrl => "{0}settings/modules";

        public static string TokenUrl => "{0}oauth/v2/token";

        public static string OAuthApiVersion => "v2";

        public static string Scopes = "ZohoCRM.modules.ALL,ZohoCRM.users.ALL,ZohoCRM.settings.ALL";

        public static IList<KeyValuePair<string, string>> SyncTables = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("Stores", "Stores"),
            new KeyValuePair<string, string>("Vendors", "Vendors"),
            new KeyValuePair<string, string>("Customers", "Customers"),
            new KeyValuePair<string, string>("Products", "Products"),
            new KeyValuePair<string, string>("Orders", "Orders"),
            new KeyValuePair<string, string>("Shipments", "Shipments"),
            new KeyValuePair<string, string>("ShipmentItems", "Shipment Items")
        };

        public static IList<SelectListItem> ToListItems(this IList<KeyValuePair<string, string>> pairs)
        {
            return pairs.Select(x => new SelectListItem()
            {
                Text = x.Value,
                Value = x.Key
            }).ToList();
        }

        public static string DataSynchronizationTaskType => "NopStation.Plugin.CRM.Zoho.DataSynchronizationTask";
    }
}
