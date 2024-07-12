using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.Quickstream.Areas.Admin.Models
{
    public record AcceptedCardModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.QuickStream.AcceptedCards.Fields.CardScheme")]
        public string CardScheme { get; set; }

        [NopResourceDisplayName("Admin.NopStation.QuickStream.AcceptedCards.Fields.CardType")]
        public string CardType { get; set; }

        [NopResourceDisplayName("Admin.NopStation.QuickStream.AcceptedCards.Fields.PictureId")]
        [UIHint("Picture")]
        public int PictureId { get; set; }
        public string PictureUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.QuickStream.AcceptedCards.Fields.Surcharge")]
        public double Surcharge { get; set; }

        [NopResourceDisplayName("Admin.NopStation.QuickStream.AcceptedCards.Fields.IsEnable")]
        public bool IsEnable { get; set; }
    }
}
