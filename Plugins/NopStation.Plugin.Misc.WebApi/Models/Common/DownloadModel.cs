using System;

namespace NopStation.Plugin.Misc.WebApi.Models.Common
{
    public class DownloadModel
    {
        public bool HasUserAgreement { get; set; }

        public Guid OrderItemId { get; set; }

        public bool Redirect { get; set; }

        public string DownloadUrl { get; set; }
    }
}
