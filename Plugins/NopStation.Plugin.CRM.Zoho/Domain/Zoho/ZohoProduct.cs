using System;

namespace NopStation.Plugin.CRM.Zoho.Domain.Zoho
{
    public class ZohoProduct : BaseZohoEntity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int VendorId { get; set; }

        public int TaxCategoryId { get; set; }

        public DateTime? AvailableStartDate { get; set; }

        public DateTime? AvailableEndDate { get; set; }

        public bool Published { get; set; }

        public string Sku { get; set; }

        public int StockQuantity { get; set; }

        public decimal Price { get; set; }

        public bool IsTaxExempt { get; set; }
    }
}
