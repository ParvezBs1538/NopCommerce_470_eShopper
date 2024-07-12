using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models
{
    public record UploadProductModel
    {
        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.UploadProduct.FromId")]
        public int FromId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AlgoliaSearch.UploadProduct.ToId")]
        public int ToId { get; set; }
    }
}
