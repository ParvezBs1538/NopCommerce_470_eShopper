using System.Collections.Generic;

namespace NopStation.Plugin.Payments.DBBL.Models
{
    public class CardListModel
    {
        public CardListModel()
        {
            AvailableCards = new List<CardModel>();
        }

        public IList<CardModel> AvailableCards { get; set; }

        public class CardModel
        {
            public string DisplayName { get; set; }

            public string Value { get; set; }

            public string LogoUrl { get; set; }
        }
    }
}
