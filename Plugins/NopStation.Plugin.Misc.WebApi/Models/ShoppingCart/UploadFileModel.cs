using System;

namespace NopStation.Plugin.Misc.WebApi.Models.ShoppingCart
{
    public class UploadFileModel
    {
        public string DownloadUrl { get; set; }

        public Guid DownloadGuid { get; set; }
    }
}
