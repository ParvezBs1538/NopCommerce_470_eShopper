using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models
{
    public record WebAppDeviceModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.PWA.WebAppDevices.Fields.PushEndpoint")]
        public string PushEndpoint { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.WebAppDevices.Fields.PushP256DH")]
        public string PushP256DH { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.WebAppDevices.Fields.PushAuth")]
        public string PushAuth { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.WebAppDevices.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.WebAppDevices.Fields.Customer")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.WebAppDevices.Fields.Strore")]
        public int StoreId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.WebAppDevices.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.WebAppDevices.Fields.VapidPublicKey")]
        public string VapidPublicKey { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.WebAppDevices.Fields.VapidPrivateKey")]
        public string VapidPrivateKey { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PWA.WebAppDevices.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}
