using Nop.Core;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Payments.MPay24.Domains
{
    public partial class PaymentOption : BaseEntity, IStoreMappingSupported
    {
        public string PaymentType { get; set; }

        public string Brand { get; set; }

        public string DisplayName { get; set; }

        public string ShortName { get; set; }

        public int PictureId { get; set; }

        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool Active { get; set; }

        public bool LimitedToStores { get; set; }
    }
}
