using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;

namespace NopStation.Plugin.SMS.Vonage.Areas.Admin.Models
{
    public record QueuedSmsModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.VonageSms.QueuedSmss.Fields.Customer")]
        public int? CustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VonageSms.QueuedSmss.Fields.Customer")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VonageSms.QueuedSmss.Fields.Store")]
        public int StoreId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VonageSms.QueuedSmss.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VonageSms.QueuedSmss.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VonageSms.QueuedSmss.Fields.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VonageSms.QueuedSmss.Fields.SentTries")]
        public int SentTries { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VonageSms.QueuedSmss.Fields.Error")]
        public string Error { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VonageSms.QueuedSmss.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VonageSms.QueuedSmss.Fields.SentOn")]
        public DateTime? SentOn { get; set; }
    }
}
