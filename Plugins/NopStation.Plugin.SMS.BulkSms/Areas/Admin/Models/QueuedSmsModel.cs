using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;

namespace NopStation.Plugin.SMS.BulkSms.Areas.Admin.Models
{
    public record QueuedSmsModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.BulkSms.QueuedSmss.Fields.Customer")]
        public int? CustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.QueuedSmss.Fields.Customer")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.QueuedSmss.Fields.Store")]
        public int StoreId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.QueuedSmss.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.QueuedSmss.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.QueuedSmss.Fields.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.QueuedSmss.Fields.SentTries")]
        public int SentTries { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.QueuedSmss.Fields.Error")]
        public string Error { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.QueuedSmss.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.BulkSms.QueuedSmss.Fields.SentOn")]
        public DateTime? SentOn { get; set; }
    }
}
