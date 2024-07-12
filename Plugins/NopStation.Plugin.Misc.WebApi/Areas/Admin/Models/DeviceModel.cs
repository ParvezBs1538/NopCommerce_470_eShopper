using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Models
{
    public record DeviceModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.WebApi.Devices.Fields.DeviceToken")]
        public string DeviceToken { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Devices.Fields.DeviceType")]
        public int DeviceTypeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Devices.Fields.DeviceType")]
        public string DeviceTypeStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Devices.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Devices.Fields.Customer")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Devices.Fields.Store")]
        public int StoreId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Devices.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Devices.Fields.SubscriptionId")]
        public string SubscriptionId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Devices.Fields.IsRegistered")]
        public bool IsRegistered { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Devices.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Devices.Fields.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }
    }
}
