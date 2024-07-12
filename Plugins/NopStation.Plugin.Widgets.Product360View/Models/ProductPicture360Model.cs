using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Product360View.Models
{
    public record ProductPicture360Model : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.ProductId")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.PictureId")]
        [UIHint("MultiPicture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.PictureId")]
        public string PictureUrl { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.OverrideAltAttribute")]
        public string OverrideAltAttribute { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Product360View.Fields.OverrideTitleAttribute")]
        public string OverrideTitleAttribute { get; set; }
        public bool IsPanorama { get; set; }
    }
}
