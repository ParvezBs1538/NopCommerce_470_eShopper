using System.Collections.Generic;

namespace NopStation.Plugin.Misc.WebApi.Models.ShoppingCart
{
    public class CartAttributeChangeModel
    {
        public CartAttributeChangeModel()
        {
            EnabledAttributeMappingIds = new List<int>();
            DisabledAttributeMappingIds = new List<int>();
        }

        public int ProductId { get; set; }

        public string Gtin { get; set; }

        public string Mpn { get; set; }

        public string Sku { get; set; }

        public string Price { get; set; }

        public string BasePricePangv { get; set; }

        public string StockAvailability { get; set; }

        public IList<int> EnabledAttributeMappingIds { get; set; }

        public IList<int> DisabledAttributeMappingIds { get; set; }
        
        public IList<int> PictureIds { get; set; }

        public string PictureFullSizeUrl { get; set; }

        public string PictureDefaultSizeUrl { get; set; }

        public bool IsFreeShipping { get; set; }
    }
}
