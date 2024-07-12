using Nop.Core;

namespace NopStation.Plugin.Payments.Quickstream.Domains
{
    public class AcceptedCard : BaseEntity
    {
        public string CardScheme { get; set; }

        public string CardType { get; set; }

        public int PictureId { get; set; }

        public double Surcharge { get; set; }

        public bool IsEnable { get; set; }
    }
}
