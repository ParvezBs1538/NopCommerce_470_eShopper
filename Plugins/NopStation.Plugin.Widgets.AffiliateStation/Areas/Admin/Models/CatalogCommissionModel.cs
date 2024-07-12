using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models
{
    public record CatalogCommissionModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.Entity")]
        public int EntityId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.Entity")]
        public string EntityName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.CommissionAmount")]
        public decimal CommissionAmount { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.UsePercentage")]
        public bool UsePercentage { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.CommissionPercentage")]
        public decimal CommissionPercentage { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.Commission")]
        public string Commission { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }

        public string ViewPath { get; set; }
    }
}
