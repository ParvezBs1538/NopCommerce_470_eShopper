using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models
{
    public record AjaxFilterSpecificationAttributeModel : BaseNopEntityModel
    {
        public int SpecificationId { get; set; }
        public string SpecificationAttributeName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.MaxSpecificationAttributesToDisplay")]
        public int MaxSpecificationAttributesToDisplay { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseSpecificationAttributeByDefault")]
        public bool CloseSpecificationAttributeByDefault { get; set; }

        public string AlternateName { get; set; }

        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.HideProductCount")]
        public bool HideProductCount { get; set; }
    }
}
