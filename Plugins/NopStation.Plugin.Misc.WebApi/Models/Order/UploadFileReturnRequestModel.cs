using System;

namespace NopStation.Plugin.Misc.WebApi.Models.Order
{
    public class UploadFileReturnRequestModel
    {
        public string DownloadUrl { get; set; }

        public Guid DownloadGuid { get; set; }
    }
}
