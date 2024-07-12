using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Common;

namespace NopStation.Plugin.Widgets.AffiliateStation.Models
{
    public record AffiliateInfoModel : BaseNopEntityModel
    {
        public AffiliateInfoModel()
        {
            Address = new AffiliateAddressModel();
        }

        [NopResourceDisplayName("NopStation.AffiliateStation.Account.URL")]
        public string Url { get; set; }

        [NopResourceDisplayName("NopStation.AffiliateStation.Account.FriendlyUrlName")]
        public string FriendlyUrlName { get; set; }

        public AffiliateAddressModel Address { get; set; }

        public bool AlreadyApplied { get; set; }

        public ApplyStatus ApplyStatus { get; set; }

        public string WarningText { get; set; }

        public bool CanUpdateInfo { get; set; }

        public record AffiliateAddressModel : AddressModel
        {

        }
    }
}
