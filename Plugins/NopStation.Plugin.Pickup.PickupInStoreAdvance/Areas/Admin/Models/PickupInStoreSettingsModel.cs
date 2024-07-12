using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models
{
    public record PickupInStoreSettingsModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Settings.ShowOrderStatusInOrderDetails")]
        public bool ShowOrderStatusInOrderDetails { get; set; }
        public bool ShowOrderStatusInOrderDetails_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Settings.AddOrderNote")]
        public bool AddOrderNote { get; set; }
        public bool AddOrderNote_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Settings.OrderNotesShowToCustomer")]
        public bool OrderNotesShowToCustomer { get; set; }

        public bool OrderNotesShowToCustomer_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Settings.NotifyCustomerIfReady")]
        public bool NotifyCustomerIfReady { get; set; }

        public bool NotifyCustomerIfReady_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PickupInStoreAdvance.Template.Edit")]
        public int TemplateId { get; set; }

        public StorePickupPointSearchModel StorePickupPointSearchModel { get; set; }
    }
}
