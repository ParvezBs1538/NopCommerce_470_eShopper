namespace NopStation.Plugin.Tax.TaxJar
{
    public class LineItem
    {
        public string id { get; set; }
        public decimal discount { get; set; }
        public int quantity { get; set; }
        public string product_identifier { get; set; }
        public string description { get; set; }
        public string product_tax_code { get; set; }
        public decimal unit_price { get; set; }
        public decimal sales_tax { get; set; }
    }
}
