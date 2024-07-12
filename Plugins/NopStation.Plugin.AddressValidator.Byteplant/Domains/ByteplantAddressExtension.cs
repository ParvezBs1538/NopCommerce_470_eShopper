using Nop.Core;

namespace NopStation.Plugin.AddressValidator.Byteplant.Domains
{
    public class ByteplantAddressExtension : BaseEntity
    {
        public int AddressId { get; set; }

        public string FormattedAddress { get; set; }
    }
}
